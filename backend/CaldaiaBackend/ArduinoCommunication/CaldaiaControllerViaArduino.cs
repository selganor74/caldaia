using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

using Newtonsoft.Json;
using Infrastructure.DomainEvents;
using Infrastructure.Logging;

using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Events;
using CaldaiaBackend.Application.Services;

namespace ArduinoCommunication
{
    internal enum ParsingState
    {
        SearchingStart,
        SearchingEnd
    }

    public class CaldaiaControllerViaArduino : 
        IDisposable, 
        IArduinoDataReader, 
        IArduinoCommandIssuer
    {
        private static readonly TimeSpan CommandToResponseTimeout = TimeSpan.FromSeconds(15);

        private static readonly TimeSpan TimeoutBeforeResettingParsingFailures = CommandToResponseTimeout.Add(CommandToResponseTimeout);
        private const int NumberOfJsonParsingFailuresBeforeReset = 10;
        private static bool _recovering = false;
        private readonly Timer _commandToResponseTimeoutTimer;
        private readonly Timer _commandSender;
        private readonly Queue<string> _commandQueue = new Queue<string>(12);

        private readonly MultipleStringToJsonParser _streamingJsonParser;

        private readonly string _serialPort;
        private Timer _failedJsonResetTimeout;
        private SerialPort _physicalPort;

        private event Action<DataFromArduino> Observers;
        private event Action<SettingsFromArduino> SettingsObservers;
        private event Action<string> RawDataObservers;

        private IEventDispatcher _dispatcher;
        private readonly ILogger _log;
        private bool _dequeuing = false;
        private bool _sendingOrReceiving = false;
        private int _failedJsonCounter = 0;

        public DataFromArduino Latest { get; private set; } = new DataFromArduino();
        public SettingsFromArduino LatestSettings { get; set; }

        /// <summary>
        /// Builds a new CaldaiaControllerViaArduino
        /// </summary>
        /// <param name="serialPort">Eg.: COM5</param>
        /// <param name="loggerFactory">a logger Factory</param>
        public CaldaiaControllerViaArduino(
            string serialPort,
            IEventDispatcher dispatcher,
            ILoggerFactory loggerFactory)
        {
            _serialPort = serialPort;
            _dispatcher = dispatcher;
            _failedJsonResetTimeout = new Timer(state => ResetFailedJsonCounter(), null, -1, -1);
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
            _commandToResponseTimeoutTimer = new Timer(TryRecoverConnection, null, -1, -1);
            _commandSender = new Timer(DequeueCommand, null, -1, -1);

            _streamingJsonParser = new MultipleStringToJsonParser(loggerFactory);
            _streamingJsonParser.foundNewJson += FoundNewJson;
        }

        public void Start()
        {
            TryToRecover();
        }

        public Action RegisterObserver(Action<DataFromArduino> observer)
        {
            Observers += observer;
            return () => { Observers -= observer; };
        }

        public Action RegisterSettingsObserver(Action<SettingsFromArduino> observer)
        {
            SettingsObservers += observer;
            return () => { SettingsObservers -= observer; };
        }

        public Action RegisterRawDataObserver(Action<string> observer)
        {
            RawDataObservers += observer;
            return () => { RawDataObservers -= observer; };
        }

        private void ResetFailedJsonCounter()
        {
            _log.Warning($"Resetting FailedJsonCounter from [{_failedJsonCounter}] to 0");
            _failedJsonCounter = 0;
        }

        private void SetupSerialPort()
        {
            try
            {
                _physicalPort?.Dispose();
            }
            catch (Exception e)
            {
                /* We don't care about these errors */
                _log.Warning("Exception while disposing serial", e);
            }

            _physicalPort = new SerialPort(_serialPort, 9600);
            _physicalPort.Handshake = Handshake.XOnXOff;
            //_physicalPort.ReceivedBytesThreshold = 1;
            _physicalPort.DataReceived += PhysicalPort_DataReceived;
            _physicalPort.DtrEnable = false;
            _physicalPort.ReadBufferSize = 64;
            //_physicalPort.ReadTimeout = 500;
            //_physicalPort.WriteTimeout = 500;

            _physicalPort.Open();
            //_physicalPort.SetFlag(COMConstants.FERRORCHAR, 0);
            //_physicalPort.SetFlag(COMConstants.FPARITY, 0);
            _physicalPort.SetFlag(COMConstants.FDTRCONTROL, 0);
            //_physicalPort.SetFlag(COMConstants.FRTSCONTROL, 0);
            _physicalPort.SetField("BaudRate", (UInt32)9600);
            _physicalPort.SetField("Parity", (byte) 0);
            _physicalPort.SetField("ByteSize", (byte) 8);
            _physicalPort.SetField("StopBits", (byte)1);
            _physicalPort.SetField("Parity", (byte)0);
            _physicalPort.SetField("XonLim", (ushort)2048);
            _physicalPort.SetField("XoffLim", (ushort)512);
            _physicalPort.SetField("XonChar", (byte)0x11);
            _physicalPort.SetField("XoffChar", (byte)0x1a);

        }

        private void TryRecoverConnection(object state)
        {
            _log.Warning("Timeout elapsed without response. Trying to recover connection");

            // FlashDTR();
            TryToRecover();
        }

        public void FlashDTR()
        {
            try
            {
                _physicalPort.DtrEnable = true;
                Thread.Sleep(450);
                _physicalPort.DtrEnable = false;
            }
            catch (Exception e)
            {
                _log.Warning("Errors while trying to flash DTR.", e);
            }
        }

        private void FoundNewJson(string _currentJson)
        {
            _log.Trace("Found full json", _currentJson);

            try
            {
                if (_currentJson.Contains("\"_type\": \"settings\""))
                {
                    // Settings
                    LatestSettings = JsonConvert.DeserializeObject<SettingsFromArduino>(_currentJson);
                    NotifySettingsObservers(LatestSettings);
                    return;
                }

                if (_currentJson.Contains("\"_type\": \"data\"")
                    || _currentJson.Contains("\"_type\": \"accumulators\"")
                       )
                {
                    Latest = JsonConvert.DeserializeObject<DataFromArduino>(_currentJson);
                    NotifyObservers(Latest);
                    if (_currentJson.Contains("\"_type\": \"accumulators\""))
                    {
                        _dispatcher.Dispatch(AccumulatorsReceived.FromData(Latest));
                    }

                    _dispatcher.Dispatch(TemperaturesReceived.FromData(Latest));
                    return;
                }

                _log.Warning("This was not an expeced json type", _currentJson);
                throw new Exception("This was not an expected json type");
            }
            catch (Exception x)
            {
                _failedJsonCounter++;
                _failedJsonResetTimeout.Change(
                    TimeoutBeforeResettingParsingFailures,
                    Timeout.InfiniteTimeSpan
                    );

                _log.Warning($"[{_failedJsonCounter}] Errors while parsing {_currentJson}", x);

                if (_failedJsonCounter >= NumberOfJsonParsingFailuresBeforeReset)
                {
                    _log.Error(
                        $"[{_failedJsonCounter} > {NumberOfJsonParsingFailuresBeforeReset}] " +
                        $"Resetting Board via FlashDTR "
                        );
                    ResetFailedJsonCounter();
                    _failedJsonResetTimeout.Change(Timeout.Infinite, Timeout.Infinite);
                    FlashDTR();
                }
            }
        }

        private void PhysicalPort_DataReceived(
            object sender,
            SerialDataReceivedEventArgs e
            )
        {
            _sendingOrReceiving = true;
            CancelTimeoutCheck();

            var readData = "";
            if (e.EventType == SerialData.Eof) return;
            try
            {
                readData += _physicalPort.ReadExisting();

                _log.Trace("DataReceived", readData);

                NotifyRawDataObservers(readData);

                _streamingJsonParser.AddString(readData);

            }
            catch (Exception ex)
            {
                _log.Error("DataReceived error", ex);
                throw;
            }
            finally
            {
                _sendingOrReceiving = false;
            }
        }

        private void CancelTimeoutCheck()
        {
            _log.Trace("Stopping timeout for command.");
            _commandToResponseTimeoutTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private void StartTimeoutCheck()
        {
            _log.Trace("Starting timeout for command.");
            _commandToResponseTimeoutTimer.Change(CommandToResponseTimeout, Timeout.InfiniteTimeSpan);
        }

        public void PullOutData()
        {
            EnqueueCommand("GET\r");
        }

        public void SendGetAndResetAccumulatorsCommand()
        {
            EnqueueCommand("GET-RA\r");
        }

        public void PullOutSettings()
        {
            EnqueueCommand("GET-RS\r");
        }

        public void IncrementRotexTermoMax()
        {
            EnqueueCommand("+RTM\r");
        }

        public void DecrementRotexTermoMax()
        {
            EnqueueCommand("-RTM\r");
        }

        public void DecrementRotexTermoMin()
        {
            EnqueueCommand("-RTm\r");
        }

        public void IncrementRotexTermoMin()
        {
            EnqueueCommand("+RTm\r");
        }

        public void SaveSettings()
        {
            EnqueueCommand("ST-RS\r");
        }

        public void SendString(string toSend)
        {
            EnqueueCommand(toSend + "\r");
        }

        private void EnqueueCommand(string command)
        {
            _commandQueue.Enqueue(command);
            StartDequeuingTask();
        }

        private void StartDequeuingTask()
        {
            if (_dequeuing) return;
            _commandSender.Change(0, -1);
        }

        private void DequeueCommand(object state)
        {
            _dequeuing = true;
            _log.Trace("Starting Command Dequeue");
            while (_commandQueue.Count > 0)
            {
                if (!_sendingOrReceiving)
                {
                    var toSend = _commandQueue.Dequeue();
                    _log.Trace("Sending string: " + toSend);
                    WriteString(toSend);
                }

                Thread.Sleep(500);
            }

            _log.Trace("Queue emptied");
            _commandSender.Change(-1, -1);
            _dequeuing = false;
        }

        private void WriteString(string toWrite)
        {
            if (_recovering) return;
            _sendingOrReceiving = true;


            try
            {
                _physicalPort.Write(toWrite);
                StartTimeoutCheck();
            }
            catch (Exception)
            {
                TryToRecover();
                _physicalPort.Write(toWrite);
                StartTimeoutCheck();
            }
            finally
            {
                _sendingOrReceiving = false;
            }
        }

        private void TryToRecover()
        {
            int trial = 0;

            bool thrownException;
            do
            {
                try
                {
                    trial = trial == Int32.MaxValue ? 0 : trial;
                    trial++;
                    _log.Info($"Recovering Connection to {_serialPort}. Attempt {trial}");
                    SetupSerialPort();
                    thrownException = false;
                }
                catch (Exception e)
                {
                    thrownException = true;
                    _log.Warning("Unable to recover Connection. Trying again in 2 seconds", e);
                    Thread.Sleep(2000);
                }
            } while (thrownException);
        }

        public void Dispose()
        {
            _streamingJsonParser?.Dispose();
            _physicalPort?.Dispose();
        }

        protected virtual void NotifyObservers(DataFromArduino data)
        {
            Observers?.Invoke(data);
        }

        protected virtual void NotifySettingsObservers(SettingsFromArduino data)
        {
            SettingsObservers?.Invoke(data);
        }

        protected virtual void NotifyRawDataObservers(string raw)
        {
            RawDataObservers?.Invoke(raw);
        }
    }
}

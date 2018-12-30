using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Events;
using CaldaiaBackend.Application.Services;
using Infrastructure.DomainEvents;
using Infrastructure.Logging;
using Newtonsoft.Json;

namespace ArduinoCommunication
{
    public class CaldaiaControllerViaArduino : IDisposable, IArduinoDataReader, IArduinoCommandIssuer
    {
        private const int CommandToResponseTimeoutMillis = 15000;
        private const int NumberOfJsonParsingFailuresBeforeReset = 10;
        private static readonly TimeSpan SecondsBeforeResettingNumberOfsonParsingFailures = TimeSpan.FromSeconds(30);
        private static bool _recovering = false;
        private readonly Timer _commandToResponseTimeoutTimer;
        private readonly Timer _commandSender;
        private readonly Queue<string> _commandQueue = new Queue<string>(12);

        private readonly string _serialPort;
        private Timer _timer;
        private Timer _failedJsonResetTimeout;
        private SerialPort _physicalPort;
        private string _currentJson;
        private readonly Queue<string> _readQueue = new Queue<string>(1024);

        private event Action<DataFromArduino> Observers;
        private event Action<SettingsFromArduino> SettingsObservers;
        private event Action<string> RawDataObservers;

        private string _parsingState = "searchingStart";
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
        }

        public void Start()
        {
            _timer = new Timer(
                state => ParseReadString(),
                null,
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(1)
            );

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
            catch (Exception)
            {
                /* We don't care about these errors */
            }

            _physicalPort = new SerialPort(_serialPort, 9600);
            _physicalPort.ReceivedBytesThreshold = 1;
            _physicalPort.DataReceived += PhysicalPort_DataReceived;
            _physicalPort.DtrEnable = false;
            _physicalPort.ReadTimeout = 500;
            _physicalPort.WriteTimeout = 500;

            _physicalPort.Open();
        }

        private void TryRecoverConnection(object state)
        {
            _log.Warning("Timeout elapsed without response. Trying to recover connection");

            FlashDTR();
            TryToRecover();
            FlashDTR();
        }

        public void FlashDTR()
        {
            try
            {
                _physicalPort.DtrEnable = true;
                Thread.Sleep(500);
                _physicalPort.DtrEnable = false;
            }
            catch (Exception e)
            {
                _log.Warning("Errors while trying to flash DTR.", e);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ParseReadString()
        {
            if (_readQueue.Count == 0) return;

            while (_readQueue.Count > 0)
            {
                var current = _readQueue.Dequeue();
                if (current == null) continue;

                NotifyRawDataObservers(current);

                while (current.Length != 0)
                {
                    switch (_parsingState)
                    {
                        case "searchingStart":
                            {
                                var startPos = current.IndexOf('{');
                                if (startPos != -1)
                                {
                                    current = current.Substring(startPos);
                                    _parsingState = "searchingEnd";
                                }

                                if (startPos == -1)
                                    current = "";
                                break;
                            }

                        case "searchingEnd":
                            {
                                var endPos = current.LastIndexOf('}');

                                if (endPos == -1)
                                {
                                    _currentJson += current;
                                    current = "";
                                }

                                if (endPos != -1)
                                {
                                    _currentJson += current.Substring(0, endPos + 1);
                                    current = current.Substring(endPos + 1);

                                    FoundNewJson();

                                    _currentJson = current;
                                    current = "";
                                    _parsingState = "searchingStart";
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void FoundNewJson()
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

                if (   _currentJson.Contains("\"_type\": \"data\"") 
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
                }
            }
            catch (Exception x)
            {
                _failedJsonCounter++;
                _failedJsonResetTimeout.Change(SecondsBeforeResettingNumberOfsonParsingFailures.Milliseconds, -1);

                _log.Warning($"[{_failedJsonCounter}] Errors while parsing {_currentJson}", x);

                if (_failedJsonCounter >= NumberOfJsonParsingFailuresBeforeReset)
                {
                    _log.Error(
                        $"[{_failedJsonCounter} > {NumberOfJsonParsingFailuresBeforeReset}] " +
                        $"Resetting Board via FlashDTR "
                        );
                    ResetFailedJsonCounter();
                    _failedJsonResetTimeout.Change(-1, -1);
                    FlashDTR();
                }
            }
        }

        private void PhysicalPort_DataReceived(
            object sender,
            SerialDataReceivedEventArgs e
            )
        {
            CancelTimeoutCheck();

            _sendingOrReceiving = true;

            var readData = "";
            if (e.EventType == SerialData.Eof) return;
            try
            {
                readData += _physicalPort.ReadExisting();

                _log.Trace("DataReceived", readData);
                _readQueue.Enqueue(readData);
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
            _commandToResponseTimeoutTimer.Change(-1, -1);
        }

        private void StartTimeoutCheck()
        {
            _log.Trace("Starting timeout for command.");
            _commandToResponseTimeoutTimer.Change(CommandToResponseTimeoutMillis, -1);
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

            StartTimeoutCheck();

            try
            {
                _physicalPort.Write(toWrite);
            }
            catch (Exception)
            {
                TryToRecover();
                _physicalPort.Write(toWrite);
            }
            finally
            {
                _sendingOrReceiving = false;
            }
        }

        private void TryToRecover()
        {
            Int32 trial = 0;

            bool thrownException = true;
            while (thrownException && trial < Int32.MaxValue)
            {
                try
                {
                    trial++;
                    _log.Info($"Recovering Connection to {_serialPort}. Attempt {trial}");
                    SetupSerialPort();
                    thrownException = false;
                }
                catch (Exception e)
                {
                    _log.Warning("Unable to recover Connection. Trying again in 2 seconds", e);
                    Thread.Sleep(2000);
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
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

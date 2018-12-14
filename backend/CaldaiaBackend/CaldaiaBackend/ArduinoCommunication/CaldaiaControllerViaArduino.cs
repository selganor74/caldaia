using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Interfaces;
using Infrastructure.Logging;
using Newtonsoft.Json;

namespace CaldaiaBackend.ArduinoCommunication
{
    public class CaldaiaControllerViaArduino : IDisposable, IArduinoDataReader, IArduinoCommandIssuer
    {
        private static bool _recovering = false;

        private readonly string _serialPort;
        private Timer _timer;
        private SerialPort _physicalPort;
        private string _currentJson;
        private readonly Queue<string> _readQueue = new Queue<string>(1024);

        private event Action<DataFromArduino> Observers;
        private event Action<SettingsFromArduino> SettingsObservers;

        private string _parsingState = "searchingStart";
        private ILogger _log;

        public DataFromArduino Latest { get; private set; } = new DataFromArduino();
        public SettingsFromArduino LatestSettings { get; set; }

        /// <summary>
        /// Builds a new CaldaiaControllerViaArduino
        /// </summary>
        /// <param name="serialPort">Eg.: COM5</param>
        /// <param name="loggerFactory">a logger Factory</param>
        public CaldaiaControllerViaArduino(
            string serialPort,
            ILoggerFactory loggerFactory
            )
        {
            _serialPort = serialPort;
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
        }

        public void Start()
        {
            SetupSerialPort();

            _timer = new Timer(
                state => ParseReadString(),
                null,
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(1)
                );
        }

        private void SetupSerialPort()
        {
            try
            {
                _physicalPort?.Dispose();
            }
            catch (Exception)
            {
            }

            _physicalPort = new SerialPort(_serialPort, 9600);
            _physicalPort.ReceivedBytesThreshold = 1;
            _physicalPort.DataReceived += PhysicalPort_DataReceived;
            _physicalPort.DtrEnable = false;
            _physicalPort.ReadTimeout = 500;
            _physicalPort.WriteTimeout = 500;

            _physicalPort.Open();
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


        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ParseReadString()
        {
            if (_readQueue.Count == 0) return;

            while (_readQueue.Count > 0)
            {
                var current = _readQueue.Dequeue();
                if (current == null) continue;

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
                                var endPos = current.IndexOf('}');

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
            _log.Info("Found full json", _currentJson);

            try
            {
                if (_currentJson.Contains("rotexTermoMin"))
                {
                    // Settings
                    LatestSettings = JsonConvert.DeserializeObject<SettingsFromArduino>(_currentJson);
                    NotifySettingsObservers(LatestSettings);
                    return;
                }

                Latest = JsonConvert.DeserializeObject<DataFromArduino>(_currentJson);
                NotifyObservers(Latest);
            }
            catch (Exception x)
            {
                _log.Warning($"Errors while parsing {_currentJson}", x);
            }
        }

        private void PhysicalPort_DataReceived(
            object sender,
            SerialDataReceivedEventArgs e
            )
        {
            var readData = "";
            if (e.EventType == SerialData.Eof) return;
            try
            {
                readData += _physicalPort.ReadExisting();

                _log.Info("DataReceived", readData);
                _readQueue.Enqueue(readData);
            }
            catch (Exception ex)
            {
                _log.Error("DataReceived error", ex);
                throw;
            }
        }

        public void SendGetCommand()
        {
            WriteString("GET\r");
        }

        public void SendGetAndResetAccumulatorsCommand()
        {
            WriteString("GET-RA\r");
        }

        public void SendGetRunTimeSettingsCommand()
        {
            WriteString("GET-RS\r");
        }

        private void WriteString(string toWrite)
        {
            if (_recovering) return;
            try
            {
                _physicalPort.Write(toWrite);
            }
            catch (Exception)
            {
                TryToRecover();
                _physicalPort.Write(toWrite);
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
                    _log.Warning("Unable to recover Connection.", e);
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

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Interfaces;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CaldaiaBackend.ArduinoCommunication
{
    public class CaldaiaControllerViaArduino : IDisposable, IArduinoDataReader, IArduinoCommandIssuer
    {
        private readonly string _serialPort;
        private Timer _timer;
        private SerialPort _physicalPort;
        private string _currentJson;
        private readonly Queue<string> readQueue = new Queue<string>(10);

        private event Action<DataFromArduino> observers;
        private event Action<SettingsFromArduino> settingsObservers;

        private string _parsingState = "searchingStart";

        public DataFromArduino Latest { get; private set; } = new DataFromArduino();
        public SettingsFromArduino LatestSettings { get; set; }

        public CaldaiaControllerViaArduino(string serialPort = "COM5")
        {
            this._serialPort = serialPort;
        }

        public void Start()
        {
            _physicalPort = new SerialPort(_serialPort, 9600);
            _physicalPort.ReceivedBytesThreshold = 1;
            _physicalPort.DataReceived += PhysicalPort_DataReceived;
            _physicalPort.DtrEnable = false;
            _physicalPort.ReadTimeout = 500;
            _physicalPort.WriteTimeout = 500;

            _physicalPort.Open();

            _timer = new Timer((object state) => { ParseReadString(); });
            _timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
        }

        public Action RegisterObserver(Action<DataFromArduino> observer)
        {
            observers += observer;
            return () => { observers -= observer; };
        }

        public Action RegisterSettingsObserver(Action<SettingsFromArduino> observer)
        {
            settingsObservers += observer;
            return () => { settingsObservers -= observer; };
        }

        private void ParseReadString()
        {
            while (readQueue.Count > 0)
            {
                var current = readQueue.Dequeue();
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

                                    _currentJson = "";
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
            Trace.TraceInformation("Found full json: " + _currentJson);

            if (_currentJson.Contains("rotexTermoMin"))
            {
                // Settings
                LatestSettings = JsonConvert.DeserializeObject<SettingsFromArduino>(_currentJson);
                NotifySettingsObservers(LatestSettings);
                return;
            }

            try
            {
                Latest = JsonConvert.DeserializeObject<DataFromArduino>(_currentJson);
                NotifyObservers(Latest);
            }
            catch (Exception x)
            {
                Trace.TraceWarning($"Errors while parsing {_currentJson}: {x}");
            }
        }


        private void PhysicalPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var readData = "";
            if (e.EventType == SerialData.Eof) return;
            try
            {
                readData += _physicalPort.ReadExisting();

                Trace.TraceInformation("DataReceived: " + readData);
                readQueue.Enqueue(readData);
            }
            catch (Exception ex)
            {
                Trace.TraceError("DataReceived error: " + ex);
                throw;
            }
        }

        public void SendGetCommand()
        {
            _physicalPort.Write("GET\r");
        }

        public void SendGetAndResetAccumulatorsCommand()
        {
            _physicalPort.Write("GET-RA\r");
        }

        public void SendGetRunTimeSettingsCommand()
        {
            _physicalPort.Write("GET-RS\r");
        }

        public void Dispose()
        {
            _timer.Dispose();
            _physicalPort?.Dispose();
        }

        protected virtual void NotifyObservers(DataFromArduino data)
        {
            observers?.Invoke(data);
        }
        protected virtual void NotifySettingsObservers(SettingsFromArduino data)
        {
            settingsObservers?.Invoke(data);
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CaldaiaBackend
{
    public class CaldaiaControllerViaArduino : IDisposable
    {
        private readonly string _serialPort;
        private Timer _timer;
        private SerialPort _physicalPort;
        private string _currentJson;
        private readonly Queue<string> readQueue = new Queue<string>(10);
        private string _parsingState = "searchingStart";

        public DataFromArduino Latest { get; private set; } = new DataFromArduino();

        public CaldaiaControllerViaArduino(string serialPort = "COM5")
        {
            this._serialPort = serialPort;
        }

        public void Start()
        {
            _physicalPort = new SerialPort(_serialPort, 9600);
            _physicalPort.ReceivedBytesThreshold = 1;
            _physicalPort.DataReceived += PhysicalPort_DataReceived;
            _physicalPort.Open();

            _timer = new Timer((object state) => { ParseReadString(); });
            _timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(3.571113));
        }

        private void ParseReadString()
        {
            while (readQueue.Count > 0)
            {
                var current = readQueue.Dequeue();

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
            try
            {
                Latest = JsonConvert.DeserializeObject<DataFromArduino>(_currentJson);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Error parsing string: {0} - Exception: {1}", _currentJson, ex);
            }
        }

        private void PhysicalPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var readData = "";
            while (_physicalPort.BytesToRead > 0)
            {
                readData += _physicalPort.ReadByte();
            }

            readQueue.Enqueue(readData);
        }

        public void SendGetCommand()
        {
            _physicalPort.Write("GET");
        }

        public void Dispose()
        {
            _timer.Dispose();
            _physicalPort?.Dispose();
        }
    }
}

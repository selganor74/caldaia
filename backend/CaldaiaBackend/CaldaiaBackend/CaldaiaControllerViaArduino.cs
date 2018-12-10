using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaldaiaBackend
{
    public class CaldaiaControllerViaArduino : IDisposable
    {
        private string serialPort;
        private SerialPort physicalPort;

        public CaldaiaControllerViaArduino(string serialPort = "COM5")
        {
            this.serialPort = serialPort;
        }

        public void Start()
        {
            physicalPort = new SerialPort(serialPort, 9600);
            physicalPort.ReceivedBytesThreshold = 1;
            physicalPort.DataReceived += PhysicalPort_DataReceived;
            physicalPort.Open();
        }

        private void PhysicalPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var read = physicalPort.ReadExisting();
        }

        public void SendGetCommand()
        {
            physicalPort.Write("GET");
        }

        public void Dispose()
        {
            physicalPort?.Dispose();
        }
    }
}

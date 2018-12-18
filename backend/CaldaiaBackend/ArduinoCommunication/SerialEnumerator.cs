using System.Collections.Generic;
using System.IO.Ports;

namespace ArduinoCommunication
{
    public class SerialEnumerator
    {
        /// <summary>
        /// Will return a list of the currently installed and visible serial ports.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSerialPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}

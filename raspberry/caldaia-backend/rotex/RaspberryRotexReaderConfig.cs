using System.IO.Ports;

namespace rotex
{
    // Configuration parameters for the RaspberryRotexReader component  
    public class RaspberryRotexReaderConfig
    {
        public string SerialPortName { get; set; } = "/dev/ttyAMA0";
        public int BaudRate {get;set;} = 9600; // default for the Rotex Serial
        public IEnumerable<string> AvailableSerials => SerialPort.GetPortNames();
    }
}
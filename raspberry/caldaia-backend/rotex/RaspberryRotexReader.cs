using System.IO.Ports;
using application.infrastructure;
using application.services;
using Microsoft.Extensions.Logging;

namespace rotex;
public class RaspberryRotexReader : IStartable, IRotexReader, IDisposable
{
    private RaspberryRotexReaderConfig config;
    private SerialPort? serialPort;
    private readonly ILogger<RaspberryRotexReader> log;
    private Thread readThread;
    private bool isStarted = false;

    public RaspberryRotexReader(
        RaspberryRotexReaderConfig config,
        ILogger<RaspberryRotexReader> log
        )
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        this.config = config.DeepClone() ?? new RaspberryRotexReaderConfig();
        this.log = log;

        this.readThread = new Thread((obj) => this.SerialRead());
    }

    public object GetRotexConfig()
    {
        return this.config;
    }

    public void Start()
    {
        this.serialPort = new SerialPort(config.SerialPortName, config.BaudRate);

        // Set the read/write timeouts in milliseconds
        this.serialPort.ReadTimeout = 500;
        serialPort.WriteTimeout = 500;

        // this.serialPort.DataReceived += this.DataReceivedHandler;
        this.serialPort.Open();

        this.isStarted = true;
        this.readThread.Start();

        log.LogInformation($"{nameof(RaspberryRotexReader)} Started!");
    }

    // Legge effettivamente l'output dell Rotex
    // Appena accesa la centralina R3 restituisce questa stringa:
    //            SOLARIS R3 V.3.1 May 30 2007 14:37:58 ROTEX GmbH Ser.-Nr: 000001******
    // Successivamente:
    // L'output seriale della R3 è un record con i valori separati da ';'
    //      HA;BK;P1 [%];P2;TK [°C];TR [°C];TS [°C];TV [°C];V [l/min]
    // 
    // https://www.abelectronics.co.uk/kb/article/1035/serial-port-setup-in-raspberry-pi-os
    // https://pimylifeup.com/raspberry-pi-serial/
    // I pin collegati sono 
    // (pin 6)              - GND
    // (pin 8)  GPIO 14     - TXD 
    // (pin 10) GPIO 15     - RXD <- Abbiamo bisogno solo di questo 
    
    private void SerialRead()
    {
        while (isStarted)
        {
            try
            {
                var line = serialPort?.ReadLine();
                log.LogDebug($"{nameof(SerialRead)}: received data: {line}");
            }
            catch (TimeoutException)
            {
                // just wait for the next run
            }
        }
    }

    public void Stop()
    {
        this.isStarted = false;
        this.readThread.Join(); // waits for the thread to finish its job

        this.serialPort?.Close();
        this.serialPort?.Dispose();
        this.serialPort = null;
    }

    public void Dispose()
    {
        Stop();
    }
}

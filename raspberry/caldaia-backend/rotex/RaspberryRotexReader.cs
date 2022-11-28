using System.IO.Ports;
using application.infrastructure;
using application.services;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using Microsoft.Extensions.Logging;

namespace rotex;
public class RaspberryRotexReader : IStartable, IRotexReader, IDisposable
{
    private RaspberryRotexReaderConfig config;
    private SerialPort? serialPort;
    private readonly ILogger<RaspberryRotexReader> log;
    private Thread readThread;
    private bool isStarted = false;

    private MockAnalogInput<Temperature> _ROTEX_TEMPERATURA_PANNELLI { get; }
    public AnalogInput<Temperature> ROTEX_TEMPERATURA_PANNELLI => _ROTEX_TEMPERATURA_PANNELLI;
    
    private MockAnalogInput<Temperature> _ROTEX_TEMPERATURA_ACCUMULO { get; }    
    public AnalogInput<Temperature> ROTEX_TEMPERATURA_ACCUMULO => _ROTEX_TEMPERATURA_ACCUMULO;

    private MockDigitalInput _ROTEX_STATO_POMPA { get; }
    public DigitalInput ROTEX_STATO_POMPA => _ROTEX_STATO_POMPA;

    public RaspberryRotexReader(
        RaspberryRotexReaderConfig config,
        MockAnalogInput<Temperature> rOTEX_TEMPERATURA_PANNELLI,
        MockAnalogInput<Temperature> rOTEX_TEMPERATURA_ACCUMULO,
        MockDigitalInput rOTEX_STATO_POMPA,
        ILogger<RaspberryRotexReader> log
        )
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        this.config = config.DeepClone() ?? new RaspberryRotexReaderConfig();
        this.log = log;

        this.readThread = new Thread((obj) => this.SerialRead());
        _ROTEX_TEMPERATURA_PANNELLI = rOTEX_TEMPERATURA_PANNELLI;
        _ROTEX_TEMPERATURA_ACCUMULO = rOTEX_TEMPERATURA_ACCUMULO;
        _ROTEX_STATO_POMPA = rOTEX_STATO_POMPA;
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
    //            0        1         2         3         4         5         6         7
    //            1234567890123456789012345678901234567890123456789012345678901234567890
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
                ParseLine(line);
            }
            catch (TimeoutException)
            {
                // just wait for the next run
            }
        }
    }

    enum RotexField {
        HA = 0,
        BK = 1,
        P1_Percent = 2,
        P2 = 3,
        TK_Pannelli = 4,
        TR_Mandata = 5,
        TS_Accumulo = 6,
        TV_Ritorno = 7,
        V_Portata = 8
    }

    private void ParseLine(string? line)
    {
        // 0  1  2      3  4       5       6       7       8
        // HA;BK;P1 [%];P2;TK [°C];TR [°C];TS [°C];TV [°C];V [l/min]
        if (line is null)
            return;

        if(!line.Any(c => c == ';'))
            return;

        var components = line.Split(";");

        var tAccumulo = Decimal.Parse(components[(int)RotexField.TS_Accumulo]);
        _ROTEX_TEMPERATURA_ACCUMULO.SetInput(new Temperature(tAccumulo));

        var tPannelli = Decimal.Parse(components[(int)RotexField.TK_Pannelli]);
        _ROTEX_TEMPERATURA_PANNELLI.SetInput(new Temperature(tPannelli));

        var statoPompaRotex = Decimal.Parse(components[(int)RotexField.P1_Percent]) != 0;
        _ROTEX_STATO_POMPA.Set(statoPompaRotex ? OnOffState.ON : OnOffState.OFF);
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

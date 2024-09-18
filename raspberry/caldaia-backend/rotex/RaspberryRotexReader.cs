using System.ComponentModel;
using System.IO.Ports;
using application.infrastructure;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using Microsoft.Extensions.Logging;

namespace rotex;

public class RaspberryRotexReader : IDisposable
{
    private RaspberryRotexReaderConfig config;
    private SerialPort? serialPort;
    private readonly ILogger log;
    private BackgroundWorker readThread;
    private bool isStarted = false;

    private MockAnalogInput _ROTEX_TEMPERATURA_PANNELLI { get; }
    public AnalogInput ROTEX_TEMPERATURA_PANNELLI => _ROTEX_TEMPERATURA_PANNELLI;

    private MockAnalogInput _ROTEX_TEMPERATURA_ACCUMULO { get; }
    public AnalogInput ROTEX_TEMPERATURA_ACCUMULO => _ROTEX_TEMPERATURA_ACCUMULO;

    private MockDigitalInput _ROTEX_STATO_POMPA { get; }
    public DigitalInput ROTEX_STATO_POMPA => _ROTEX_STATO_POMPA;

    public RaspberryRotexReader(
        RaspberryRotexReaderConfig config,
        MockAnalogInput rOTEX_TEMPERATURA_PANNELLI,
        MockAnalogInput rOTEX_TEMPERATURA_ACCUMULO,
        MockDigitalInput rOTEX_STATO_POMPA,
        ILogger log
        )
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        this.config = config.DeepClone() ?? new RaspberryRotexReaderConfig();
        this.log = log;

        this.readThread = new BackgroundWorker();
        this.readThread.DoWork += (obj, evt) => this.SerialRead();

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
        serialPort.ReadTimeout = 500;
        serialPort.WriteTimeout = 500;
        serialPort.NewLine = "" + (char)0x0a;

        // this.serialPort.DataReceived += this.DataReceivedHandler;
        try
        {
            this.serialPort.Open();
        } catch (Exception ex)
        {
            log.LogError($"Unable to open Serial {config.SerialPortName} @ {config.BaudRate}. Rotex will not be available: {ex}");
            return;
        }

        this.isStarted = true;
        this.readThread.RunWorkerAsync();

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
            catch(Exception e)
            {
                log.LogWarning($"{nameof(SerialRead)}: error while reading data: {e}");
                Thread.Sleep(5000);
            }
        }
    }

    struct RotexField
    {
        public static readonly int HA = 0;
        public static readonly int BK = 1;
        public static readonly int P1_Percent = 2;
        public static readonly int P2 = 3;
        public static readonly int TK_Pannelli = 4;
        public static readonly int TR_Mandata = 5;
        public static readonly int TS_Accumulo = 6;
        public static readonly int TV_Ritorno = 7;
        public static readonly int V_Portata = 8;
    }

    private void ParseLine(string? line)
    {
        // 0  1  2      3  4       5       6       7       8
        // HA;BK;P1 [%];P2;TK [°C];TR [°C];TS [°C];TV [°C];V [l/min]
        if (line is null)
            return;

        if (!line.Any(c => c == ';'))
            return;

        var components = line.Split(";");

        try
        {
            var tAccumulo = Decimal.Parse(components[RotexField.TS_Accumulo]);
            _ROTEX_TEMPERATURA_ACCUMULO.SetInput(new Temperature(tAccumulo));
            log.LogDebug($"{nameof(ParseLine)}: read {nameof(tAccumulo)}: {tAccumulo}");
        }
        catch (Exception e)
        {
            log.LogError($"{nameof(_ROTEX_TEMPERATURA_ACCUMULO)}: Unable to parse {components[RotexField.TS_Accumulo]}");
        }

        try
        {
            var tPannelli = Decimal.Parse(components[RotexField.TK_Pannelli]);
            _ROTEX_TEMPERATURA_PANNELLI.SetInput(new Temperature(tPannelli));
            log.LogDebug($"{nameof(ParseLine)}: read {nameof(tPannelli)}: {tPannelli}");
        }
        catch (Exception e)
        {
            log.LogError($"{nameof(_ROTEX_TEMPERATURA_PANNELLI)}: Unable to parse {components[RotexField.TK_Pannelli]}");
        }

        try
        {
            var statoPompaRotex = Decimal.Parse(components[RotexField.P1_Percent]) != 0;
            _ROTEX_STATO_POMPA.Set(statoPompaRotex ? OnOffState.ON : OnOffState.OFF);
            log.LogDebug($"{nameof(ParseLine)}: read {nameof(statoPompaRotex)}: {statoPompaRotex}");
        }
        catch (Exception e)
        {
            log.LogError($"{nameof(_ROTEX_STATO_POMPA)}: Unable to parse {components[RotexField.P1_Percent]}");
        }
    }

    public void Stop()
    {
        this.isStarted = false;
        // this.readThread.CancelAsync(); // waits for the thread to finish its job

        this.serialPort?.Close();
        this.serialPort?.Dispose();
        this.serialPort = null;
    }

    public void Dispose()
    {
        Stop();
        this.readThread?.Dispose();
    }
}

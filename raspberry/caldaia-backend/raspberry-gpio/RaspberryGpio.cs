using System.Device.Gpio;
using Microsoft.Extensions.Logging;

namespace raspberry_gpio;

public static class DigitalInput_GPIO
{
    /// <summary>
    /// GPIO=13, PIN=33
    /// </summary>
    public static readonly int TERMOSTATO_AMBIENTI = 13;

    /// <summary>
    /// GPIO=6, PIN=31 
    /// </summary>
    public static readonly int TERMOSTATO_ROTEX = 6;
}

public static class RelayOutput_GPIO
{
    /// <summary>
    /// GPIO=17, PIN=11
    /// </summary>
    public static readonly int RELAY_CALDAIA = 17;

    /// <summary>
    /// GPIO=27, PIN=13
    /// </summary>
    public static readonly int RELAY_POMPA_CAMINO = 27;

    /// <summary>
    /// GPIO=22, PIN=15
    /// </summary>
    public static readonly int RELAY_BYPASS_TERMOSTATO = 22;

    /// <summary>
    /// GPIO=23, PIN=16
    /// </summary>
    public static readonly int RELAY_POMPA_RISCALDAMENTO = 23;

}

public class RaspberryGpio : IDisposable
{
    // private Thread blinker;
    private GpioController gpio;
    private bool isStarted;
    private readonly ILogger<RaspberryGpio> log;



    public RaspberryGpio(
        ILogger<RaspberryGpio> log
        )
    {
        // this.blinker = new Thread((obj) => this.Blinker());
        this.log = log;

        this.gpio = new GpioController();
    }

    private void SetRelayOn(int relay)
    {
        log.LogDebug($"Setting GPIO {relay} to On ...");
        this.gpio.Write(relay, 0);
        log.LogDebug($"... GPIO {relay} set to On");
    }

    private void SetRelayOff(int relay)
    {
        log.LogDebug($"Setting GPIO ({relay}) to Off ...");
        this.gpio.Write(relay, 1);
        log.LogDebug($"... {relay} set to Off");
    }

    public void Start()
    {
        try
        {
            log.LogDebug($"{nameof(Start)}: Setting up Relays");
            SetupRelays();

            log.LogDebug($"{nameof(Start)}: Setting up DigitalInputs");
            SetupDigitalInputs();

            isStarted = true;
            // this.blinker.Start();
            log.LogInformation($"{nameof(RaspberryGpio)} Started!");
        }
        catch (Exception e)
        {
            log.LogCritical($"{nameof(Start)} {e}");
            throw;
        }
    }

    public void Stop()
    {
        this.isStarted = false;
        // this.blinker.Join();
        log.LogInformation($"{nameof(RaspberryGpio)} Stopped!");
    }

    private void SetupDigitalInputs()
    {
        // Setup Input DIGITALI
        this.gpio.OpenPin((int)DigitalInput_GPIO.TERMOSTATO_AMBIENTI);
        this.gpio.SetPinMode((int)DigitalInput_GPIO.TERMOSTATO_AMBIENTI, PinMode.Input);

        this.gpio.OpenPin((int)DigitalInput_GPIO.TERMOSTATO_ROTEX);
        this.gpio.SetPinMode((int)DigitalInput_GPIO.TERMOSTATO_ROTEX, PinMode.Input);
    }

    private void SetupRelays()
    {
        // Setup RELAY
        this.gpio.OpenPin((int)RelayOutput_GPIO.RELAY_CALDAIA);
        this.gpio.SetPinMode((int)RelayOutput_GPIO.RELAY_CALDAIA, PinMode.Output);
        SetRelayOff(RelayOutput_GPIO.RELAY_CALDAIA);

        this.gpio.OpenPin((int)RelayOutput_GPIO.RELAY_POMPA_CAMINO);
        this.gpio.SetPinMode((int)RelayOutput_GPIO.RELAY_POMPA_CAMINO, PinMode.Output);
        SetRelayOff(RelayOutput_GPIO.RELAY_POMPA_CAMINO);

        this.gpio.OpenPin((int)RelayOutput_GPIO.RELAY_BYPASS_TERMOSTATO);
        this.gpio.SetPinMode((int)RelayOutput_GPIO.RELAY_BYPASS_TERMOSTATO, PinMode.Output);
        SetRelayOff(RelayOutput_GPIO.RELAY_BYPASS_TERMOSTATO);

        this.gpio.OpenPin((int)RelayOutput_GPIO.RELAY_POMPA_RISCALDAMENTO);
        this.gpio.SetPinMode((int)RelayOutput_GPIO.RELAY_POMPA_RISCALDAMENTO, PinMode.Output);
        SetRelayOff(RelayOutput_GPIO.RELAY_POMPA_RISCALDAMENTO);
    }

    //private void Blinker()
    //{
    //    try
    //    {
    //        var allPins = Enum.GetValues(typeof(RelayOutput_GPIO));
    //        var noOfPins = allPins.Length;
    //        var currIndex = 0;
    //        while (isStarted)
    //        {
    //            if (currIndex >= noOfPins)
    //                currIndex = 0;

    //            var currPin = (RelayOutput_GPIO)(allPins.GetValue(currIndex) ?? default(RelayOutput_GPIO));
    //            if ((int)currPin == 0)
    //            {
    //                log.LogDebug($"Null pin {currPin.ToString()} ({(int)currPin}) Skipping");
    //                currIndex++;
    //                continue;
    //            }

    //            SetRelayOn(currPin);

    //            Thread.Sleep(TimeSpan.FromSeconds(2));

    //            SetRelayOff(currPin);

    //            currIndex++;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        log.LogError($"{nameof(blinker)} {e}");
    //    }
    //}


    public void Dispose()
    {
        Stop();
    }
}
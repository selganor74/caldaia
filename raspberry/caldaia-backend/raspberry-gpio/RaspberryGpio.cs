using System.Device.Gpio;
using Microsoft.Extensions.Logging;

namespace raspberry_gpio;

public class RaspberryGpio : IDisposable
{
    private GpioController gpio;
    private Thread blinker;
    private bool isStarted;
    private readonly ILogger<RaspberryGpio> log;

    public enum DigitalInput_GPIO
    {
        Unset = 0,

        /// <summary>
        /// GPIO=26, PIN=37
        /// </summary>
        TERMOSTATO_AMBIENTI = 26,

        /// <summary>
        /// GPIO=6, PIN=31 
        /// </summary>
        TERMOSTATO_ROTEX = 6
    }

    public enum RelayOutput_GPIO
    {
        Unset = 0,

        /// <summary>
        /// GPIO=17, PIN=11
        /// </summary>
        RELAY_CALDAIA = 17, 

        /// <summary>
        /// GPIO=27, PIN=13
        /// </summary>
        RELAY_POMPA_CAMINO = 27, 
        
        /// <summary>
        /// GPIO=22, PIN=15
        /// </summary>
        RELAY_BYPASS_TERMOSTATO = 22,

        /// <summary>
        /// GPIO=23, PIN=16
        /// </summary>
        RELAY_POMPA_RISCALDAMENTO = 23 
    }

    public RaspberryGpio(
        ILogger<RaspberryGpio> log
        )
    {
        this.blinker = new Thread((obj) => this.Blinker());
        this.log = log;

        this.gpio = new System.Device.Gpio.GpioController();
    }

    private void SetRelayOn(RelayOutput_GPIO relay)
    {
        log.LogDebug($"Setting {relay.ToString()} ({(int)relay}) to On ...");
        this.gpio.Write((int)relay, 0);
        log.LogDebug($"... {relay.ToString()} ({(int)relay}) set to On");
    }

    private void SetRelayOff(RelayOutput_GPIO relay)
    {
        log.LogDebug($"Setting {relay.ToString()} ({(int)relay}) to Off ...");
        this.gpio.Write((int)relay, 1);
        log.LogDebug($"... {relay.ToString()} ({(int)relay}) set to Off");
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

    private void Blinker()
    {
        try
        {
            var allPins = Enum.GetValues(typeof(RelayOutput_GPIO));
            var noOfPins = allPins.Length;
            var currIndex = 0;
            while (isStarted)
            {
                if (currIndex >= noOfPins)
                    currIndex = 0;

                var currPin = (RelayOutput_GPIO)(allPins.GetValue(currIndex) ?? default(RelayOutput_GPIO));
                if ((int)currPin == 0)
                {
                    log.LogDebug($"Null pin {currPin.ToString()} ({(int)currPin}) Skipping");
                    currIndex++;
                    continue;
                }

                SetRelayOn(currPin);

                Thread.Sleep(TimeSpan.FromSeconds(2));

                SetRelayOff(currPin);

                currIndex++;
            }
        }
        catch (Exception e)
        {
            log.LogError($"{nameof(blinker)} {e}");
        }
    }


    public void Dispose()
    {
        Stop();
    }
}
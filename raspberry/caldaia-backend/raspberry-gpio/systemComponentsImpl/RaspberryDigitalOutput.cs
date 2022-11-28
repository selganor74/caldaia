using System.Device.Gpio;
using domain.systemComponents;
using Microsoft.Extensions.Logging;

namespace raspberry_gpio;

public class RaspberryDigitalOutput : DigitalOutput
{
    private readonly int gpioId;
    private readonly GpioController gpio;

    public RaspberryDigitalOutput(
        string name,
        int gpioId,
        GpioController gpio, 
        ILogger<RaspberryDigitalOutput> log) : base(name, log)
    {
        this.gpioId = gpioId;
        this.gpio = gpio;

        this.gpio.OpenPin(gpioId, PinMode.Output, PinValue.Low);
        this.SetToOff("Inizializzazione.");
        log.LogInformation($"{nameof(RaspberryDigitalOutput)}: Initialized {name} on GPIO {gpioId}");
    }

    protected override void SetToOffImplementation()
    {
        log.LogDebug($"Setting {Name} ({gpioId}) to Off ...");
        this.gpio.Write(gpioId, PinValue.High);
        log.LogDebug($"... {Name} ({gpioId}) set to Off");
    }

    protected override void SetToOnImplementation()
    {
        log.LogDebug($"Setting {Name} ({gpioId}) to On ...");
        this.gpio.Write(gpioId, PinValue.Low);
        log.LogDebug($"... {Name} ({gpioId}) set to On");
    }
}

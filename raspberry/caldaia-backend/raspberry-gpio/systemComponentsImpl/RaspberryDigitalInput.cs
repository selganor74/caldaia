using System.Device.Gpio;
using domain.measures;
using domain.systemComponents;
using Microsoft.Extensions.Logging;

namespace raspberry_gpio;

public class RaspberryDigitalInput : DigitalInput
{
    private readonly int gpioId;
    private readonly GpioController gpio;

    public RaspberryDigitalInput(
        string name,
        int gpioId,
        GpioController gpio, 
        ILogger log
        ) : base(name, log)
    {
        this.gpioId = gpioId;
        this.gpio = gpio;
        this.gpio.OpenPin(gpioId, PinMode.Input);

        this.gpio.RegisterCallbackForPinValueChangedEvent(gpioId, PinEventTypes.Rising | PinEventTypes.Falling, OnPinChangedEvent);
        var value = this.gpio.Read(this.gpioId) == PinValue.High ? OnOffState.ON : OnOffState.OFF;
        this.LastMeasure = new OnOff(value);
        log.LogInformation($"{nameof(RaspberryDigitalInput)}: Initialized {name} on GPIO {gpioId}");
    }

    private void OnPinChangedEvent(object sender, PinValueChangedEventArgs pinValueChangedEventArgs) {
        switch(pinValueChangedEventArgs.ChangeType) {
            case PinEventTypes.Falling: 
                this.LastMeasure = new OnOff(OnOffState.OFF);
                break;  
            case PinEventTypes.Rising:
                this.LastMeasure = new OnOff(OnOffState.ON);
                break;            
        }
    }
}

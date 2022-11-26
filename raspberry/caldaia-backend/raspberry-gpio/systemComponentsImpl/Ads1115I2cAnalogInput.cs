using System.Threading;
using domain;
using domain.measures;
using domain.systemComponents;
using Microsoft.Extensions.Logging;

namespace raspberry_gpio;

public class Ads1115I2cAnalogInput : AnalogInput<PureNumber>, IDisposable
{
    private readonly ADS1115Sensor adc;
    private readonly TimeSpan readInterval;
    private ADS1115SensorSetting settings;
    private bool isStarted = true;

    Thread readThread;

    public Ads1115I2cAnalogInput(
        string name,
        int busId,
        AdcAddress addr,
        AdcInput input,
        TimeSpan readInterval,
        ILogger<Ads1115I2cAnalogInput> log) : base(name, log)
    {
        try
        {
            this.adc = new ADS1115Sensor(addr);
            this.readInterval = readInterval;

#pragma warning disable CS8604
#pragma warning disable CS8631
            LastMeasure = LastMeasure.WithNewValue(0m);
#pragma warning restore CS8631
#pragma warning restore CS8604
            adc.Initialize(busId);

            settings = new ADS1115SensorSetting();
            settings.Mode = AdcMode.CONTINOUS_CONVERSION;
            settings.Input = input;

            adc.readContinuousInit(settings).Wait();
            this.readThread = new Thread((obj) => ReadCycle());
            this.readThread.Start();
        }
        catch (Exception e)
        {
            log.LogCritical($"Unable to initialize {name} with {nameof(addr)}: {addr.ToString()}.{Environment.NewLine}{e}");
            throw;
        }
    }

    private void ReadCycle()
    {
        try
        {
            while (isStarted)
            {
#pragma warning disable CS8631
                Thread.Sleep(readInterval);
                var value = (decimal)adc.readContinuous();
                LastMeasure = LastMeasure.WithNewValue(value);
#pragma warning restore CS8604
                log.LogDebug($"{Name} Read new value: adc:{value}");
            }
        }
        catch (Exception e)
        {
            log.LogCritical($"Errors reading {Name}.{Environment.NewLine}{e}");
            throw;
        }
    }

    private Boolean isDisposing = false;
    
    public void Dispose()
    {
        lock(this) {
            if(isDisposing)
                return;

            isDisposing = true;
        }

        log.LogDebug($"{Name} is disposing ...");
        isStarted = false;
        readThread.Join();
        log.LogDebug($"{Name} ... {nameof(readThread)} stopped ...");
        adc.Dispose();
        log.LogDebug($"{Name} ... {nameof(adc)} disposed ...");
        log.LogDebug($"{Name} ... Disposed!");
    }
}

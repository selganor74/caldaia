using System.Runtime.InteropServices.ComTypes;
using domain;
using domain.measures;
using domain.systemComponents;
using Microsoft.Extensions.Logging;

namespace raspberry_gpio;

public class Ads1115I2cAnalogInput : AnalogInput<Voltage>, IDisposable
{
    private readonly ADS1115Sensor adc;
    private readonly TimeSpan readInterval;
    private ADS1115SensorSetting settings;
    private bool isRunning = true;

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

            LastMeasure = LastMeasure.WithNewValue(0m);
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
            while (isRunning)
            {
                Thread.Sleep(readInterval);
                var value = (decimal)adc.readContinuous();
                var tensionValue = value * (3.3m / 32767m);
                LastMeasure = LastMeasure.WithNewValue(value);
                log.LogDebug($"{Name} Read new value: adc:{value} -> voltage: {LastMeasure.FormattedValue}");
            }
        }
        catch (Exception e)
        {
            log.LogCritical($"Errors reading {Name}.{Environment.NewLine}{e}");
            throw;
        }
    }

    public void Dispose()
    {
        isRunning = false;
        readThread.Join();
    }
}

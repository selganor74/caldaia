using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public class AnalogInputConverter<TToMeasure> : AnalogInput, IDisposable
    where TToMeasure : IMeasure
{
    private readonly AnalogInput source;

    public AnalogInputConverter(
        string name,
        AnalogInput source,
        Func<decimal, decimal> valueConverter,
        ILogger<AnalogInput> log) : base(name, log)
    {
        this.source = source;

        source.OnValueRead += (src, measure) =>
        {
            try
            {
                this.LastError = null;
                #pragma warning disable CS8604
                LastMeasure = LastMeasure.WithNewValue<TToMeasure>(valueConverter(measure.Value), measure.UtcTimeStamp);
                #pragma warning restore CS8604
            }
            catch (Exception e)
            {
                log.LogError($"{Name} cannot convert value {measure} from {source.Name}.{Environment.NewLine}{e}");
                LastError = e;
            }
        };
    }

    public void Dispose()
    {
        var disposable = (source as IDisposable);
        if(disposable != null) {
            Console.WriteLine($"{Name} is disposing its {nameof(source)} [{source.GetType().Name}]");
        }
    }
}

using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public class AnalogInputConverter<TFromMeasure, TToMeasure> : AnalogInput<TToMeasure>, IDisposable
    where TFromMeasure : IMeasure
    where TToMeasure : IMeasure
{
    private readonly AnalogInput<TFromMeasure> source;

    public AnalogInputConverter(
        string name,
        AnalogInput<TFromMeasure> source,
        Func<decimal, decimal> valueConverter,
        ILogger<AnalogInput<TToMeasure>> log) : base(name, log)
    {
        this.source = source;

        source.ValueRead += (src, measure) =>
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

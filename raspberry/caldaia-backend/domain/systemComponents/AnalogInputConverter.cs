using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public class AnalogInputConverter<TFromMeasure, TToMeasure> : AnalogInput<TToMeasure>
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

        source.ValueChanged += (src, measure) =>
        {
            try
            {
                this.LastError = null;
                LastMeasure = LastMeasure.WithNewValue<TToMeasure>(valueConverter(measure.Value), measure.UtcTimeStamp);
            }
            catch (Exception e)
            {
                log.LogError($"{Name} cannot convert value {measure} from {source.Name}.{Environment.NewLine}{e}");
                LastError = e;
            }
        };
    }
}

using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

// Negates the source input value
public class LogicNot : DigitalInput
{
    public LogicNot(
        string name,
        DigitalInput source,  
        ILogger log
        ) : base(name, log)
    {
        if (source.LastMeasure != null)
            OnSourceChanged(source, source.LastMeasure);

        source.OnValueRead += OnSourceChanged;
    }

    private void OnSourceChanged(object? source, IMeasure newValue) 
    {
        var lv = (OnOff)newValue;
        if (lv.DigitalValue == OnOffState.ON)
            this.LastMeasure = lv.WithNewValue(0);
        
        if (lv.DigitalValue == OnOffState.OFF)
            this.LastMeasure = lv.WithNewValue(1);

        log.LogDebug($"{nameof(LogicNot)}<{((DigitalInput)source).Name}>: Input: {lv.DigitalValue} Output: {LastMeasure.FormattedValue}");
    }
}

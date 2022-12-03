using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

// Negates the source input value
public class LogicNot : DigitalInput
{
    public LogicNot(
        string name,
        DigitalInput source,  
        ILogger<LogicNot> log
        ) : base(name, log)
    {
        if (source.LastMeasure != null)
            OnSourceChanged(null, source.LastMeasure);

        source.ValueRead += OnSourceChanged;
    }

    private void OnSourceChanged(object? source, OnOff newValue) 
    {
        if (newValue.DigitalValue == OnOffState.ON)
            this.LastMeasure = newValue.WithNewValue(0);
        
        if (newValue.DigitalValue == OnOffState.OFF)
            this.LastMeasure = newValue.WithNewValue(1);
    }
}

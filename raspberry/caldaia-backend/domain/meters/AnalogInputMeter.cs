using domain.systemComponents;

namespace domain.measures.meters;

public class AnalogInputMeter<TMeasure> where TMeasure : IMeasure
{
    public TMeasure? LastKnownValue { get; protected set; }
    public TMeasure? Average { get; protected set; }
    protected List<TMeasure> history = new List<TMeasure>();
    protected const int MAX_ITEMS_IN_HISTORY = 512;

    // A meter keeps track of the values assumed by an input as time goes by...
    public AnalogInputMeter(AnalogInput<TMeasure> inputToMeasure)
    {
        if (inputToMeasure is null)
            throw new ArgumentNullException(nameof(inputToMeasure));

        inputToMeasure.ValueChanged += ValueChangedHandler;
        ValueChangedHandler(this, inputToMeasure.LastMeasure);
    }

    public TMeasure? GetAverageByTimeSpan(TimeSpan period)
    {
        if (LastKnownValue is null)
            return default(TMeasure);

        var average = history.Where(h => h.UtcTimeStamp > DateTime.UtcNow - period).Select(h => h.Value).Average();
        return LastKnownValue.WithNewValue(average, DateTime.UtcNow);
    }

    protected virtual void ValueChangedHandler(object? sender, TMeasure? newValue)
    {
        if (newValue is null)
            return;

        this.LastKnownValue = newValue;
        TMeasure? removedValue = default(TMeasure);
        if (this.history.Count == MAX_ITEMS_IN_HISTORY)
        {
            removedValue = this.history[0];
            this.history.RemoveAt(0);
        }
        this.history.Add(newValue);

        UpdateAverage(newValue, removedValue);
    }

    private void UpdateAverage(TMeasure newValue, TMeasure? removedValue)
    {
        if (Average is null)
        {
            Average = newValue;
        }
        else
        {
            decimal newAverage = ComputeAverage(Average, newValue, removedValue);
            Average = Average.WithNewValue(newAverage, newValue.UtcTimeStamp);
        }
    }

    private decimal ComputeAverage(TMeasure currentAverage, TMeasure newValue, TMeasure? removedValue)
    {
        var totAvgSamples = (decimal)(history.Count - 1);
        var totElements = (decimal)history.Count;
        var avgContribution = currentAverage.Value / totAvgSamples;
        var newValueContribution = (newValue.Value / totElements) - avgContribution;
        var exitedValueContribution = removedValue == null ? 0 : avgContribution - (removedValue.Value / totElements);
        var newAverage = currentAverage.Value + newValueContribution + exitedValueContribution;
        return newAverage;
    }
}

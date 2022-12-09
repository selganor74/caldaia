using domain.systemComponents;

namespace domain.measures.meters;

public class AnalogInputMeter
{
    public IMeasure? LastKnownValue { get; protected set; }
    public IMeasure? Average { get; protected set; }
    protected List<IMeasure> history = new List<IMeasure>();
    protected const int MAX_ITEMS_IN_HISTORY = 512;

    // A meter keeps track of the values assumed by an input as time goes by...
    public AnalogInputMeter(AnalogInput inputToMeasure)
    {
        if (inputToMeasure is null)
            throw new ArgumentNullException(nameof(inputToMeasure));

        inputToMeasure.OnValueRead += ValueChangedHandler;
        ValueChangedHandler(this, inputToMeasure.LastMeasure);
    }

    public IMeasure? GetAverageByTimeSpan(TimeSpan period)
    {
        if (LastKnownValue is null)
            return default(IMeasure);

        var average = history.Where(h => h.UtcTimeStamp > DateTime.UtcNow - period).Select(h => h.Value).Average();
        return LastKnownValue.WithNewValue(average, DateTime.UtcNow);
    }

    protected virtual void ValueChangedHandler(object? sender, IMeasure? newValue)
    {
        if (newValue is null)
            return;

        this.LastKnownValue = newValue;
        IMeasure? removedValue = default(IMeasure);
        if (this.history.Count == MAX_ITEMS_IN_HISTORY)
        {
            removedValue = this.history[0];
            this.history.RemoveAt(0);
        }
        this.history.Add(newValue);

        UpdateAverage(newValue, removedValue);
    }

    private void UpdateAverage(IMeasure newValue, IMeasure? removedValue)
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

    private decimal ComputeAverage(IMeasure currentAverage, IMeasure newValue, IMeasure? removedValue)
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

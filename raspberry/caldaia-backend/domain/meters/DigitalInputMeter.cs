using domain.measures;
using domain.systemComponents;

namespace domain.meters;

public class DigitalInputMeter
{
    public OnOff? LastKnownValue { get; protected set; }

    private IntervalMilliseconds _totalTimeOn = new IntervalMilliseconds(0);
    public IntervalMilliseconds TotalTimeOn
    {
        get
        {
            if (LastKnownValue == null || LastKnownValue.Value == 0)
                return _totalTimeOn;

            var newUtcTimestamp = DateTime.UtcNow;
            var newValue = _totalTimeOn.Value + (decimal)(newUtcTimestamp - _totalTimeOn.UtcTimeStamp).TotalMilliseconds;
            return _totalTimeOn.WithNewValue<IntervalMilliseconds>(newValue, newUtcTimestamp);
        }
        protected set
        {
            _totalTimeOn = value;
        }
    }

    private IntervalMilliseconds _totalTimeOff = new IntervalMilliseconds(0);
    public IntervalMilliseconds TotalTimeOff
    {
        get
        {
            if (LastKnownValue == null || LastKnownValue.Value == 1)
                return _totalTimeOff;

            var newUtcTimestamp = DateTime.UtcNow;
            var newValue = _totalTimeOff.Value + (decimal)(newUtcTimestamp - _totalTimeOff.UtcTimeStamp).TotalMilliseconds;
            return _totalTimeOn.WithNewValue<IntervalMilliseconds>(newValue, newUtcTimestamp);
        }
        protected set
        {
            _totalTimeOn = value;
        }
    }

    protected List<OnOff> history = new List<OnOff>();
    protected const int MAX_ITEMS_IN_HISTORY = 512;

    public DigitalInputMeter(DigitalInput inputToMeasure)
    {
        inputToMeasure.ValueChanged += ValueChangedHandler;
        inputToMeasure.TransitionedFromOffToOn += OffToOnHandler;
        inputToMeasure.TransitionedFromOnToOff += OnToOffHandler;
    }

    protected virtual void ValueChangedHandler(object? sender, OnOff? newValue)
    {
        if (newValue is null)
            return;

        this.LastKnownValue = newValue;
        OnOff? removedValue = default(OnOff);
        if (this.history.Count == MAX_ITEMS_IN_HISTORY)
        {
            removedValue = this.history[0];
            this.history.RemoveAt(0);
        }
        this.history.Add(newValue);
    }

    protected virtual void OffToOnHandler(object? sender, OnOff? newValue)
    {
        if (newValue == null)
            return;

        var newMilliseconds = (decimal)(newValue.UtcTimeStamp - _totalTimeOff.UtcTimeStamp).TotalMilliseconds;
        TotalTimeOff = TotalTimeOff.WithNewValue<IntervalMilliseconds>(_totalTimeOff.Value + newMilliseconds, newValue.UtcTimeStamp);
    }

    protected virtual void OnToOffHandler(object? sender, OnOff? newValue)
    {
        if (newValue == null)
            return;

        var newMilliseconds = (decimal)(newValue.UtcTimeStamp - _totalTimeOn.UtcTimeStamp).TotalMilliseconds;
        TotalTimeOn = TotalTimeOn.WithNewValue<IntervalMilliseconds>(_totalTimeOn.Value + newMilliseconds, newValue.UtcTimeStamp);
    }
}

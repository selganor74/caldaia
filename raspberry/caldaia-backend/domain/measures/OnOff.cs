namespace domain.measures;

public enum OnOffState {
    OFF = 0,
    ON = 1
}

public static class OnOffExtensions {
    public static bool IsOn(this OnOffState? onOff) {
        if (onOff == null)
            return false;
        
        return onOff==OnOffState.ON;
    }

    public static bool IsOn(this OnOffState onOff) {
        return onOff==OnOffState.ON;
    }

    public static bool IsOff(this OnOffState? onOff) {
        return !onOff.IsOn();
    }

    public static bool IsOff(this OnOffState onOff) {
        return !onOff.IsOn();
    }
}

public class OnOff : SimpleMeasure
{
    public override string UoM => "";
    public override string FormattedValue => Value == 0 ? "Off" : "On";

    public OnOffState DigitalValue => this.Value == 0 ? OnOffState.OFF : OnOffState.ON;

    public bool IsOn() => this.DigitalValue.IsOn();
    public bool IsOff() => this.DigitalValue.IsOff();
    public OnOff(OnOffState state, DateTime? utcTimeStamp = null) : base((decimal)state, utcTimeStamp)
    {
    }
}
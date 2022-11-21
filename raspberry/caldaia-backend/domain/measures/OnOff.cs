namespace domain.measures;

public enum OnOffState {
    OFF = 0,
    ON = 1
}

public class OnOff : SimpleMeasure
{
    public override string UoM => "";
    public override string FormattedValue => Value == 0 ? "Off" : "On";

    public OnOffState DigitalValue => this.Value == 0 ? OnOffState.OFF : OnOffState.ON;
    public OnOff(OnOffState state, DateTime? utcTimeStamp = null) : base((decimal)state, utcTimeStamp)
    {
    }
}
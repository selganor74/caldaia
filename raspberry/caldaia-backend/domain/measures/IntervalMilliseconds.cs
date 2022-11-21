namespace domain.measures;

public class IntervalMilliseconds : SimpleMeasure
{
    public override string UoM => "ms";
    public override string FormattedValue => $"{Value:F0} {UoM}";


    public IntervalMilliseconds(decimal value, DateTime? utcTimeStamp = null) : base(value, utcTimeStamp)
    {
    }
}

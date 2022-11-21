namespace domain.measures;

public class Temperature : SimpleMeasure
{
    public override string UoM => "°C";

    public override string FormattedValue => $"{Value:F1} {UoM}";

    public Temperature(decimal value, DateTime? utcTimeStamp = null) : base(value, utcTimeStamp)
    {
    }
}

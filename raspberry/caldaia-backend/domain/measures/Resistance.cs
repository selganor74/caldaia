namespace domain.measures;

public class Resistance : SimpleMeasure
{
    public override string UoM => "Ω";

    public override string FormattedValue => $"{Value:F2} {UoM}";

    public Resistance(decimal value, DateTime? utcTimeStamp = null) : base(value, utcTimeStamp)
    {
    }
}

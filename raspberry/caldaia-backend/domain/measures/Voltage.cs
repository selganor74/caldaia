namespace domain.measures;

public class Voltage : SimpleMeasure
{
    public override string UoM => "V";

    public Voltage(decimal value, DateTime? utcTimeStamp = null) : base(value, utcTimeStamp)
    {
    }

}

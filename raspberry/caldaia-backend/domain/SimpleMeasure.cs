using System.Reflection;

namespace domain;

public abstract class SimpleMeasure : IMeasure
{
    // this must be overwritten in all subclasses
    public abstract string UoM { get; } // You MUST implement this in each subclass !
    public decimal Value { get; }
    public DateTime UtcTimeStamp { get; }

    public virtual string FormattedValue => $"{Value} {UoM}";

    public SimpleMeasure(decimal value, DateTime? utcTimeStamp = null)
    {
        Value = value;
        UtcTimeStamp = utcTimeStamp ?? DateTime.UtcNow;
    }
}

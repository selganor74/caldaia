using System.Reflection;

namespace domain;

public abstract class SimpleMeasure : IMeasure
{
    // this must be overwritten in all subclasses
    public abstract string UoM { get; } // You MUST implement this in each subclass !
    public decimal Value { get; }
    public DateTime UtcTimeStamp { get; }

    public virtual string FormattedValue
    {
        get
        {
            var absValue = Math.Abs(Value);
            for (var exponent = -12d; exponent <= 12d; exponent += 3d)
            {
                var from = (decimal)Math.Pow(10d, exponent);
                var to = (decimal)Math.Pow(10d, exponent + 3d);
                if (from <= absValue && absValue < to)
                {
                    var value = Value / from;
                    switch (exponent)
                    {
                        case -12: return $"{value:F3} p{UoM}";
                        case -9: return $"{value:F3} n{UoM}";
                        case -6: return $"{value:F3} u{UoM}";
                        case -3: return $"{value:F3} m{UoM}";
                        case 0: return $"{value:F3}  {UoM}";
                        case 3: return $"{value:F3} k{UoM}";
                        case 6: return $"{value:F3} M{UoM}";
                        case 9: return $"{value:F3} G{UoM}";
                        case 12: return $"{value:F3} P{UoM}";
                    }
                }
            }
            return $"{Value:F3} {UoM}";
        }
    }

    public SimpleMeasure(decimal value, DateTime? utcTimeStamp = null)
    {
        Value = value;
        UtcTimeStamp = utcTimeStamp ?? DateTime.UtcNow;
    }
}

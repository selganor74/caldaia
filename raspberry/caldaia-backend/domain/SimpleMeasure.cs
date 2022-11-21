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

    public virtual T WithNewValue<T>(decimal newValue, DateTime? newUtcTimeStamp = null) where T : IMeasure
    {
        T? toReturn = default(T);
        Type type = typeof(T);
        ConstructorInfo? ctor = type.GetConstructor(new[] { typeof(decimal), typeof(DateTime?) });
        if (ctor != null)
        {
            toReturn = (T)ctor.Invoke(new object[]
            {
                newValue,
                #pragma warning disable CS8601
                newUtcTimeStamp
                #pragma warning restore CS8601
            });
        }

        if (toReturn == null)
            throw new Exception($"Unable to create new instance of {typeof(T)}");

        return toReturn;
    }
}

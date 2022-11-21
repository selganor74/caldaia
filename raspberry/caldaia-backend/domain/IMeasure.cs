namespace domain;

public interface IMeasure
{
    DateTime UtcTimeStamp { get; }
    decimal Value { get; }
    string UoM { get; }
    string FormattedValue { get; }

    T WithNewValue<T>(decimal newValue, DateTime? newUtcTimeStamp = null) where T : IMeasure;
}

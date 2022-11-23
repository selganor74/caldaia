namespace domain;

public interface IMeasure
{
    DateTime UtcTimeStamp { get; }
    decimal Value { get; }
    string UoM { get; }
    string FormattedValue { get; }

}

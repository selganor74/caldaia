namespace domain;

public static class IMeasureExtensions
{
    public static IMeasure WithNewValue(this IMeasure src, decimal newValue, DateTime? newUtcTimeStamp = null)
    {
        IMeasure? toReturn = default(IMeasure);
        Type type = src.GetType();
        toReturn = (IMeasure)Activator.CreateInstance(type, new object[] { newValue, null }) ?? throw new Exception($"{nameof(WithNewValue)}Unable to create new instance of type {type.Name}");

        return toReturn;
    }

    public static TMeasure WithNewValue<TMeasure>(this IMeasure src, decimal newValue, DateTime? newUtcTimeStamp = null)
        where TMeasure : IMeasure
    {
        TMeasure? toReturn = default(TMeasure);
        Type type = typeof(TMeasure);
        toReturn = (TMeasure)Activator.CreateInstance(type, new object[] { newValue, null }) ?? throw new Exception($"{nameof(WithNewValue)}Unable to create new instance of type {type.Name}");

        return toReturn;
    }

}

namespace domain;

public static class IMeasureExtensions
{
    public static TMeasure WithNewValue<TMeasure>(this TMeasure src, decimal newValue, DateTime? newUtcTimeStamp = null) where TMeasure : IMeasure
    {
        TMeasure? toReturn = default(TMeasure);
        Type type = typeof(TMeasure);
        toReturn = (TMeasure)Activator.CreateInstance(typeof(TMeasure), new object[] { 0m, null }) ?? throw new Exception($"Unable to create new instance of {typeof(TMeasure)}");

        return toReturn;
    }
}

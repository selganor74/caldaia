namespace domain.meters;

public static class StatsCalculator
{
    public static StatsDTO GetStats(this List<IMeasure> history, DateTimeOffset fromDate, DateTimeOffset? toDate = null)
    {
        toDate = toDate ?? DateTimeOffset.UtcNow;
        var valuesInPeriod = history.Where(h => h.UtcTimeStamp > fromDate && h.UtcTimeStamp < toDate).OrderByDescending(h => h.UtcTimeStamp).ToList();

        valuesInPeriod = AddFirstValue(history, fromDate, valuesInPeriod);
        valuesInPeriod = AddLastValue(toDate, valuesInPeriod);

        var toReturn = ComputeStats(fromDate, toDate, valuesInPeriod);
        return toReturn;
    }

    private static List<IMeasure> AddLastValue(DateTimeOffset? toDate, List<IMeasure> valuesInPeriod)
    {
        valuesInPeriod = valuesInPeriod.OrderBy(h => h.UtcTimeStamp).ToList();
        
        var lastValueInPeriod = valuesInPeriod.LastOrDefault();

        if (lastValueInPeriod != null)
        {
            lastValueInPeriod = lastValueInPeriod.WithNewValue(lastValueInPeriod.Value, toDate.Value.DateTime);
            valuesInPeriod.Add(lastValueInPeriod);
        }

        return valuesInPeriod;
    }

    private static List<IMeasure> AddFirstValue(List<IMeasure> history, DateTimeOffset fromDate, List<IMeasure> valuesInPeriod)
    {
        var lastKnownValueBefore = history.Where(h => h.UtcTimeStamp <= fromDate).OrderByDescending(h => h.UtcTimeStamp).FirstOrDefault();
        if (lastKnownValueBefore != null)
        {
            lastKnownValueBefore = lastKnownValueBefore.WithNewValue(lastKnownValueBefore.Value, fromDate.DateTime);
            valuesInPeriod.Add(lastKnownValueBefore);
        }
        return valuesInPeriod;
    }

    private static StatsDTO ComputeStats(DateTimeOffset fromDate, DateTimeOffset? toDate, List<IMeasure> valuesInPeriod)
    {
        valuesInPeriod = valuesInPeriod.OrderBy(h => h.UtcTimeStamp).ToList();
        decimal totalWeight = (decimal)(valuesInPeriod.Last().UtcTimeStamp - valuesInPeriod.First().UtcTimeStamp).TotalMilliseconds;
        IMeasure prev = null;
        decimal avg = 0;
        decimal min = valuesInPeriod.Count == 0 ? 0 : Decimal.MaxValue;
        decimal max = valuesInPeriod.Count == 0 ? 0 : Decimal.MinValue;
        foreach (var m in valuesInPeriod)
        {
            if (prev == null)
            {
                prev = m;
                continue;
            }
            var weight = (decimal)((m.UtcTimeStamp - prev.UtcTimeStamp).TotalMilliseconds) / totalWeight;
            var contribute = prev.Value * weight;
            avg += contribute;
            min = prev.Value < min ? prev.Value : min;
            max = prev.Value > max ? prev.Value : max;
            prev = m;
        }

        var toReturn = new StatsDTO()
        {
            Max = max,
            Min = min,
            Avg = avg,
            From = fromDate.DateTime,
            To = toDate.Value.DateTime,
            Samples = valuesInPeriod.Count
        };
        return toReturn;
    }
}

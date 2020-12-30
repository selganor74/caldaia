namespace CaldaiaBackend.Application.Projections.DataModels
{
    /// <summary>
    /// This is only a DTO 
    /// </summary>
    public class TemperatureStatisticsWithNoDetails
    {
        public AggregatedValues TEMPERATURA_ACCUMULO { get; set; } = new AggregatedValues();

        public AggregatedValues TEMPERATURA_PANNELLI { get; set; } = new AggregatedValues();

        public AggregatedValues TEMPERATURA_CAMINO { get; set; } = new AggregatedValues();

        public AggregatedValues TEMPERATURA_ACCUMULO_INFERIORE { get; set; } = new AggregatedValues();

    }
}
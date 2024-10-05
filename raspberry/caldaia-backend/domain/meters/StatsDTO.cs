namespace domain.meters;

public class StatsDTO
{
    public static StatsDTO EmptyStatsDTO = new StatsDTO();

    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Avg { get; set; }
    public int Samples { get; set; }
}

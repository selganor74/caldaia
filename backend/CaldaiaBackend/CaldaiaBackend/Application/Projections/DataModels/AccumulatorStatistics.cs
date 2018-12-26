namespace CaldaiaBackend.Application.Projections.DataModels
{
    public class AccumulatorStatistics
    {
        public ulong TEMPO_ACCENSIONE_POMPA_RISCALDAMENTO { get; set; }
        public ulong TEMPO_ACCENSIONE_POMPA_CAMINO { get; set; }
        public ulong TEMPO_ACCENSIONE_CALDAIA { get; set; }
        public ulong TEMPO_TERMOSTATI_AMBIENTE { get; set; }
        public ulong TEMPO_TERMOSTATO_ACCUMULATORE { get; set; }
        public ulong TEMPO_ACCENSIONE_POMPA_SOLARE { get; set; }
    }
}

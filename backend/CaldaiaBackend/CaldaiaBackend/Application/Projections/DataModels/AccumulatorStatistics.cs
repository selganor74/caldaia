namespace CaldaiaBackend.Application.Projections.DataModels
{
    public class AccumulatorStatistics
    {
        public ulong outPompaAccu_On { get; set; }
        public ulong outPompaCaminoAccu_On { get; set; }
        public ulong outCaldaiaAccu_On { get; set; }
        public ulong inTermoAmbienteAccu_On { get; set; }
        public ulong inTermoAccumulatoreAccu_On { get; set; }
        public ulong rotexP1Accu_On { get; set; }
    }
}

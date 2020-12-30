using CaldaiaBackend.Application.Events;

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

        public void AddAccumulatorsReceivedEvent(AccumulatorsReceived evt)
        {
            this.TEMPO_TERMOSTATO_ACCUMULATORE += evt.inTermoAccumulatoreAccu_On;
            this.TEMPO_TERMOSTATI_AMBIENTE += evt.inTermoAmbienteAccu_On;
            this.TEMPO_ACCENSIONE_POMPA_CAMINO += evt.outPompaCaminoAccu_On;
            this.TEMPO_ACCENSIONE_CALDAIA += evt.outCaldaiaAccu_On;
            this.TEMPO_ACCENSIONE_POMPA_RISCALDAMENTO += evt.outPompaAccu_On;
            this.TEMPO_ACCENSIONE_POMPA_SOLARE += evt.rotexP1Accu_On;

        }
    }
}

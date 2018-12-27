using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaldaiaBackend.Application.DataModels;
using Infrastructure.DomainEvents;

namespace CaldaiaBackend.Application.Events
{
    public class AccumulatorsReceived : IDomainEvent
    {
        public DateTime timestamp { get; set; }
        public ulong outPompaAccu_On { get; set; }
        public ulong outPompaAccu_Off { get; set; }
        public ulong outPompaCaminoAccu_On { get; set; }
        public ulong outPompaCaminoAccu_Off { get; set; }
        public ulong outCaldaiaAccu_On { get; set; }
        public ulong outCaldaiaAccu_Off { get; set; }
        public ulong inTermoAmbienteAccu_On { get; set; }
        public ulong inTermoAmbienteAccu_Off { get; set; }
        public ulong inTermoAccumulatoreAccu_On { get; set; }
        public ulong inTermoAccumulatoreAccu_Off { get; set; }
        public ulong rotexP1Accu_On { get; set; }

        public static AccumulatorsReceived FromData(DataFromArduino data)
        {
            return new AccumulatorsReceived
            {
                timestamp = DateTime.Parse(data.timestamp),
                outCaldaiaAccu_Off = data.outCaldaiaAccu_Off,
                outCaldaiaAccu_On = data.outCaldaiaAccu_On,
                inTermoAccumulatoreAccu_On = data.inTermoAccumulatoreAccu_On,
                inTermoAccumulatoreAccu_Off = data.inTermoAccumulatoreAccu_Off,
                inTermoAmbienteAccu_Off = data.inTermoAmbienteAccu_Off,
                inTermoAmbienteAccu_On = data.inTermoAmbienteAccu_On,
                outPompaAccu_Off = data.outPompaAccu_Off,
                outPompaAccu_On = data.outPompaAccu_On,
                outPompaCaminoAccu_Off = data.outPompaCaminoAccu_Off,
                outPompaCaminoAccu_On = data.outPompaCaminoAccu_On,
                rotexP1Accu_On = data.rotexP1Accu_On
            };
        }
    }
}

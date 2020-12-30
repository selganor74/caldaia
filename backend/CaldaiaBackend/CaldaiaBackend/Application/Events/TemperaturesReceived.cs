using System;
using CaldaiaBackend.Application.DataModels;
using Infrastructure.DomainDesign.DomainEvents;

namespace CaldaiaBackend.Application.Events
{
    public class TemperaturesReceived : IDomainEvent
    {
        public DateTime timestamp { get; set; }

        /// <summary>
        /// temperatura registrata dal sensore sui pannelli solari
        /// </summary>
        public int rotexTK { get; set; }

        /// <summary>
        /// temperatura della parte bassa dell'accumulo (attualmente montato sul tubo di mandata verso i pannelli)
        /// </summary>
        public int rotexTR { get; set; }

        /// <summary>
        /// Temperatura dell'accumulo
        /// </summary>
        public int rotexTS { get; set; }

        /// <summary>
        /// Temperatura del camino.
        /// </summary>
        public float ainTempCaminoValueCentigradi { get; set; }

        public static TemperaturesReceived FromData(DataFromArduino data)
        {
            return new TemperaturesReceived
            {
                timestamp = DateTime.Parse(data.timestamp),
                ainTempCaminoValueCentigradi = data.ainTempCaminoValueCentigradi,
                rotexTK = data.rotexTK,
                rotexTR = data.rotexTR,
                rotexTS = data.rotexTS
            };
        }
    }
}
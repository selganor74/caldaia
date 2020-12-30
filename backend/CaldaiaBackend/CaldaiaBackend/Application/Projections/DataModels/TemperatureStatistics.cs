using System.Collections.Generic;
using System.Linq;
using CaldaiaBackend.Application.Events;

namespace CaldaiaBackend.Application.Projections.DataModels
{
    public class TemperatureDetails
    {
        /// <summary>
        /// Timestamp del dettaglio.
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Corrisponde alla temperatura TS dell'accumulo rotex
        /// </summary>
        public int TEMPERATURA_ACCUMULO { get; set; }

        /// <summary>
        /// Corrisponde alla temperatura TK dell'accumulo
        /// </summary>
        public int TEMPERATURA_PANNELLI { get; set; }

        /// <summary>
        /// Temperatura del camino
        /// </summary>
        public int TEMPERATURA_CAMINO { get; set; }

        /// <summary>
        /// Corrisponde alla temperatura TR dell'accumulo Rotex.
        /// </summary>
        public int TEMPERATURA_ACCUMULO_INFERIORE { get; set; }

        public static TemperatureDetails FromEvent(TemperaturesReceived evt)
        {
            var toReturn = new TemperatureDetails
            {
                Timestamp = evt.timestamp.ToString("o"),
                TEMPERATURA_ACCUMULO = evt.rotexTS,
                TEMPERATURA_ACCUMULO_INFERIORE = evt.rotexTR,
                TEMPERATURA_CAMINO = (int) evt.ainTempCaminoValueCentigradi,
                TEMPERATURA_PANNELLI = evt.rotexTK
            };
            return toReturn;
        }
    }

    public class AggregatedValues
    {
        public int Max { get; set; }
        public int Min { get; set; }
        public double Avg { get; set; }
    }

    public class TemperatureStatistics : TemperatureStatisticsWithNoDetails
    {
        public List<TemperatureDetails> DETAILS { get; set; } = new List<TemperatureDetails>();

        public void AddDetail(TemperatureDetails detail)
        {
            DETAILS.Add(detail);

            TEMPERATURA_ACCUMULO.Max = DETAILS.Max(d => d.TEMPERATURA_ACCUMULO);
            TEMPERATURA_ACCUMULO.Min = DETAILS.Min(d => d.TEMPERATURA_ACCUMULO);
            TEMPERATURA_ACCUMULO.Avg = DETAILS.Average(d => d.TEMPERATURA_ACCUMULO);

            TEMPERATURA_PANNELLI.Max = DETAILS.Max(d => d.TEMPERATURA_PANNELLI);
            TEMPERATURA_PANNELLI.Min = DETAILS.Min(d => d.TEMPERATURA_PANNELLI);
            TEMPERATURA_PANNELLI.Avg = DETAILS.Average(d => d.TEMPERATURA_PANNELLI);

            TEMPERATURA_CAMINO.Max = DETAILS.Max(d => d.TEMPERATURA_CAMINO);
            TEMPERATURA_CAMINO.Min = DETAILS.Min(d => d.TEMPERATURA_CAMINO);
            TEMPERATURA_CAMINO.Avg = DETAILS.Average(d => d.TEMPERATURA_CAMINO);

            TEMPERATURA_ACCUMULO_INFERIORE.Max = DETAILS.Max(d => d.TEMPERATURA_ACCUMULO_INFERIORE);
            TEMPERATURA_ACCUMULO_INFERIORE.Min = DETAILS.Min(d => d.TEMPERATURA_ACCUMULO_INFERIORE);
            TEMPERATURA_ACCUMULO_INFERIORE.Avg = DETAILS.Average(d => d.TEMPERATURA_ACCUMULO_INFERIORE);
        }
    }
}

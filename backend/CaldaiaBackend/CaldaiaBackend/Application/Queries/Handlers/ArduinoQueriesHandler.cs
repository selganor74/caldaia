using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Services;
using Infrastructure.Actions.Query.Handler;

namespace CaldaiaBackend.Application.Queries.Handlers
{
    public class ArduinoQueriesHandler :    IQueryHandler<GetLatestDataQuery, DataFromArduino>,
                                            IQueryHandler<GetLast24HoursStatisticsQuery, string>,
                                            IQueryHandler<GetLast24HoursTemperaturesStatisticsQuery, string>
    {
        private readonly IArduinoDataReader _reader;
        private readonly Last24Hours _last24HoursProjection;
        private readonly Last24HoursTemperatures _last24HoursTempsProjection;

        public ArduinoQueriesHandler(
            IArduinoDataReader reader,
            Last24Hours last24HoursProjection,
            Last24HoursTemperatures last24HoursTempsProjection
            )
        {
            _reader = reader;
            _last24HoursProjection = last24HoursProjection;
            _last24HoursTempsProjection = last24HoursTempsProjection;
        }
        public DataFromArduino Execute(GetLatestDataQuery Action)
        {
            return _reader.Latest;
        }

        public string Execute(GetLast24HoursStatisticsQuery Action)
        {
            return _last24HoursProjection.GetCurrentStatisticsAsJson();
        }

        public string Execute(GetLast24HoursTemperaturesStatisticsQuery Action)
        {
            return _last24HoursTempsProjection.GetCurrentStatisticsAsJson();
        }
    }
}
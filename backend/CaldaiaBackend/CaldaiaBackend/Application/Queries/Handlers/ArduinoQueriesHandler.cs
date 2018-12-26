using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Services;
using Infrastructure.Actions.Query.Handler;

namespace CaldaiaBackend.Application.Queries.Handlers
{
    public class ArduinoQueriesHandler :    IQueryHandler<GetLatestDataQuery, DataFromArduino>,
                                            IQueryHandler<GetLast24HoursStatisticsQuery, string>
    {
        private IArduinoDataReader _reader;
        private Last24Hours _last24HoursProjection;

        public ArduinoQueriesHandler(
            IArduinoDataReader reader,
            Last24Hours last24HoursProjection
            )
        {
            _reader = reader;
            _last24HoursProjection = last24HoursProjection;
        }
        public DataFromArduino Execute(GetLatestDataQuery Action)
        {
            return _reader.Latest;
        }

        public string Execute(GetLast24HoursStatisticsQuery Action)
        {
            return _last24HoursProjection.GetCurrentStatisticsAsJson();
        }
    }
}
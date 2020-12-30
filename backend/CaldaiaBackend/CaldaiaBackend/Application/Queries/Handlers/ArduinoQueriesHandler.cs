using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Services;
using Infrastructure.Actions;
using Infrastructure.Application;

namespace CaldaiaBackend.Application.Queries.Handlers
{
    public class ArduinoQueriesHandler : IQueryHandler<GetLatestDataQuery, DataFromArduino>,
                                            IQueryHandler<GetLast24HoursAccumulatorsStatisticsQuery, string>,
                                            IQueryHandler<GetLast24HoursTemperaturesStatisticsQuery, string>,
                                            IQueryHandler<GetLastWeekAccumulatorsStatisticsQuery, string>,
                                            IQueryHandler<GetLastWeekTemperaturesStatisticsQuery, string>
    {
        private readonly IArduinoDataReader _reader;
        private readonly Last24HoursAccumulators _last24HoursProjection;
        private readonly Last24HoursTemperatures _last24HoursTempsProjection;
        private readonly LastWeekAccumulators _lastWeekProjection;
        private readonly LastWeekTemperatures _lastWeekTempsProjection;

        public ArduinoQueriesHandler(
            IArduinoDataReader reader,
            Last24HoursAccumulators last24HoursProjection,
            Last24HoursTemperatures last24HoursTempsProjection,
            LastWeekAccumulators lastWeekProjection,
            LastWeekTemperatures lastWeekTempsProjection
            )
        {
            _reader = reader;
            _last24HoursProjection = last24HoursProjection;
            _last24HoursTempsProjection = last24HoursTempsProjection;
            _lastWeekProjection = lastWeekProjection;
            _lastWeekTempsProjection = lastWeekTempsProjection;
        }

        public DataFromArduino Execute(GetLatestDataQuery Action, IExecutionContext context)
        {
            return _reader.Latest;
        }

        public string Execute(GetLast24HoursAccumulatorsStatisticsQuery Action, IExecutionContext context)
        {
            return _last24HoursProjection.GetCurrentStatisticsAsJson();
        }

        public string Execute(GetLast24HoursTemperaturesStatisticsQuery Action, IExecutionContext context)
        {
            return _last24HoursTempsProjection.GetCurrentStatisticsAsJson();
        }

        public string Execute(GetLastWeekAccumulatorsStatisticsQuery Action, IExecutionContext context)
        {
            return _lastWeekProjection.GetCurrentStatisticsAsJson();
        }

        public string Execute(GetLastWeekTemperaturesStatisticsQuery Action, IExecutionContext context)
        {
            return _lastWeekTempsProjection.GetCurrentStatisticsAsJson();
        }
    }
}
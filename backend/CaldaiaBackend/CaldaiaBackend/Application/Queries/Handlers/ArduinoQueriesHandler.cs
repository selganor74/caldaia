using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Interfaces;
using Infrastructure.Actions.Query.Handler;

namespace CaldaiaBackend.Application.Queries.Handlers
{
    public class ArduinoQueriesHandler : IQueryHandler<GetLatestDataQuery, DataFromArduino>
    {
        private IArduinoDataReader _reader;

        public ArduinoQueriesHandler(IArduinoDataReader reader)
        {
            _reader = reader;
        }
        public DataFromArduino Execute(GetLatestDataQuery Action)
        {
            return _reader.Latest;
        }
    }
}
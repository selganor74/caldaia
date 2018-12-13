using CaldaiaBackend.Application.DataModels;
using Infrastructure.Actions.Query;

namespace CaldaiaBackend.Application.Queries
{
    /// <summary>
    /// Returns the Latest Data read by the ArduinoDataReader.
    /// </summary>
    public class GetLatestDataQuery : IQuery<DataFromArduino>
    {
    }
}

using System.Web.Http;
using CaldaiaBackend.Application.Queries;
using Infrastructure.Actions;

namespace CaldaiaBackend.SelfHosted.Controllers
{
    public class StatisticsController : System.Web.Http.ApiController
    {
        private readonly IQueryExecutor _queryExecutor;

        public StatisticsController(
            IQueryExecutor queryExecutor
            )
        {
            _queryExecutor = queryExecutor;
        }

        [HttpGet]
        [Route("api/statistics/last-24-hours")]
        public IHttpActionResult GetLast24HoursStats()
        {
            var query = new GetLast24HoursAccumulatorsStatisticsQuery();
            var toReturn = _queryExecutor.ExecuteQuery<GetLast24HoursAccumulatorsStatisticsQuery,string>(query);

            return Ok(toReturn);
        }

        [HttpGet]
        [Route("api/statistics/last-24-hours-temperatures")]
        public IHttpActionResult GetLast24HoursTempStats()
        {
            var query = new GetLast24HoursTemperaturesStatisticsQuery();
            var toReturn = _queryExecutor.ExecuteQuery<GetLast24HoursTemperaturesStatisticsQuery, string>(query);

            return Ok(toReturn);
        }

        [HttpGet]
        [Route("api/statistics/last-week-accumulators")]
        public IHttpActionResult GetLastWeekAccumulatorsStats()
        {
            var query = new GetLastWeekAccumulatorsStatisticsQuery();
            var toReturn = _queryExecutor.ExecuteQuery<GetLastWeekAccumulatorsStatisticsQuery, string>(query);

            return Ok(toReturn);
        }

        [HttpGet]
        [Route("api/statistics/last-week-temperatures")]
        public IHttpActionResult GetLastWeekTempStats()
        {
            var query = new GetLastWeekTemperaturesStatisticsQuery();
            var toReturn = _queryExecutor.ExecuteQuery<GetLastWeekTemperaturesStatisticsQuery, string>(query);

            return Ok(toReturn);
        }
    }
}

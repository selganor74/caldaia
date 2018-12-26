using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CaldaiaBackend.Application.Commands;
using CaldaiaBackend.Application.Queries;
using Infrastructure.Actions.Command.Handler;
using Infrastructure.Application;

namespace CaldaiaBackend.SelfHosted.Controllers
{
    public class StatisticsController : System.Web.Http.ApiController
    {
        private readonly IApplication _arduinoApp;

        public StatisticsController(IApplication arduinoApp, ICommandHandler<ReadDataFromArduinoCommand> readDataHandler)
        {
            _arduinoApp = arduinoApp;
        }

        [HttpGet]
        [Route("api/statistics/last-24-hours")]
        public IHttpActionResult GetLast24HoursStats()
        {
            var query = new GetLast24HoursStatisticsQuery();
            var toReturn = _arduinoApp.ExecuteQuery<GetLast24HoursStatisticsQuery,string>(query);

            return Ok(toReturn);
        }
    }
}

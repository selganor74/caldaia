using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CaldaiaBackend.Application.Commands;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Queries;
using Infrastructure.Actions;

namespace CaldaiaBackend.SelfHosted.Controllers
{
    public class ApiController : System.Web.Http.ApiController
    {
        private readonly ICommandExecutor _cmdExecutor;
        private readonly IQueryExecutor _queryExecutor;

        public ApiController(
            ICommandExecutor cmdExecutor,
            IQueryExecutor queryExecutor
            )
        {
            _cmdExecutor = cmdExecutor;
            _queryExecutor = queryExecutor;
        }

        [HttpGet]
        [Route("")]
        public HttpResponseMessage RedirectToApp()
        {
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(Request.RequestUri, "app/index.html");
            return response;
        }

        [HttpGet]
        [Route("api/queries/latestdata")]
        public IHttpActionResult LatestData()
        {
            var qry = new GetLatestDataQuery();
            var data = _queryExecutor.ExecuteQuery<GetLatestDataQuery, DataFromArduino>(qry);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/queries/runtimesettings")]
        public IHttpActionResult RuntimeSettings()
        {
            var qry = new GetRuntimeSettingsQuery();
            var data = _queryExecutor.ExecuteQuery<GetRuntimeSettingsQuery, SettingsFromArduino>(qry);
            return Ok(data);
        }

        [HttpPost]
        [Route("api/commands/get")]
        public IHttpActionResult UpdateLatestData()
        {
            var cmd = new ReadDataFromArduinoCommand();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/commands/get-ra")]
        public IHttpActionResult ResetAccumulators()
        {
            var cmd = new ReadDataAndResetAccumulatorsCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/commands/reloadsettings")]
        public IHttpActionResult ReloadSettings()
        {
            var cmd = new ReadSettingsFromArduinoCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/settings/save")]
        public IHttpActionResult SaveSettings()
        {
            var cmd = new SaveSettingsCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/settings/rotex-termo-min/decrement")]
        public IHttpActionResult DecrementRotexTermoMin()
        {
            var cmd = new DecrementRotexTermoMinCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/settings/rotex-termo-min/increment")]
        public IHttpActionResult IncrementRotexTermoMin()
        {
            var cmd = new IncrementRotexTermoMinCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/settings/rotex-termo-max/decrement")]
        public IHttpActionResult DecrementRotexTermoMax()
        {
            var cmd = new DecrementRotexTermoMaxCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/settings/rotex-termo-max/increment")]
        public IHttpActionResult IncrementRotexTermoMax()
        { 
            var cmd = new IncrementRotexTermoMaxCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/commands/pause-poller")]
        public IHttpActionResult PausePoller([FromBody] PausePollerCommand cmd)
        {
            // _readDataHandler.Execute(cmd).Wait();
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

        [HttpPost]
        [Route("api/commands/send-string")]
        public IHttpActionResult SendString([FromBody] SendStringCommand cmd)
        {
            _cmdExecutor.ExecuteCommand(cmd);
            return Ok();
        }

    }
}

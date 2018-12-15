using System.Web.Http;
using CaldaiaBackend.Application.Commands;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Queries;
using Infrastructure.Application;
using Infrastructure.Actions.Command.Handler;

namespace CaldaiaBackend.SelfHosted.Controllers
{
    public class ApiController : System.Web.Http.ApiController
    {
        private readonly IApplication _arduinoApp;
        private ICommandHandler<ReadDataFromArduinoCommand> _readDataHandler;

        public ApiController(IApplication arduinoApp, ICommandHandler<ReadDataFromArduinoCommand> readDataHandler)
        {
            _arduinoApp = arduinoApp;
            _readDataHandler = readDataHandler;
        }

        [HttpGet]
        [Route("api/queries/latestdata")]
        public IHttpActionResult LatestData()
        {
            var qry = new GetLatestDataQuery();
            var data = _arduinoApp.ExecuteQuery<GetLatestDataQuery, DataFromArduino>(qry);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/queries/runtimesettings")]
        public IHttpActionResult RuntimeSettings()
        {
            var qry = new GetRuntimeSettingsQuery();
            var data = _arduinoApp.ExecuteQuery<GetRuntimeSettingsQuery, SettingsFromArduino>(qry);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/commands/get")]
        public IHttpActionResult UpdateLatestData()
        {
            var cmd = new ReadDataFromArduinoCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _arduinoApp.Execute(cmd);
            return Ok();
        }

        [HttpGet]
        [Route("api/commands/get-ra")]
        public IHttpActionResult ResetAccumulators()
        {
            var cmd = new ReadDataAndResetAccumulatorsCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _arduinoApp.Execute(cmd);
            return Ok();
        }

        [HttpGet]
        [Route("api/commands/reloadsettings")]
        public IHttpActionResult ReloadSettings()
        {
            var cmd = new ReadSettingsFromArduinoCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _arduinoApp.Execute(cmd);
            return Ok();
        }

    }
}

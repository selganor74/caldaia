using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CaldaiaBackend.Application.Commands;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Queries;
using Infrastructure.Application;
using Infrastructure.Actions.Command.Handler;

namespace CaldaiaBackend.SelfHosted.Controllers
{
    public class ArduinoController : ApiController
    {
        private IApplication _arduinoApp;
        private ICommandHandler<ReadDataFromArduinoCommand> _readDataHandler;

        public ArduinoController(IApplication arduinoApp, ICommandHandler<ReadDataFromArduinoCommand> readDataHandler)
        {
            _arduinoApp = arduinoApp;
            _readDataHandler = readDataHandler;
        }

        [HttpGet]
        [Route("api/arduino/latestdata")]
        public IHttpActionResult LatestData()
        {
            var qry = new GetLatestDataQuery();
            var data = _arduinoApp.ExecuteQuery<GetLatestDataQuery, DataFromArduino>(qry);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/arduino/commands/get")]
        public IHttpActionResult ExecuteGetCommand()
        {
            var cmd = new ReadDataFromArduinoCommand();
            // _readDataHandler.Execute(cmd).Wait();
            _arduinoApp.Execute(cmd);
            return Ok();
        }

    }
}

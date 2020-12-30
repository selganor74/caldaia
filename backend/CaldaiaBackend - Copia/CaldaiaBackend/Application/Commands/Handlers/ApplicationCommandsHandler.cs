using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Actions.Command.Handler;

namespace CaldaiaBackend.Application.Commands.Handlers
{
    public class ApplicationCommandsHandler : ICommandHandler<PausePollerCommand>
    {
        private readonly ArduinoBackendApplication _application;
        private Task _emptyTask => Task.Run(() => { });
        public ApplicationCommandsHandler(
            ArduinoBackendApplication application
            )
        {
            _application = application;
        }

        public Task Execute(PausePollerCommand Action)
        {
            _application.PausePollerForSeconds(Action.PauseForSeconds);
            return _emptyTask;
        }
    }
}

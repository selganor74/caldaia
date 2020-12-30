using Infrastructure.Actions;
using Infrastructure.Application;

namespace CaldaiaBackend.Application.Commands.Handlers
{
    public class ApplicationCommandsHandler : ICommandHandler<PausePollerCommand>
    {
        private readonly ArduinoBackendApplication _application;

        public ApplicationCommandsHandler(
            ArduinoBackendApplication application
            )
        {
            _application = application;
        }

        public Void Execute(PausePollerCommand action, IExecutionContext context)
        {
            _application.PausePollerForSeconds(action.PauseForSeconds);
            return Void.Result;
        }
    }
}

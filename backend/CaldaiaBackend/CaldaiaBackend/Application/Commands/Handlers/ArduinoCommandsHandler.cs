using System.Threading.Tasks;
using CaldaiaBackend.Application.Interfaces;
using Infrastructure.Actions.Command.Handler;

namespace CaldaiaBackend.Application.Commands.Handlers
{
    public class ArduinoCommandsHandler : ICommandHandler<ReadDataFromArduinoCommand>
    {
        private IArduinoCommandIssuer _arduino;

        public ArduinoCommandsHandler(IArduinoCommandIssuer arduino)
        {
            _arduino = arduino;
        }

        public Task Execute(ReadDataFromArduinoCommand Action)
        {
            return Task.Run(() => _arduino.SendGetCommand());
        }
    }
}

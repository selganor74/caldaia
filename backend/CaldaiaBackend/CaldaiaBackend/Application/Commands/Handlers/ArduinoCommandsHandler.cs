using System.Threading.Tasks;
using CaldaiaBackend.Application.Interfaces;
using Infrastructure.Actions.Command.Handler;

namespace CaldaiaBackend.Application.Commands.Handlers
{
    public class ArduinoCommandsHandler :   ICommandHandler<ReadDataFromArduinoCommand>,
                                            ICommandHandler<ReadSettingsFromArduinoCommand>
    {
        private IArduinoCommandIssuer _arduino;

        public ArduinoCommandsHandler(IArduinoCommandIssuer arduino)
        {
            _arduino = arduino;
        }

        public Task Execute(ReadDataFromArduinoCommand Action)
        {
            var toReturn = Task.Run(() => { });

            _arduino.SendGetCommand();

            return toReturn;
        }


        public Task Execute(ReadSettingsFromArduinoCommand Action)
        {
            var toReturn = Task.Run(() => { });

            _arduino.SendGetRunTimeSettingsCommand();

            return toReturn;
        }
    }
}

using System;
using System.Threading.Tasks;
using CaldaiaBackend.Application.Interfaces;
using Infrastructure.Actions.Command.Handler;

namespace CaldaiaBackend.Application.Commands.Handlers
{
    public class ArduinoCommandsHandler :   ICommandHandler<ReadDataFromArduinoCommand>,
                                            ICommandHandler<ReadSettingsFromArduinoCommand>,
                                            ICommandHandler<IncrementRotexTermoMinCommand>,
                                            ICommandHandler<DecrementRotexTermoMinCommand>,
                                            ICommandHandler<IncrementRotexTermoMaxCommand>,
                                            ICommandHandler<DecrementRotexTermoMaxCommand>
    {
        private IArduinoCommandIssuer _arduino;

        public ArduinoCommandsHandler(IArduinoCommandIssuer arduino)
        {
            _arduino = arduino;
        }

        public Task Execute(ReadDataFromArduinoCommand Action)
        {
            var toReturn = Task.Run(() => { });

            _arduino.PullOutData();

            return toReturn;
        }


        public Task Execute(ReadSettingsFromArduinoCommand Action)
        {
            var toReturn = Task.Run(() => { });

            _arduino.PullOutSettings();

            return toReturn;
        }

        public Task Execute(IncrementRotexTermoMaxCommand Action)
        {
            var toReturn = Task.Run(() => { });

            _arduino.IncrementRotexTermoMax();

            return toReturn;
        }

        public Task Execute(DecrementRotexTermoMaxCommand Action)
        {
            var toReturn = Task.Run(() => { });

            _arduino.DecrementRotexTermoMax();

            return toReturn;
        }

        public Task Execute(DecrementRotexTermoMinCommand Action)
        {
            var toReturn = Task.Run(() => { });

            _arduino.DecrementRotexTermoMin();

            return toReturn;
        }

        public Task Execute(IncrementRotexTermoMinCommand Action)
        {
            var toReturn = Task.Run(() => { });

            _arduino.IncrementRotexTermoMin();

            return toReturn;
        }
    }
}

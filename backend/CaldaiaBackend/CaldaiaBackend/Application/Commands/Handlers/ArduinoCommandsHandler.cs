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
                                            ICommandHandler<DecrementRotexTermoMaxCommand>,
                                            ICommandHandler<SaveSettingsCommand>,
                                            ICommandHandler<SendStringCommand>
    {
        private readonly IArduinoCommandIssuer _arduino;

        private Task EmptyTask => Task.Run(() => { });
        public ArduinoCommandsHandler(IArduinoCommandIssuer arduino)
        {
            _arduino = arduino;
        }

        public Task Execute(ReadDataFromArduinoCommand Action)
        {
            _arduino.PullOutData();

            return EmptyTask;
        }


        public Task Execute(ReadSettingsFromArduinoCommand Action)
        {
            _arduino.PullOutSettings();

            return EmptyTask;
        }

        public Task Execute(IncrementRotexTermoMaxCommand Action)
        {
            _arduino.IncrementRotexTermoMax();

            return EmptyTask;
        }

        public Task Execute(DecrementRotexTermoMaxCommand Action)
        {
            _arduino.DecrementRotexTermoMax();

            return EmptyTask;
        }

        public Task Execute(DecrementRotexTermoMinCommand Action)
        {
            _arduino.DecrementRotexTermoMin();

            return EmptyTask;
        }

        public Task Execute(IncrementRotexTermoMinCommand Action)
        {
            _arduino.IncrementRotexTermoMin();

            return EmptyTask;
        }

        public Task Execute(SaveSettingsCommand Action)
        {
            _arduino.SaveSettings();

            return EmptyTask;
        }

        public Task Execute(SendStringCommand Action)
        {
            _arduino.SendString(Action.ToSend);

            return EmptyTask;
        }
    }
}

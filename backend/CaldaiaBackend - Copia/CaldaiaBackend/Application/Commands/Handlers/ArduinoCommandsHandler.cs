﻿using System.Threading.Tasks;
using CaldaiaBackend.Application.Services;
using Infrastructure.Actions.Command.Handler;

namespace CaldaiaBackend.Application.Commands.Handlers
{
    public class ArduinoCommandsHandler :   ICommandHandler<ReadDataFromArduinoCommand>,
                                            ICommandHandler<ReadDataAndResetAccumulatorsCommand>,
                                            ICommandHandler<ReadSettingsFromArduinoCommand>,
                                            ICommandHandler<IncrementRotexTermoMinCommand>,
                                            ICommandHandler<DecrementRotexTermoMinCommand>,
                                            ICommandHandler<IncrementRotexTermoMaxCommand>,
                                            ICommandHandler<DecrementRotexTermoMaxCommand>,
                                            ICommandHandler<SaveSettingsCommand>,
                                            ICommandHandler<SendStringCommand>,
                                            ICommandHandler<ResetArduinoCommand>
    {
        private readonly IArduinoCommandIssuer _arduino;

        private static Task EmptyTask => Task.Run(() => { });

        public ArduinoCommandsHandler(IArduinoCommandIssuer arduino)
        {
            _arduino = arduino;
        }

        public Task Execute(ReadDataFromArduinoCommand Action)
        {
            _arduino.PullOutData();

            return EmptyTask;
        }

        public Task Execute(ReadDataAndResetAccumulatorsCommand Action)
        {
            _arduino.SendGetAndResetAccumulatorsCommand();

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
            if (Action.ToSend == "RESET")
                _arduino.FlashDTR();
            else
                _arduino.SendString(Action.ToSend);

            return EmptyTask;
        }

        public Task Execute(ResetArduinoCommand Action)
        {
            _arduino.FlashDTR();
            return EmptyTask;
        }
    }
}

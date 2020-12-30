using CaldaiaBackend.Application.Services;
using Infrastructure.Actions;
using Infrastructure.Application;

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

        public ArduinoCommandsHandler(IArduinoCommandIssuer arduino)
        {
            _arduino = arduino;
        }

        public Void Execute(ReadDataFromArduinoCommand Action, IExecutionContext context)
        {
            _arduino.PullOutData();

            return Void.Result;
        }

        public Void Execute(ReadDataAndResetAccumulatorsCommand Action, IExecutionContext context)
        {
            _arduino.SendGetAndResetAccumulatorsCommand();

            return Void.Result;
        }

        public Void Execute(ReadSettingsFromArduinoCommand Action, IExecutionContext context)
        {
            _arduino.PullOutSettings();

            return Void.Result;
        }

        public Void Execute(IncrementRotexTermoMaxCommand Action, IExecutionContext context)
        {
            _arduino.IncrementRotexTermoMax();

            return Void.Result;
        }

        public Void Execute(DecrementRotexTermoMaxCommand Action, IExecutionContext context)
        {
            _arduino.DecrementRotexTermoMax();

            return Void.Result;
        }

        public Void Execute(DecrementRotexTermoMinCommand Action, IExecutionContext context)
        {
            _arduino.DecrementRotexTermoMin();

            return Void.Result;
        }

        public Void Execute(IncrementRotexTermoMinCommand Action, IExecutionContext context)
        {
            _arduino.IncrementRotexTermoMin();

            return Void.Result;
        }

        public Void Execute(SaveSettingsCommand Action, IExecutionContext context)
        {
            _arduino.SaveSettings();

            return Void.Result;
        }

        public Void Execute(SendStringCommand Action, IExecutionContext context)
        {
            if (Action.ToSend == "RESET")
                _arduino.FlashDTR();
            else
                _arduino.SendString(Action.ToSend);

            return Void.Result;
        }

        public Void Execute(ResetArduinoCommand Action, IExecutionContext context)
        {
            _arduino.FlashDTR();
            return Void.Result;
        }
    }
}

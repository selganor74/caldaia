using Infrastructure.Actions;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Reset The arduino Board by flashing the DTR line on the (virtual) serial port.
    /// </summary>
    public class ResetArduinoCommand : ICommand
    {
    }
}

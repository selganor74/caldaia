using System;
using Infrastructure.Actions.Command;

namespace CaldaiaBackend.Application.Commands
{
    public class ReadSettingsFromArduinoCommand : ICommand
    {
        public string CommandId { get; } = Guid.NewGuid().ToString();
    }
}

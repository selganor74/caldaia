using System;
using Infrastructure.Actions.Command;

namespace CaldaiaBackend.Application.Commands
{
    /// <summary>
    /// Send a GET command to Arduino. Will cause the reader to update its latest data
    /// </summary>
    public class ReadDataFromArduinoCommand : ICommand
    {
        public string CommandId { get; } = Guid.NewGuid().ToString();
    }
}

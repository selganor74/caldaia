using Infrastructure.Actions.Command;

/// <summary>
/// Salva i settaggi correnti nella EEPROM di Arduino
/// </summary>
namespace CaldaiaBackend.Application.Commands
{
    public class SaveSettingsCommand : ICommand
    {
        public string CommandId { get; }
    }
}

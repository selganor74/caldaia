namespace CaldaiaBackend.Application.Interfaces
{
    public interface IArduinoCommandIssuer
    {
        void SendGetCommand();
        void SendGetAndResetAccumulatorsCommand();
        void SendGetRunTimeSettingsCommand();
    }
}
namespace CaldaiaBackend.Application.Services
{
    public interface IArduinoCommandIssuer
    {
        void PullOutData();
        void SendGetAndResetAccumulatorsCommand();
        void PullOutSettings();
        void IncrementRotexTermoMax();
        void DecrementRotexTermoMax();
        void DecrementRotexTermoMin();
        void IncrementRotexTermoMin();
        void SaveSettings();
        void SendString(string toSend);
    }
}
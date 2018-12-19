namespace CaldaiaBackend.Application.Interfaces
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
    }
}
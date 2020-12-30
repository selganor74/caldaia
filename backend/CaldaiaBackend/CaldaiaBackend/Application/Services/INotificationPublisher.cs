namespace CaldaiaBackend.Application.Services
{
    public interface INotificationPublisher
    {
        void Notify<TData>(string channel, TData data);
    }
}

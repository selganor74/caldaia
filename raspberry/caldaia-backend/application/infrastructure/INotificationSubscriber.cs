namespace application.infrastructure;

public interface INotificationSubscriber
{
    void Subscribe(string channel, Action<object> handler);
    void Subscribe<TEvent>(string channel, Action<TEvent> handler);
}

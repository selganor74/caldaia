namespace application.infrastructure;

public interface INotificationPublisher
{
    public void Publish<TEvent>(string channel, TEvent data);
}

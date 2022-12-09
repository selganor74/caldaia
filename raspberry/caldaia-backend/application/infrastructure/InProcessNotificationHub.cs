using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace application.infrastructure;

public class InProcessNotificationHub : INotificationSubscriber, INotificationPublisher
{
    private readonly ILogger<InProcessNotificationHub> log;

    private static Dictionary<string, Dictionary<Type, List<Action<object>>>> handlers = new Dictionary<string, Dictionary<Type, List<Action<object>>>>();

    private static Dictionary<string, List<Action<object>>> channelHandlers = new Dictionary<string, List<Action<object>>>();

    public InProcessNotificationHub(
        ILogger<InProcessNotificationHub> log
        )
    {
        this.log = log;
    }
    public void Publish<TEvent>(string channel, TEvent data)
    {
        log.LogTrace($"Publishing {typeof(TEvent).Name} in channel {channel}.{Environment.NewLine}{JsonConvert.SerializeObject(data, Formatting.Indented)}");
        lock (handlers)
        {
            PublishForChannel(channel, data);
            PublishForChannelAndType(channel, data);
        }
    }

    private void PublishForChannel(string channel, object data)
    {
        if (!channelHandlers.ContainsKey(channel))
            return;

        var hdlrs = channelHandlers[channel];
        foreach (var h in hdlrs)
        {
            try
            {
                h.Invoke(data);
            }
            catch (Exception e)
            {
                log.LogError($"{nameof(PublishForChannel)}: Errors while publishing event in channel {channel}.{Environment.NewLine}{JsonConvert.SerializeObject(data, Formatting.Indented)}{Environment.NewLine}{e}");
            }
        }
    }

    private void PublishForChannelAndType<TEvent>(string channel, TEvent data)
    {
        if (!handlers.ContainsKey(channel))
            return;

        var handlersForChannel = handlers[channel];

        if (!handlersForChannel.ContainsKey(typeof(TEvent)))
            return;

        var handlersList = handlersForChannel[typeof(TEvent)];
        foreach (var h in handlersList)
        {
            try
            {
                h.Invoke(data);
            }
            catch (Exception e)
            {
                log.LogError($"{nameof(PublishForChannelAndType)}: Errors while publishing event {nameof(TEvent)} in channel {channel}.{Environment.NewLine}{JsonConvert.SerializeObject(data, Formatting.Indented)}{Environment.NewLine}{e}");
            }
        }
    }

    public void Subscribe(string channel, Action<object> handler)
    {
        log.LogInformation($"Subscribing handler on all types in channel {channel}");
        lock (handlers)
        {
            if (!channelHandlers.ContainsKey(channel))
                channelHandlers.Add(channel, new List<Action<object>>());

            channelHandlers[channel].Add((e) => { handler(e); });
        }
    }

    public void Subscribe<TEvent>(string channel, Action<TEvent> handler)
    {
        log.LogInformation($"Subscribing handler on type {typeof(TEvent).Name} in channel {channel}");
        lock (handlers)
        {

            if (!handlers.ContainsKey(channel))
                handlers.Add(channel, new Dictionary<Type, List<Action<object>>>());

            var handlersForChannel = handlers[channel];

            if (!handlersForChannel.ContainsKey(typeof(TEvent)))
                handlersForChannel.Add(typeof(TEvent), new List<Action<object>>());

            var handlersList = handlersForChannel[typeof(TEvent)];
            handlersList.Add((e) => { handler((TEvent)e); });
        }
    }
}

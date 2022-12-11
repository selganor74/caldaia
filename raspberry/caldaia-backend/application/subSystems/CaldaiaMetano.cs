using application.infrastructure;
using domain.systemComponents;
using domain.systemComponents.mocks;
using Microsoft.Extensions.Logging;

namespace application.subSystems;

public class CaldaiaMetano : Subsystem
{
    public DigitalOutput RELAY_ACCENSIONE_CALDAIA { get; set; }

    public CaldaiaMetano(
        INotificationPublisher hub,
        ILogger<CaldaiaMetano> log
    ) : base(hub, log)
    {
        var relay = new MockDigitalOutput(
            nameof(RELAY_ACCENSIONE_CALDAIA),
            log
        );

        relay.SetToOff("init");
        RELAY_ACCENSIONE_CALDAIA = relay;
    }
}
#pragma warning restore CS8618

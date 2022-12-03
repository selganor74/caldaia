using application.infrastructure;
using domain.systemComponents;

namespace application.subSystems;

public class CaldaiaMetano : IDisposable
{
    public DigitalOutput RELAY_ACCENSIONE_CALDAIA { get; set; }

    public CaldaiaMetano(DigitalOutput rELAY_ACCENSIONE_CALDAIA)
    {
        RELAY_ACCENSIONE_CALDAIA = rELAY_ACCENSIONE_CALDAIA;
    }

    public void Dispose()
    {
        this.DisposeDisposables(null);
    }
}
#pragma warning restore CS8618

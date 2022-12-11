using Microsoft.Extensions.Logging;

namespace domain.systemComponents.mocks;

public class MockDigitalOutput : DigitalOutput
{
    public MockDigitalOutput(string name, ILogger log) : base(name, log)
    {
    }

    protected override void SetToOffImplementation()
    {
        // Does really nothing !        
    }

    protected override void SetToOnImplementation()
    {
        // Does really nothing !
    }
}

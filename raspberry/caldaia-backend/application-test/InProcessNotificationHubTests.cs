using application.infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace application_test;

public class InProcessNotificationHubTests
{
    [Test]
    public void All_handlers_must_be_called()
    {
        var nh = new InProcessNotificationHub(new NullLogger<InProcessNotificationHub>());
        var channelOnly = false;
        var channelAndType = false;

        nh.Subscribe("channel", (a) => { channelOnly = true; });
        nh.Subscribe<SampleData>("channel", (a) => { channelAndType = true; });

        nh.Publish("wrong-channel", new SampleData() { SomeData = "Some Text" });
        Assert.IsFalse(channelOnly);
        Assert.IsFalse(channelAndType);

        nh.Publish("channel", new SampleData() { SomeData = "Some Text" });
        Assert.IsTrue(channelOnly);
        Assert.IsTrue(channelAndType);
    }
}

internal class SampleData
{
    public string SomeData { get; set; }
}
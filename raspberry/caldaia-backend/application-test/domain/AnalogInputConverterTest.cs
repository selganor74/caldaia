using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using Microsoft.Extensions.Logging.Abstractions;
using raspberry_gpio;

namespace application_test;

public class AnalogInputConverterTest
{
    [Test]
    public void IceBreaker()
    {
        Assert.Pass();
    }

    [Test]
    public void AnalogInputConverter_must_convert_at_each_ValueChanged_event()
    {
        var input = new MockAnalogInput("mock input", new NullLogger<MockAnalogInput>());
        var converter = new AnalogInputConverter<Temperature>("sut", input, (inmeasure) =>
        {
            return inmeasure * 2m;
        },
        new NullLogger<AnalogInput>()
        );

        input.SetInput(new PureNumber(1m));
        Assert.IsTrue((converter.LastMeasure?.Value ?? 0m) == 2m);
    }

    [Test]
    public void NtcVulcanoConverter_must_never_throw_an_OverflowException()
    {
        var input = new MockAnalogInput("mock input", new NullLogger<MockAnalogInput>());
        var sut = new NtcVulcanoConverter("sut", input, new NullLogger<NtcVulcanoConverter>());
        for(var testInput = -32767m; testInput <=32767m; testInput++ ) {
            var newMeasure = new PureNumber(testInput);
            Assert.DoesNotThrow(() => input.SetInput(newMeasure));
            Assert.IsNull(sut.LastError, $"failed at testInput = {testInput}");
            Assert.IsNull(input.LastError, $"failed at testInput = {testInput}");
        }
    }
}

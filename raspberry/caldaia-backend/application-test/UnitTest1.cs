using System.Diagnostics;
using application;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace rotex_test;

public class Tests
{
    #pragma warning disable CS8618
    private CaldaiaIOSet io;
    private CaldaiaApplication application;
    #pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        this.io = new CaldaiaIOSet(
            rELAY_POMPA_CAMINO: new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_POMPA_CAMINO), new NullLogger<MockDigitalOutput>()),
            rELAY_BYPASS_TERMOSTATO_AMBIENTE: new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_BYPASS_TERMOSTATO_AMBIENTE), new NullLogger<MockDigitalOutput>()),
            rELAY_POMPA_RISCALDAMENTO: new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_POMPA_RISCALDAMENTO), new NullLogger<MockDigitalOutput>()),
            rELAY_CALDAIA: new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_CALDAIA), new NullLogger<MockDigitalOutput>()),
            tERMOSTATO_AMBIENTI: new MockDigitalInput(nameof(CaldaiaIOSet.TERMOSTATO_AMBIENTI), new NullLogger<MockDigitalInput>()),
            tERMOSTATO_ROTEX: new MockDigitalInput(nameof(CaldaiaIOSet.TERMOSTATO_ROTEX), new NullLogger<MockDigitalInput>()),
            caminoTemp: new MockAnalogInput<Temperature>(nameof(CaldaiaIOSet.CaminoTemp), new NullLogger<MockAnalogInput<Temperature>>()),
            rotexTempAccumulo: new MockAnalogInput<Temperature>(nameof(CaldaiaIOSet.RotexTempAccumulo), new NullLogger<MockAnalogInput<Temperature>>()),
            rotexTempPannelli: new MockAnalogInput<Temperature>(nameof(CaldaiaIOSet.RotexTempPannelli), new NullLogger<MockAnalogInput<Temperature>>()),
            rotexStatoPompa: new MockDigitalInput(nameof(CaldaiaIOSet.RotexStatoPompa), new NullLogger<DigitalInput>())
        );

        this.application = new CaldaiaApplication(io, new NullLogger<CaldaiaApplication>());
    }

    [TearDown]
    public void TearDown()
    {
        application.Dispose();
    }

    [Test]
    public void Test1()
    {
        application.Start();
        ((MockDigitalInput)io.TERMOSTATO_ROTEX).StartSquareInput(TimeSpan.FromSeconds(15));
        var sw = Stopwatch.StartNew();
        while(sw.ElapsedMilliseconds < 20000) {
            Thread.Sleep(2000);
            var reading = io.ReadAll();
            Console.WriteLine(JsonConvert.SerializeObject(reading, Formatting.Indented));
        }
        Assert.Pass();
    }
}
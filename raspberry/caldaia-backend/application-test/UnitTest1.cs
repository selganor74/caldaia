using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

using application;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using application.subSystems;
using application.infrastructure;

namespace rotex_test;

public class Tests
{
#pragma warning disable CS8618
    private CaldaiaIOSet io;
    private CaldaiaApplication application;

    private InProcessNotificationHub notificationHub = new InProcessNotificationHub(new NullLogger<InProcessNotificationHub>());
    
#pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        notificationHub.Reset();
        var adcInput = new MockAnalogInput("caminoTemp ADC", new NullLogger<MockAnalogInput>());
        var caminoTemp = new AnalogInputConverter<Temperature>(
            nameof(CaldaiaIOSet.CAMINO_TEMPERATURA),
            adcInput,
            (sourceVal) =>
            {
                return sourceVal / 750m;
            },
            new NullLogger<AnalogInput>()
        );

        var camino_on_off = new ComparatorWithHysteresis(
            nameof(CaldaiaIOSet.CAMINO_ON_OFF),
            caminoTemp,
            45m,
            40m,
            OnOffLogic.OnWhenRaising,
            TimeSpan.FromSeconds(1),
            new NullLogger<ComparatorWithHysteresis>()
            );

        var relayPompaCamino = new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_POMPA_CAMINO), new NullLogger<MockDigitalOutput>());

        var camino = new Camino(notificationHub, new NullLogger<Camino>());

        var riscaldamento = new Riscaldamento(notificationHub, new NullLogger<Riscaldamento>());

        var rotex = new Rotex(notificationHub, new NullLogger<Rotex>());

        var caldaia = new CaldaiaMetano(notificationHub, new NullLogger<CaldaiaMetano>());

        this.io = new CaldaiaIOSet(
            cALDAIA: caldaia,
            rOTEX: rotex,
            cAMINO: camino,
            rISCALDAMENTO: riscaldamento
        );

        io.RELAY_BYPASS_TERMOSTATO_AMBIENTE.SetMinTimeBetweenToggles(TimeSpan.Zero);
        io.RELAY_CALDAIA.SetMinTimeBetweenToggles(TimeSpan.Zero);
        io.RELAY_POMPA_CAMINO.SetMinTimeBetweenToggles(TimeSpan.Zero);
        io.RELAY_POMPA_RISCALDAMENTO.SetMinTimeBetweenToggles(TimeSpan.Zero);

        var config = new CaldaiaConfig(mainLoopPeriod: TimeSpan.FromMilliseconds(1));
        
        this.application = new CaldaiaApplication(
            io: io,
            config: config,
            notificationHub: notificationHub,
            log: new NullLogger<CaldaiaApplication>()
            );
    }

    [TearDown]
    public void TearDown()
    {
        application.Stop();
        application.Dispose();
    }

    [Test]
    public void Test1()
    {
        application.Start();
        ((MockDigitalInput)io.TERMOSTATO_ROTEX).StartSquareInput(TimeSpan.FromMilliseconds(150));

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 400)
        {
            Thread.Sleep(1);
            var reading = io.ReadAll(null);
            Assert.IsTrue(reading.TERMOSTATO_ROTEX == reading.STATO_RELAY_CALDAIA);
            // Console.WriteLine(JsonConvert.SerializeObject(reading, Formatting.Indented));
        }
        Assert.Pass();
    }
}
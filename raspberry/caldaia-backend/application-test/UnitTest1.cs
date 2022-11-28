using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

using application;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using application.subSystems;

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
        var adcInput = new MockAnalogInput<Voltage>("caminoTemp ADC", new NullLogger<MockAnalogInput<Voltage>>());
        var caminoTemp = new AnalogInputConverter<Voltage, Temperature>(
            nameof(CaldaiaIOSet.CAMINO_TEMPERATURA),
            adcInput,
            (sourceVal) =>
            {
                return sourceVal / 750m;
            },
            new NullLogger<AnalogInput<Temperature>>()
        );

        var camino_on_off = new ComparatorWithHysteresis<Temperature>(
            nameof(CaldaiaIOSet.CAMINO_ON_OFF),
            caminoTemp,
            45m,
            40m,
            OnOffLogic.OnWhenRaising,
            TimeSpan.FromSeconds(1),
            new NullLogger<ComparatorWithHysteresis<Temperature>>()
            );

        var relayPompaCamino = new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_POMPA_CAMINO), new NullLogger<MockDigitalOutput>());

        var camino = new Camino(
            cAMINO_TEMPERATURA: caminoTemp,
            cAMINO_ON_OFF: camino_on_off,
            rELAY_POMPA_CAMINO: relayPompaCamino
        );

        var riscaldamento = new Riscaldamento(
            rELAY_BYPASS_TERMOSTATO_AMBIENTE: new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_BYPASS_TERMOSTATO_AMBIENTE), new NullLogger<MockDigitalOutput>()),
            rELAY_POMPA_RISCALDAMENTO: new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_POMPA_RISCALDAMENTO), new NullLogger<MockDigitalOutput>()),
            tERMOSTATO_AMBIENTI: new MockDigitalInput(nameof(CaldaiaIOSet.TERMOSTATO_AMBIENTI), new NullLogger<MockDigitalInput>()) 
        );

        var rotex = new Rotex(
            tERMOSTATO_ROTEX: new MockDigitalInput(nameof(CaldaiaIOSet.TERMOSTATO_ROTEX), new NullLogger<MockDigitalInput>()),
            rOTEX_STATO_POMPA: new MockDigitalInput(nameof(CaldaiaIOSet.ROTEX_STATO_POMPA), new NullLogger<DigitalInput>()),
            rOTEX_TEMP_ACCUMULO: new MockAnalogInput<Temperature>(nameof(CaldaiaIOSet.ROTEX_TEMP_ACCUMULO), new NullLogger<MockAnalogInput<Temperature>>()),
            rOTEX_TEMP_PANNELLI: new MockAnalogInput<Temperature>(nameof(CaldaiaIOSet.ROTEX_TEMP_PANNELLI), new NullLogger<MockAnalogInput<Temperature>>())
        );

        var caldaia = new CaldaiaMetano(
            rELAY_ACCENSIONE_CALDAIA: new MockDigitalOutput(nameof(CaldaiaIOSet.RELAY_CALDAIA), new NullLogger<MockDigitalOutput>())
        );

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
        
        this.application = new CaldaiaApplication(io, config, new NullLogger<CaldaiaApplication>());
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
        ((MockDigitalInput)io.TERMOSTATO_ROTEX).StartSquareInput(TimeSpan.FromMilliseconds(150));

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 400)
        {
            Thread.Sleep(1);
            var reading = io.ReadAll();
            Assert.IsTrue(reading.TERMOSTATO_ROTEX == reading.STATO_RELAY_CALDAIA);
            // Console.WriteLine(JsonConvert.SerializeObject(reading, Formatting.Indented));
        }
        Assert.Pass();
    }
}
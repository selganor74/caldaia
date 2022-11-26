using domain.measures;
using Microsoft.Extensions.Logging;

namespace application;

public class CaldaiaApplication : IDisposable
{
    private readonly CaldaiaIOSet io;
    private readonly CaldaiaConfig config;
    private readonly ILogger log;

    private Thread mainLoopThread;
    private bool isStarted;

    public CaldaiaApplication(
        CaldaiaIOSet io,
        CaldaiaConfig config,
        ILogger<CaldaiaApplication> log
        )
    {
        this.io = io;
        this.config = config;
        this.log = log;

        mainLoopThread = new Thread((obj) =>
        {
            try
            {
                MainLoop();
            }
            catch (Exception e)
            {
                log.LogCritical($"{nameof(mainLoopThread)} Errors during Main Loop.{Environment.NewLine}{e}");
                throw;
            }
        });
    }

    public void Dispose()
    {
        if (isStarted)
            Stop();

        io.Dispose();
    }

    public void Start()
    {
        this.isStarted = true;
        this.mainLoopThread.Start();
        log.LogInformation($"{nameof(CaldaiaApplication)} Started!");
    }

    public void Stop()
    {
        this.isStarted = false;
        this.mainLoopThread.Join();
        log.LogInformation($"{nameof(CaldaiaApplication)} Stopped!");
    }

    // This is the main control loop.
    private void MainLoop()
    {
        var ROTEX_DISPONIBILE = (CaldaiaAllValues stato) => stato.ROTEX_TEMP_ACCUMULO.Value != 0m;

        var CALDAIA_ACCESA = (CaldaiaAllValues stato) => stato.STATO_RELAY_CALDAIA.IsOn();
        var CALDAIA_SPENTA = (CaldaiaAllValues stato) => stato.STATO_RELAY_CALDAIA.IsOff();

        var TERMOSTATO_ROTEX_ATTIVO = (CaldaiaAllValues stato) => stato.TERMOSTATO_ROTEX.IsOn();
        var TERMOSTATO_ROTEX_NON_ATTIVO = (CaldaiaAllValues stato) => stato.TERMOSTATO_ROTEX.IsOff();

        var TEMPERATURA_ROTEX = (CaldaiaAllValues stato) => stato.ROTEX_TEMP_ACCUMULO.Value;
        var TEMPERATURA_CAMINO = (CaldaiaAllValues stato) => stato.TEMPERATURA_CAMINO.Value;

        var POMPA_CAMINO_ACCESA = (CaldaiaAllValues stato) => stato.STATO_RELAY_POMPA_CAMINO.IsOn() || io.RELAY_POMPA_CAMINO.IsDutyCycleStarted;
        var DUTY_CYCLE_POMPA_CAMINO = (CaldaiaAllValues stato) =>
        {
            if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_INNESCO_50_50)
                return 0m;
            if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_INNESCO_75_25)
                return 0.5m;
            if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_INNESCO_100)
                return 0.75m;
            return 1m;
        };

        var DELTA_T_CAMINO_T_ROTEX_SOPRA_SOGLIA = (CaldaiaAllValues stato) =>
        {
            return stato.TEMPERATURA_CAMINO.Value > stato.ROTEX_TEMP_ACCUMULO.Value + config.DELTA_T_CAMINO_T_ROTEX_INNESCO;
        };

        var nextLoopStart = DateTime.Now;
        while (isStarted)
        {
            nextLoopStart = WaitForNextLoopStart(config.MainLoopPeriod, nextLoopStart);
            var stato = io.ReadAll();

            if (ROTEX_DISPONIBILE(stato))
            {
                // Se il ROTEX è DISPONIBILE usiamo la temperatura della centralina per decidere se accendere la caldaia o no
                if (CALDAIA_ACCESA(stato) && TEMPERATURA_ROTEX(stato) > config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA)
                    io.RELAY_CALDAIA.SetToOff($"Temperatura accumulo ROTEX [{TEMPERATURA_ROTEX(stato)}] > {nameof(config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA)} [{config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA}]");

                if (CALDAIA_SPENTA(stato) && TEMPERATURA_ROTEX(stato) < config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA)
                    io.RELAY_CALDAIA.SetToOn($"Temperatura accumulo ROTEX [{TEMPERATURA_ROTEX(stato)}] < {nameof(config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA)} [{config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA}]");
            }

            if (!ROTEX_DISPONIBILE(stato))
            {
                if (CALDAIA_SPENTA(stato) && TERMOSTATO_ROTEX_ATTIVO(stato))
                    io.RELAY_CALDAIA.SetToOn($"Termostato ROTEX sotto set point");

                if (CALDAIA_ACCESA(stato) && TERMOSTATO_ROTEX_NON_ATTIVO(stato))
                    io.RELAY_CALDAIA.SetToOff($"Termostato ROTEX ha raggiunto il set point");
            }

            if (stato.TERMOSTATO_AMBIENTI.IsOff() && stato.STATO_RELAY_POMPA_RISCALDAMENTO.IsOn())
            {
                io.RELAY_POMPA_RISCALDAMENTO.SetToOff("Temperatura termostato ambienti raggiunta.");
            }

            if (stato.TERMOSTATO_AMBIENTI.IsOn() && stato.STATO_RELAY_POMPA_RISCALDAMENTO.IsOff())
            {
                io.RELAY_POMPA_RISCALDAMENTO.SetToOn("Temperatura ambienti inferiore a temperatura su termostato.");
            }

            if (ROTEX_DISPONIBILE(stato))
            {
                if (!POMPA_CAMINO_ACCESA(stato) && DELTA_T_CAMINO_T_ROTEX_SOPRA_SOGLIA(stato))
                {

                }
                if (POMPA_CAMINO_ACCESA(stato) && DUTY_CYCLE_POMPA_CAMINO(stato) == 0m)
                {
                    io.RELAY_POMPA_CAMINO.StopDutyCycle();
                    io.RELAY_POMPA_CAMINO.SetToOff($"Temperatura camino sotto soglia minima innesco {config.CAMINO_T_INNESCO_50_50}");
                }

                if (DUTY_CYCLE_POMPA_CAMINO(stato) != 0m)
                {
                    io.RELAY_POMPA_CAMINO.StartDutyCycle(DUTY_CYCLE_POMPA_CAMINO(stato), TimeSpan.FromMinutes(5));
                }
            }

            if (!ROTEX_DISPONIBILE(stato))
            {
                if (POMPA_CAMINO_ACCESA(stato) && DUTY_CYCLE_POMPA_CAMINO(stato) == 0m)
                {
                    io.RELAY_POMPA_CAMINO.StopDutyCycle();
                    io.RELAY_POMPA_CAMINO.SetToOff($"Temperatura camino sotto soglia minima innesco {config.CAMINO_T_INNESCO_50_50}");
                }

                if (DUTY_CYCLE_POMPA_CAMINO(stato) != 0m)
                {
                    io.RELAY_POMPA_CAMINO.StartDutyCycle(DUTY_CYCLE_POMPA_CAMINO(stato), TimeSpan.FromMinutes(5));

                    if (TEMPERATURA_CAMINO(stato) > config.CAMINO_T_BYPASS_AMBIENTI && stato.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE.IsOff())
                        io.RELAY_BYPASS_TERMOSTATO_AMBIENTE.SetToOn($"Attivazione BYPASS Termostati. Temperatura Rotex non disponibile e temperatura CAMINO {TEMPERATURA_CAMINO(stato)} > {nameof(config.CAMINO_T_BYPASS_AMBIENTI)} {config.CAMINO_T_BYPASS_AMBIENTI}");

                    if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_BYPASS_AMBIENTI && stato.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE.IsOn())
                        io.RELAY_BYPASS_TERMOSTATO_AMBIENTE.SetToOff($"Disattivazione BYPASS Termostati. Temperatura Rotex non disponibile e temperatura CAMINO {TEMPERATURA_CAMINO(stato)} < {nameof(config.CAMINO_T_BYPASS_AMBIENTI)} {config.CAMINO_T_BYPASS_AMBIENTI} ");
                }
            }
        }
    }

    private DateTime WaitForNextLoopStart(TimeSpan LOOP_PERIOD, DateTime nextLoopStart)
    {
        TimeSpan delay = nextLoopStart - DateTime.Now;
        var skipped = 0;
        while (delay.Ticks < 0)
        {
            nextLoopStart += LOOP_PERIOD;
            delay = nextLoopStart - DateTime.Now;
            skipped++;
        }
        if (skipped > 1)
            log.LogWarning($"Last {nameof(MainLoop)} execution exceeded {skipped - 1} times configure loop length ({LOOP_PERIOD.TotalMilliseconds} ms).{Environment.NewLine}Consider either optimizing {nameof(MainLoop)} or increasing {nameof(config.MainLoopPeriod)} parameter");

        Thread.Sleep(delay);
        return nextLoopStart;
    }
}

using application.infrastructure;
using domain.measures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        Stop();
        this.DisposeDisposables(log);
    }

    public void Start()
    {
        this.isStarted = true;
        this.mainLoopThread.Start();
        log.LogInformation($"{nameof(CaldaiaApplication)} Started with config:{Environment.NewLine}{JsonConvert.SerializeObject(config, Formatting.Indented)}");
    }

    public void Stop()
    {
        if (!this.isStarted)
            return;

        this.isStarted = false;
        this.mainLoopThread.Join();

        log.LogInformation($"{nameof(CaldaiaApplication)} Stopped!");
    }

    // This is the main control loop.
    private void MainLoop()
    {
        var nextLoopStart = DateTime.Now;

        log.LogInformation($"{nameof(MainLoop)}: Waiting for {nameof(io)} to become Ready ...");

        while (!io.IsReady(log))
            Thread.Sleep(100);

        log.LogInformation($"{nameof(MainLoop)}: ... ok, {nameof(io)} Ready ...");

        var lastOutput = DateTime.Now;
        while (isStarted)
        {
            nextLoopStart = WaitForNextLoopStart(config.MainLoopPeriod, nextLoopStart);

            stato = io.ReadAll(log);
            if (DateTime.Now > (lastOutput + TimeSpan.FromSeconds(10)))
            {
                lastOutput = DateTime.Now;
                log.LogDebug(stato.ToString());
            }

            CrunchInputs();
        }


    }

    private Func<CaldaiaAllValues, bool>
        ROTEX_DISPONIBILE = (CaldaiaAllValues stato) => stato.ROTEX_TEMP_ACCUMULO.UtcTimeStamp > DateTime.UtcNow - TimeSpan.FromMinutes(5);

    private Func<CaldaiaAllValues, bool>
        CALDAIA_ACCESA = (CaldaiaAllValues stato) => stato.STATO_RELAY_CALDAIA.IsOn();

    private Func<CaldaiaAllValues, bool>
        CALDAIA_SPENTA = (CaldaiaAllValues stato) => stato.STATO_RELAY_CALDAIA.IsOff();

    private Func<CaldaiaAllValues, bool>
        TERMOSTATO_ROTEX_ATTIVO = (CaldaiaAllValues stato) => stato.TERMOSTATO_ROTEX.IsOn();

    private Func<CaldaiaAllValues, bool>
        TERMOSTATO_ROTEX_NON_ATTIVO = (CaldaiaAllValues stato) => stato.TERMOSTATO_ROTEX.IsOff();

    private Func<CaldaiaAllValues, decimal>
        TEMPERATURA_ROTEX = (CaldaiaAllValues stato) => stato.ROTEX_TEMP_ACCUMULO.Value;

    private Func<CaldaiaAllValues, decimal>
        TEMPERATURA_CAMINO = (CaldaiaAllValues stato) => stato.TEMPERATURA_CAMINO.Value;

    private Func<CaldaiaAllValues, CaldaiaIOSet, bool> POMPA_CAMINO_ACCESA = (CaldaiaAllValues stato, CaldaiaIOSet io) => stato.STATO_RELAY_POMPA_CAMINO.IsOn() || io.RELAY_POMPA_CAMINO.IsDutyCycleStarted;
    private CaldaiaAllValues stato;

    private decimal CALCOLA_DUTY_CYCLE_POMPA_CAMINO(CaldaiaAllValues stato)
    {
        if (!ROTEX_DISPONIBILE(stato))
        {
            return CalcolaDutyCycleSoloSuTemperaturaCamino(stato);
        }

        if (ROTEX_DISPONIBILE(stato))
        {
            if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_INNESCO_50_50)
                return 0m;

            // La temperatura del rotex non deve scendere sotto i 60
            // Quindi se il camino è sotto a 60, fermiamo la pompa 
            if (TEMPERATURA_ROTEX(stato) > TEMPERATURA_CAMINO(stato)
                && TEMPERATURA_ROTEX(stato) < 60
                )
                return 0m;

            if (TEMPERATURA_ROTEX(stato) < TEMPERATURA_CAMINO(stato))
                return CalcolaDutyCycleSoloSuTemperaturaCamino(stato);

            // La temperatura del rotex non deve scendere sotto i 60
            // Quindi se il camino è sotto a 60, fermiamo la pompa 
            if (TEMPERATURA_ROTEX(stato) > TEMPERATURA_CAMINO(stato)
                && TEMPERATURA_ROTEX(stato) >= 60
                )
                return CalcolaDutyCycleSoloSuTemperaturaCamino(stato);

        }

        // Come fall back ... ma non ci dovremmo mai cadere!
        return CalcolaDutyCycleSoloSuTemperaturaCamino(stato);
    }
    private decimal CalcolaDutyCycleSoloSuTemperaturaCamino(CaldaiaAllValues stato)
    {
        if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_INNESCO_50_50)
            return 0m;
        if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_INNESCO_75_25)
            return 0.5m;
        if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_INNESCO_100)
            return 0.75m;
        return 1m;
    }

    private bool DELTA_T_CAMINO_T_ROTEX_SOPRA_SOGLIA(CaldaiaAllValues stato)
    {
        return stato.TEMPERATURA_CAMINO.Value > stato.ROTEX_TEMP_ACCUMULO.Value + config.DELTA_T_CAMINO_T_ROTEX_INNESCO;
    }


    private void CrunchInputs()
    {
        Manage_ACCENSIONE_CALDAIA();

        Manage_POMPA_RISCALDAMENTO();

        Manage_BYPASS_TERMO_AMBIENTI();

        Manage_POMPA_CAMINO();
    }

    private void Manage_POMPA_CAMINO()
    {
        var dutyCyclePompaCamino = CALCOLA_DUTY_CYCLE_POMPA_CAMINO(stato);
        if (ROTEX_DISPONIBILE(stato))
        {
            if (POMPA_CAMINO_ACCESA(stato, io) && dutyCyclePompaCamino == 0m)
            {
                io.RELAY_POMPA_CAMINO.StopDutyCycle();
                io.RELAY_POMPA_CAMINO.SetToOff($"Temperatura camino sotto soglia minima innesco {config.CAMINO_T_INNESCO_50_50}");
            }

            if (dutyCyclePompaCamino != 0m)
            {
                if (TEMPERATURA_ROTEX(stato) < TEMPERATURA_CAMINO(stato))
                {
                    if (TEMPERATURA_ROTEX(stato) > 60)
                    {
                        io.RELAY_POMPA_CAMINO.SetDutyCycle(0.25m, TimeSpan.FromMinutes(5));
                    }
                    if (TEMPERATURA_ROTEX(stato) <= 60)
                    {
                        io.RELAY_POMPA_CAMINO.StopDutyCycle();
                        io.RELAY_POMPA_CAMINO.SetToOff($"Temperatura camino ({TEMPERATURA_CAMINO(stato):F1}) inferiore a temperatura ROTEX ({TEMPERATURA_ROTEX(stato):F1}) e temperatura ROTEX <= 60.");
                    }
                }
                else
                {
                    io.RELAY_POMPA_CAMINO.SetDutyCycle(dutyCyclePompaCamino, TimeSpan.FromMinutes(5));
                }
            }
        }

        if (!ROTEX_DISPONIBILE(stato))
        {
            if (POMPA_CAMINO_ACCESA(stato, io) && dutyCyclePompaCamino == 0m)
            {
                io.RELAY_POMPA_CAMINO.StopDutyCycle();
                io.RELAY_POMPA_CAMINO.SetToOff($"Temperatura camino sotto soglia minima innesco {config.CAMINO_T_INNESCO_50_50}");
            }

            if (dutyCyclePompaCamino != 0m)
            {
                io.RELAY_POMPA_CAMINO.SetDutyCycle(dutyCyclePompaCamino, TimeSpan.FromMinutes(5));
            }
        }
    }

    private void Manage_BYPASS_TERMO_AMBIENTI()
    {
        if (!ROTEX_DISPONIBILE(stato))
        {
            if (TEMPERATURA_CAMINO(stato) > config.CAMINO_T_INNESCO_BYPASS_AMBIENTI && stato.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE.IsOff())
                io.RELAY_BYPASS_TERMOSTATO_AMBIENTE.SetToOn($"Attivazione BYPASS Termostati Ambiente. Temperatura Rotex non disponibile e temperatura CAMINO {TEMPERATURA_CAMINO(stato)} > {nameof(config.CAMINO_T_INNESCO_BYPASS_AMBIENTI)} {config.CAMINO_T_INNESCO_BYPASS_AMBIENTI}");

            if (TEMPERATURA_CAMINO(stato) < config.CAMINO_T_DISINNESCO_BYPASS_AMBIENTI && stato.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE.IsOn())
                io.RELAY_BYPASS_TERMOSTATO_AMBIENTE.SetToOff($"Disattivazione BYPASS Termostati Ambiente. Temperatura Rotex non disponibile e temperatura CAMINO {TEMPERATURA_CAMINO(stato)} < {nameof(config.CAMINO_T_INNESCO_BYPASS_AMBIENTI)} {config.CAMINO_T_INNESCO_BYPASS_AMBIENTI} ");
        }

        if (ROTEX_DISPONIBILE(stato) && stato.CAMINO_ON_OFF.IsOn())
        {
            if (stato.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE.IsOff()
                && TEMPERATURA_ROTEX(stato) > 60
                && TEMPERATURA_CAMINO(stato) > config.CAMINO_T_INNESCO_BYPASS_AMBIENTI
                )
                io.RELAY_BYPASS_TERMOSTATO_AMBIENTE.SetToOn($"Attivazione BYPASS Termostati Ambiente. Temperatura Rotex {TEMPERATURA_ROTEX(stato)} > 60 e temperatura CAMINO {TEMPERATURA_CAMINO(stato)} > {nameof(config.CAMINO_T_INNESCO_BYPASS_AMBIENTI)} ({config.CAMINO_T_INNESCO_BYPASS_AMBIENTI})");

            if (stato.STATO_RELAY_BYPASS_TERMOSTATO_AMBIENTE.IsOn()
                && (
                        TEMPERATURA_ROTEX(stato) <= 60
                    || TEMPERATURA_CAMINO(stato) <= config.CAMINO_T_DISINNESCO_BYPASS_AMBIENTI
                    )
                )
                io.RELAY_BYPASS_TERMOSTATO_AMBIENTE.SetToOff($"Disattivazione BYPASS Termostati Ambiente. Temperatura Rotex {TEMPERATURA_ROTEX(stato)} < 60 o temperatura CAMINO {TEMPERATURA_CAMINO(stato)} < {nameof(config.CAMINO_T_DISINNESCO_BYPASS_AMBIENTI)} ({config.CAMINO_T_DISINNESCO_BYPASS_AMBIENTI})");
        }
    }

    private void Manage_POMPA_RISCALDAMENTO()
    {
        if (stato.TERMOSTATO_AMBIENTI.IsOff() && stato.STATO_RELAY_POMPA_RISCALDAMENTO.IsOn())
            io.RELAY_POMPA_RISCALDAMENTO.SetToOff("Temperatura termostato ambienti raggiunta.");

        if (stato.TERMOSTATO_AMBIENTI.IsOn() && stato.STATO_RELAY_POMPA_RISCALDAMENTO.IsOff())
            io.RELAY_POMPA_RISCALDAMENTO.SetToOn("Temperatura ambienti inferiore a temperatura su termostato.");
    }

    private void Manage_ACCENSIONE_CALDAIA()
    {
        if (ROTEX_DISPONIBILE(stato))
        {
            // Se il ROTEX è DISPONIBILE usiamo la temperatura dell'accumulo per decidere se accendere la caldaia o no
            if (CALDAIA_ACCESA(stato) && TEMPERATURA_ROTEX(stato) > config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA)
                io.RELAY_CALDAIA.SetToOff($"Temperatura accumulo ROTEX [{TEMPERATURA_ROTEX(stato)}] > {nameof(config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA)} [{config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA}]");

            if (CALDAIA_SPENTA(stato) && TEMPERATURA_ROTEX(stato) < config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA)
                io.RELAY_CALDAIA.SetToOn($"Temperatura accumulo ROTEX [{TEMPERATURA_ROTEX(stato)}] < {nameof(config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA)} [{config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA}]");
        }

        if (!ROTEX_DISPONIBILE(stato))
        {
            // se la centralina rotex non è disponibile, usiamo il termostato "volante" 
            if (CALDAIA_SPENTA(stato) && TERMOSTATO_ROTEX_ATTIVO(stato))
                io.RELAY_CALDAIA.SetToOn($"Termostato ROTEX sotto set point");

            if (CALDAIA_ACCESA(stato) && TERMOSTATO_ROTEX_NON_ATTIVO(stato))
                io.RELAY_CALDAIA.SetToOff($"Termostato ROTEX ha raggiunto il set point");
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
            log.LogWarning($"Last {nameof(MainLoop)} execution exceeded {skipped - 1} times configured loop length ({LOOP_PERIOD.TotalMilliseconds} ms).{Environment.NewLine}Consider either optimizing {nameof(MainLoop)} or increasing {nameof(config.MainLoopPeriod)} parameter");

        Thread.Sleep(delay);
        return nextLoopStart;
    }
}

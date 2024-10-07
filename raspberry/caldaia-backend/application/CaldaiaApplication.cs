using application.infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace application;

public class CaldaiaApplication : IDisposable
{
    private readonly CaldaiaIOSet io;
    private readonly CaldaiaConfig config;
    private readonly INotificationPublisher notificationHub;
    private readonly ILogger log;

    private CaldaiaAllValues stato;
    private decimal prevDutyCycle;

    private Thread mainLoopThread;
    private Thread logStatoThread;
    private bool isStarted;

    public CaldaiaApplication(
        CaldaiaIOSet io,
        CaldaiaConfig config,
        INotificationPublisher notificationHub,
        ILogger<CaldaiaApplication> log
        )
    {
        this.io = io;
        this.config = config;
        this.notificationHub = notificationHub;
        this.log = log;

        logStatoThread = new Thread((obj) =>
        {
            var LOG_OUTPUT_INTERVAL = TimeSpan.FromSeconds(10);
            var lastTimeLogged = DateTime.Now;

            while (isStarted)
            {
                Thread.Sleep(LOG_OUTPUT_INTERVAL);
                if (stato is not null)
                    log.LogDebug(stato!.ToString());
            }
        });

        mainLoopThread = new Thread((obj) =>
        {
            log.LogDebug($"{mainLoopThread} started!");
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
        log.LogInformation($"{nameof(CaldaiaApplication)} Starting application.");
        if (isStarted)
            return;

        isStarted = true;

        log.LogInformation($"{nameof(CaldaiaApplication)} * Starting main loop thread.");
        mainLoopThread.Start();

        log.LogInformation($"{nameof(CaldaiaApplication)} * Starting status logging thread.");
        logStatoThread.Start();

        log.LogInformation($"{nameof(CaldaiaApplication)} Started with config:{Environment.NewLine}{JsonConvert.SerializeObject(config, Formatting.Indented)}");
    }

    public void Stop()
    {
        if (!this.isStarted)
            return;

        this.isStarted = false;
        this.mainLoopThread.Join();
        this.logStatoThread.Join();
        this.io.Dispose();

        log.LogInformation($"{nameof(CaldaiaApplication)} Stopped!");
    }

    // This is the main control loop.
    private void MainLoop()
    {
        var nextLoopStart = DateTime.Now;

        log.LogInformation($"{nameof(MainLoop)}: 😁 Waiting for {nameof(io)} to become Ready ...");

        var timer = Stopwatch.StartNew();
        var READY_TIMEOUT = TimeSpan.FromSeconds(15);
        while (!io.IsReady(log) && timer.Elapsed < READY_TIMEOUT)
        {
            log.LogDebug($"Ready Timeout will occour in {Math.Round((READY_TIMEOUT - timer.Elapsed).TotalSeconds,1)} seconds");
            Thread.Sleep(100);
        }

        if (io.IsReady(log))
        {
            log.LogInformation($"{nameof(MainLoop)}: ... ok, {nameof(io)} Ready ...");
        }
        else
        {
            log.LogInformation("Timeout while waiting for readyness ... lets go on");
        }

        while (isStarted)
        {
            log.LogDebug($"{nameof(MainLoop)}: Waiting to start ... (loop period: {config.MainLoopPeriod}) ");

            nextLoopStart = WaitForNextLoopStart(config.MainLoopPeriod, nextLoopStart);

            log.LogDebug($"{nameof(MainLoop)}: Reading IO state ...");

            stato = io.ReadAll(log);

            log.LogDebug($"{nameof(MainLoop)}: {stato}");

            log.LogDebug($"{nameof(MainLoop)}: Crunching inputs ...");

            CrunchInputs();

            log.LogDebug($"{nameof(MainLoop)}: Notifying state to UI ...");

            NotifyState();
        }
    }

    private void NotifyState()
    {
        log.LogDebug($"Publishing to status-reading channel");
        notificationHub.Publish("status-reading", stato);
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
        log.LogDebug($"{nameof(CrunchInputs)}: {nameof(Manage_ACCENSIONE_CALDAIA)}");
        Manage_ACCENSIONE_CALDAIA();

        log.LogDebug($"{nameof(CrunchInputs)}: {nameof(Manage_POMPA_RISCALDAMENTO)}");
        Manage_POMPA_RISCALDAMENTO();

        log.LogDebug($"{nameof(CrunchInputs)}: {nameof(Manage_BYPASS_TERMO_AMBIENTI)}");
        Manage_BYPASS_TERMO_AMBIENTI();

        log.LogDebug($"{nameof(CrunchInputs)}: {nameof(Manage_POMPA_CAMINO)}");
        Manage_POMPA_CAMINO();
    }

    private void Manage_POMPA_CAMINO()
    {
        var dutyCyclePompaCamino = CALCOLA_DUTY_CYCLE_POMPA_CAMINO(stato);
        log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: Computed Duty Cycle pompa camino: {dutyCyclePompaCamino}");
        if (prevDutyCycle != dutyCyclePompaCamino)
        {
            log.LogDebug($"{nameof(Manage_POMPA_CAMINO)}: Duty Cycle has changed from {prevDutyCycle} to {dutyCyclePompaCamino}.");
            prevDutyCycle = dutyCyclePompaCamino;
        }

        if (ROTEX_DISPONIBILE(stato))
        {
            log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: ROTEX is available!");
            if (POMPA_CAMINO_ACCESA(stato, io) && dutyCyclePompaCamino == 0m)
            {
                log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: Setting {nameof(io.RELAY_POMPA_CAMINO)} ad {dutyCyclePompaCamino} Duty Cycle!");
                io.RELAY_POMPA_CAMINO.StopDutyCycle();
                io.RELAY_POMPA_CAMINO.SetToOff($"Temperatura camino sotto soglia minima innesco {config.CAMINO_T_INNESCO_50_50}");
            }

            if (dutyCyclePompaCamino != 0m)
            {
                if (TEMPERATURA_ROTEX(stato) > TEMPERATURA_CAMINO(stato))
                {
                    log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: {nameof(io.ROTEX_TEMP_ACCUMULO)} ({TEMPERATURA_ROTEX(stato)}) > {nameof(io.CAMINO_TEMPERATURA)} ({TEMPERATURA_CAMINO(stato)}).");
                    if (TEMPERATURA_ROTEX(stato) > 60)
                    {
                        log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: Setting {nameof(io.RELAY_POMPA_CAMINO)} to 0.25 Duty Cycle (FIXED)!");
                        io.RELAY_POMPA_CAMINO.SetDutyCycle(0.25m, TimeSpan.FromMinutes(5));
                    }
                    if (TEMPERATURA_ROTEX(stato) <= 60)
                    {
                        log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: Stopping {nameof(io.RELAY_POMPA_CAMINO)}");
                        io.RELAY_POMPA_CAMINO.StopDutyCycle();
                        io.RELAY_POMPA_CAMINO.SetToOff($"Temperatura camino ({TEMPERATURA_CAMINO(stato):F1}) inferiore a temperatura ROTEX ({TEMPERATURA_ROTEX(stato):F1}) e temperatura ROTEX <= 60.");
                    }
                }
                else
                {
                    log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: {nameof(io.ROTEX_TEMP_ACCUMULO)} ({TEMPERATURA_ROTEX(stato)}) <= {nameof(io.CAMINO_TEMPERATURA)} ({TEMPERATURA_CAMINO(stato)}).");
                    log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: Setting {nameof(io.RELAY_POMPA_CAMINO)} to {dutyCyclePompaCamino} Duty Cycle (computed)!");
                    io.RELAY_POMPA_CAMINO.SetDutyCycle(dutyCyclePompaCamino, TimeSpan.FromMinutes(5));
                }
            }
        }

        if (!ROTEX_DISPONIBILE(stato))
        {
            log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: ROTEX is NOT available!");
            if (POMPA_CAMINO_ACCESA(stato, io) && dutyCyclePompaCamino == 0m)
            {
                log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: Stopping {nameof(io.RELAY_POMPA_CAMINO)}!");
                io.RELAY_POMPA_CAMINO.StopDutyCycle();
                io.RELAY_POMPA_CAMINO.SetToOff($"Temperatura camino sotto soglia minima innesco {config.CAMINO_T_INNESCO_50_50}");
            }

            if (dutyCyclePompaCamino != 0m)
            {
                log.LogDebug($"  {nameof(Manage_POMPA_CAMINO)}: Setting {nameof(io.RELAY_POMPA_CAMINO)} ad {dutyCyclePompaCamino} Duty Cycle!");
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
                && TEMPERATURA_ROTEX(stato) > 62
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
            log.LogDebug($"  {nameof(Manage_ACCENSIONE_CALDAIA)}: ROTEX is available!");
            // Se il ROTEX è DISPONIBILE usiamo la temperatura dell'accumulo per decidere se accendere la caldaia o no
            if (CALDAIA_ACCESA(stato) && TEMPERATURA_ROTEX(stato) > config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA)
            {
                log.LogDebug($"  {nameof(Manage_ACCENSIONE_CALDAIA)}: Temperatura accumulo ROTEX [{TEMPERATURA_ROTEX(stato)}] > {nameof(config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA)} [{config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA}]");
                io.RELAY_CALDAIA.SetToOff($"Temperatura accumulo ROTEX [{TEMPERATURA_ROTEX(stato)}] > {nameof(config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA)} [{config.ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA}]");
            }

            if (CALDAIA_SPENTA(stato) && TEMPERATURA_ROTEX(stato) < config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA)
            {
                log.LogDebug($"  {nameof(Manage_ACCENSIONE_CALDAIA)}: Temperatura accumulo ROTEX [{TEMPERATURA_ROTEX(stato)}] < {nameof(config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA)} [{config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA}]");
                io.RELAY_CALDAIA.SetToOn($"Temperatura accumulo ROTEX [{TEMPERATURA_ROTEX(stato)}] < {nameof(config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA)} [{config.ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA}]");
            }
        }

        if (!ROTEX_DISPONIBILE(stato))
        {
            log.LogDebug($"  {nameof(Manage_ACCENSIONE_CALDAIA)}: ROTEX is NOT available!");
            log.LogDebug($"  {nameof(Manage_ACCENSIONE_CALDAIA)}: CALDAIA_SPENTA:{CALDAIA_SPENTA(stato)} TERMOSTATO_ROTEX_ATTIVO:{TERMOSTATO_ROTEX_ATTIVO(stato)}");
            // se la centralina rotex non è disponibile, usiamo il termostato "volante" 
            if (CALDAIA_SPENTA(stato) && TERMOSTATO_ROTEX_ATTIVO(stato))
            {
                log.LogDebug($"  {nameof(Manage_ACCENSIONE_CALDAIA)}: Setting {nameof(io.RELAY_CALDAIA)} to ON: Termostato ROTEX sotto set point");
                io.RELAY_CALDAIA.SetToOn($"Termostato ROTEX sotto set point");
            }

            if (CALDAIA_ACCESA(stato) && TERMOSTATO_ROTEX_NON_ATTIVO(stato))
            {
                log.LogDebug($"  {nameof(Manage_ACCENSIONE_CALDAIA)}: Setting {nameof(io.RELAY_CALDAIA)} to OFF: Termostato ROTEX ha raggiunto il set point");
                io.RELAY_CALDAIA.SetToOff($"Termostato ROTEX ha raggiunto il set point");
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
            log.LogWarning($"Last {nameof(MainLoop)} execution exceeded {skipped - 1} times configured loop length ({LOOP_PERIOD.TotalMilliseconds} ms).{Environment.NewLine}Consider either optimizing {nameof(MainLoop)} or increasing {nameof(config.MainLoopPeriod)} parameter");

        Thread.Sleep(delay);
        return nextLoopStart;
    }
}

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
        var nextLoopStart = DateTime.Now;

        while (isStarted)
        {
            nextLoopStart = WaitForNextLoopStart(config.MainLoopPeriod, nextLoopStart);

            var stato = io.ReadAll();

            if (stato.TERMOSTATO_AMBIENTI.IsOff() && stato.STATO_RELAY_POMPA_RISCALDAMENTO.IsOn())
            {
                io.RELAY_POMPA_RISCALDAMENTO.SetToOff("Temperatura termostato ambienti raggiunta.");
            }

            if (stato.TERMOSTATO_AMBIENTI.IsOn() && stato.STATO_RELAY_POMPA_RISCALDAMENTO.IsOff())
            {
                io.RELAY_POMPA_RISCALDAMENTO.SetToOn("Temperatura ambienti inferiore a temperatura su termostato.");
            }

            if (stato.CAMINO_ON_OFF.IsOff() /* aggiungere controllo su rotex disponibile */)
            {
                // Camino "spento"

                if (stato.TERMOSTATO_ROTEX.IsOff() && stato.STATO_RELAY_CALDAIA.IsOn())
                {
                    io.RELAY_CALDAIA.SetToOff("Il termostato ROTEX rileva temperatura sufficiente.");
                }

                if (stato.TERMOSTATO_ROTEX.IsOff() && stato.STATO_RELAY_CALDAIA.IsOff())
                {
                    io.RELAY_CALDAIA.SetToOn("Il termostato ROTEX indica temperatura inferiore al setpoint.");
                }
            }

            if (stato.CAMINO_ON_OFF.IsOn())
            {
                // Il camino è acceso, ignoriamo lo stato del termostato ROTEX.
                // Gestione Pompa Camino

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

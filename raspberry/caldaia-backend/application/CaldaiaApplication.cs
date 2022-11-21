using domain.measures;
using Microsoft.Extensions.Logging;

namespace application;

public class CaldaiaApplication : IDisposable
{
    private readonly CaldaiaIOSet io;
    private readonly ILogger log;

    private Thread mainLoopThread;
    private bool isStarted;

    public CaldaiaApplication(
        CaldaiaIOSet io,
        ILogger<CaldaiaApplication> log
        )
    {
        this.io = io;
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
        const int LOOP_FREQUENCY_MS = 100;
        var nextLoopStart = DateTime.Now + TimeSpan.FromMilliseconds(LOOP_FREQUENCY_MS);
        while (isStarted)
        {
            nextLoopStart = WaitForNextLoopStart(LOOP_FREQUENCY_MS, nextLoopStart);

            var stato = io.ReadAll();

            if (stato.TERMOSTATO_AMBIENTI == OnOffState.OFF && stato.RELAY_POMPA_RISCALDAMENTO == OnOffState.ON)
            {
                io.RELAY_POMPA_RISCALDAMENTO.SetToOff("Temperatura termostato ambienti raggiunta.");
            }

            if (stato.TERMOSTATO_AMBIENTI == OnOffState.ON && stato.RELAY_POMPA_RISCALDAMENTO == OnOffState.OFF)
            {
                io.RELAY_POMPA_RISCALDAMENTO.SetToOn("Temperatura ambienti inferiore a temperatura su termostato.");
            }

            if (stato.TERMOSTATO_ROTEX == OnOffState.OFF && stato.RELAY_CALDAIA == OnOffState.ON)
            {
                io.RELAY_CALDAIA.SetToOff("Il termostato ROTEX rileva temperatura sufficiente.");
            }

            if (stato.TERMOSTATO_ROTEX == OnOffState.ON && stato.RELAY_CALDAIA == OnOffState.OFF)
            {
                io.RELAY_CALDAIA.SetToOn("Il termostato ROTEX indica temperatura inferiore al setpoint.");
            }
        }
    }

    private static DateTime WaitForNextLoopStart(int LOOP_FREQUENCY_MS, DateTime nextLoopStart)
    {
        TimeSpan delay = nextLoopStart - DateTime.Now;
        while (delay.Ticks < 0)
        {
            nextLoopStart += TimeSpan.FromMilliseconds(LOOP_FREQUENCY_MS);
            delay = nextLoopStart - DateTime.Now;
        }
        Thread.Sleep(delay);
        return nextLoopStart;
    }
}

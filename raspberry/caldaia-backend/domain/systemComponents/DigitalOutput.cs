using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class DigitalOutput : DigitalInput, IDisposable
{
    private TimeSpan MIN_TIME_BETWEEN_TOGGLES;
    private DateTime lastMinTimeBetweenTogglesLog;

    private Thread dutyCycleThread;
    public bool IsDutyCycleStarted { get; private set; }
    private decimal percentOn_0_1;
    private TimeSpan dutyCyclePeriod;
    private TimeSpan tOn;
    private TimeSpan tOff;

    protected DigitalOutput(string name, ILogger<DigitalOutput> log) : base(name, log)
    {
        MIN_TIME_BETWEEN_TOGGLES = TimeSpan.FromSeconds(10);
        lastMinTimeBetweenTogglesLog = DateTime.Now - MIN_TIME_BETWEEN_TOGGLES;
        dutyCycleThread = new Thread(() => DutyCycle());
    }

    private bool IsGoingToToggle(OnOff newMeasure)
    {
        if (LastMeasure == null)
            return true;

        if (newMeasure.Value != LastMeasure.Value)
            return true;

        return false;
    }

    private bool CanToggle()
    {
        if (LastMeasure == null)
            return true;

        var timeSinceLastToggle = (DateTime.UtcNow - LastMeasure.UtcTimeStamp);
        if ((DateTime.UtcNow - LastMeasure.UtcTimeStamp) < MIN_TIME_BETWEEN_TOGGLES)
        {
            // Avoids spamming logs
            if (DateTime.Now - lastMinTimeBetweenTogglesLog >= MIN_TIME_BETWEEN_TOGGLES)
            {
                log.LogDebug($"{nameof(SetToOn)}: Cannot toggle {Name} because last toggle happened {timeSinceLastToggle.TotalSeconds} seconds ago, and minimum time between toggles is {MIN_TIME_BETWEEN_TOGGLES.TotalSeconds} seconds");
                lastMinTimeBetweenTogglesLog = DateTime.Now;
            }
            return false;
        }

        this.lastMinTimeBetweenTogglesLog = DateTime.Now - MIN_TIME_BETWEEN_TOGGLES;
        return true;
    }

    public void SetMinTimeBetweenToggles(TimeSpan newMinTime)
    {
        this.MIN_TIME_BETWEEN_TOGGLES = newMinTime;
        this.lastMinTimeBetweenTogglesLog = DateTime.Now - newMinTime;
        log.LogInformation($"{Name}: Setting min time between toggles to {newMinTime.TotalSeconds}s");    
    }


    // Avvia un ciclo di durata "dutyCyclePe" e duty cycle "percentOn_0_1"
    public void SetDutyCycle(decimal percentOn_0_1, TimeSpan dutyCyclePeriod)
    {
        if(this.percentOn_0_1 == percentOn_0_1)
            return;

        log.LogInformation($"{Name}: Changing dutyCycle from {this.percentOn_0_1 * 100}% to {percentOn_0_1 * 100}% on a period of {dutyCyclePeriod.TotalSeconds}s");    
        this.percentOn_0_1 = percentOn_0_1;
        this.dutyCyclePeriod = dutyCyclePeriod;
        var minPeriod = 0.99 * dutyCyclePeriod * (double)Math.Min(percentOn_0_1, 1 - percentOn_0_1);

        tOn = TimeSpan.FromMilliseconds((int)(dutyCyclePeriod.TotalMilliseconds * (double)percentOn_0_1));
        tOff = dutyCyclePeriod - tOn;

        SetMinTimeBetweenToggles(minPeriod);

        if (!IsDutyCycleStarted)
        {
            IsDutyCycleStarted = true;
            dutyCycleThread.Start();
        }
    }

    public void StopDutyCycle()
    {
        if (!IsDutyCycleStarted)
            return;

        this.IsDutyCycleStarted = false;
        this.dutyCycleThread.Join();
        log.LogInformation($"{Name}: Stopping {this.percentOn_0_1 * 100}% duty cycle.");    
    }

    private void DutyCycle()
    {
        try
        {
            while (IsDutyCycleStarted)
            {
                if (tOn.TotalMilliseconds > 0)
                {
                    SetToOn($"{Name} duty cycle {percentOn_0_1 * 100:F0}%, tOn: {tOn.TotalSeconds} s");
                    Thread.Sleep(tOn);
                }

                if (tOff.TotalMilliseconds > 0)
                {
                    SetToOff($"{Name} duty cycle {percentOn_0_1 * 100:F0}%, tOff: {tOff.TotalSeconds} s");
                    Thread.Sleep(tOff);
                }
            }
        }
        catch (Exception e)
        {
            log.LogError($"{Name} Errors in {nameof(DutyCycle)}.{Environment.NewLine}{e}");
        }
    }

    public void SetToOn(string reason)
    {
        var newMeasure = new OnOff(OnOffState.ON);
        if (!IsGoingToToggle(newMeasure))
            return;

        if (!CanToggle())
            return;

        try
        {
            SetToOnImplementation();
            this.LastMeasure = newMeasure;
            log.LogInformation($"{Name} set to On. Reason: {reason}");
        }
        catch (Exception e)
        {
            log.LogError($"{Name}: Errors while setting to On.{Environment.NewLine}{e}");
        }
    }

    public void SetToOff(string reason)
    {
        var newMeasure = new OnOff(OnOffState.OFF);
        if (!IsGoingToToggle(newMeasure))
            return;

        if (!CanToggle())
            return;

        try
        {
            SetToOffImplementation();
            this.LastMeasure = newMeasure;
            log.LogInformation($"{Name} set to Off. Reason: {reason}");
        }
        catch (Exception e)
        {
            log.LogError($"{Name}: Errors while setting to Off.{Environment.NewLine}{e}");
        }
    }

    protected abstract void SetToOnImplementation();
    protected abstract void SetToOffImplementation();

    public virtual void Dispose()
    {
        SetToOffImplementation();
    }
}

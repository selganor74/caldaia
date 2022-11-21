using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents.mocks;

public class MockAnalogInput<TMeasure> : AnalogInput<TMeasure>, IDisposable where TMeasure : IMeasure
{
    private class SineInputParameters
    {
        public TMeasure min { get; }
        public TMeasure max { get; }
        public TimeSpan period { get; }
        public int measuresPerPeriod { get; }
        public TimeSpan timeStep => period / measuresPerPeriod;
        public double amplitude => (double)(max.Value - min.Value) / 2;
        public decimal offset => (max.Value + min.Value) / 2;

        public SineInputParameters(
            TMeasure min,
            TMeasure max,
            TimeSpan period,
            int measuresPerPeriod)
        {
            this.min = min;
            this.max = max;
            this.period = period;
            this.measuresPerPeriod = measuresPerPeriod;
        }
    }

    private Thread sineInputThread;
    private bool isSineInputStarted;

    #pragma warning disable CS8618
    private MockAnalogInput<TMeasure>.SineInputParameters sineInputParameters;

    public MockAnalogInput(string name, ILogger<MockAnalogInput<TMeasure>> log) : base(name, log)
    {
        sineInputThread = new Thread((obj) => SineInput());
    }
    #pragma warning restore CS8618

    // Provides a means of setting the input from the outside.
    public void SetInput(TMeasure newMeasure)
    {
        this.LastMeasure = newMeasure;
    }

    public void StartSineInput(
        TMeasure min,
        TMeasure max,
        TimeSpan period,
        int measuresPerPeriod)
    {
        this.sineInputParameters = new SineInputParameters(min, max, period, measuresPerPeriod);
        if (isSineInputStarted)
            return;
        isSineInputStarted = true;
        sineInputThread.Start();
    }

    private void StopSineInput()
    {
        if (!isSineInputStarted)
            return;

        isSineInputStarted = false;
        sineInputThread.Join();
    }

    private void SineInput()
    {
        double step = 0;
        while (isSineInputStarted)
        {
            Thread.Sleep(this.sineInputParameters.timeStep);
            var stepRadians = (2 * Math.PI) / this.sineInputParameters.measuresPerPeriod;
            var newValue = this.sineInputParameters.offset + (decimal)(this.sineInputParameters.amplitude * Math.Sin(step * stepRadians));

            SetInput(this.sineInputParameters.min.WithNewValue<TMeasure>(newValue));

            step += 1;
            if (step >= this.sineInputParameters.measuresPerPeriod)
                step = 0;
        }
    }

    public void Dispose()
    {
        if (isSineInputStarted)
            StopSineInput();
    }
}
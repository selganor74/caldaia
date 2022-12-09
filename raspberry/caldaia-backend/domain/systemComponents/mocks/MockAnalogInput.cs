using Microsoft.Extensions.Logging;

namespace domain.systemComponents.mocks;

public class MockAnalogInput : AnalogInput, IDisposable
{
    private class SineInputParameters
    {
        public IMeasure min { get; }
        public IMeasure max { get; }
        public TimeSpan period { get; }
        public int measuresPerPeriod { get; }
        public TimeSpan timeStep => period / measuresPerPeriod;
        public double amplitude => (double)(max.Value - min.Value) / 2;
        public decimal offset => (max.Value + min.Value) / 2;

        public SineInputParameters(
            IMeasure min,
            IMeasure max,
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
    private MockAnalogInput.SineInputParameters sineInputParameters;

    public MockAnalogInput(string name, ILogger<MockAnalogInput> log) : base(name, log)
    {
        sineInputThread = new Thread((obj) => SineInput());
    }
    #pragma warning restore CS8618

    // Provides a means of setting the input from the outside.
    public void SetInput(IMeasure newMeasure)
    {
        this.LastMeasure = newMeasure;
    }

    public void StartSineInput(
        IMeasure min,
        IMeasure max,
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

            SetInput(this.sineInputParameters.min.WithNewValue(newValue));

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
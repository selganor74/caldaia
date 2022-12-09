using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents.mocks;

public class MockDigitalInput : DigitalInput, IDisposable
{
    private Thread _squareInputThread;
    private bool _isSquareInputRunning;
    private TimeSpan period = TimeSpan.FromSeconds(10);

    public MockDigitalInput(string name, ILogger<DigitalInput> log) : base(name, log)
    {
        this._squareInputThread = new Thread((obj) => SquareInput());
    }

    public void Set(OnOffState state, DateTime? utcTimeStamp = null)
    {
        this.LastMeasure = new OnOff(state, utcTimeStamp);
    }

    public void Toggle(DateTime? utcTimeStamp = null)
    {
        var newValue = (((OnOff)(this._lastMeasure))?.DigitalValue ?? OnOffState.OFF) == OnOffState.ON ? OnOffState.OFF : OnOffState.ON;
        this.LastMeasure = new OnOff(newValue, utcTimeStamp);
    }

    public void StartSquareInput(TimeSpan period)
    {
        this.period = period;
        if (this._isSquareInputRunning)
            return;

        this._isSquareInputRunning = true;
        this._squareInputThread.Start();
    }

    public void StopSquareInput()
    {
        if(!this._isSquareInputRunning)
            return;

        this._isSquareInputRunning = false;
        this._squareInputThread.Join();
    }

    private void SquareInput()
    {
        while (_isSquareInputRunning)
        {
            Thread.Sleep(this.period);
            this.Toggle();
        }
    }

    public void Dispose()
    {

    }
}
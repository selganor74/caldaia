namespace application;

public class CaldaiaConfig 
{
    // Specifies how often the main loop will cycle.
    // If loop does not end within one MainLoopPeriod, 
    // the next will start after another full loop cycle
    //
    // |-- MainLoopPeriod --|-- MainLoopPeriod --|-- MainLoopPeriod --|-- MainLoopPeriod --|
    // |-- Loop 1 --|       |-------- Loop 2 --------|                |-- Loop 3 --|
    public TimeSpan MainLoopPeriod {get;}

    public CaldaiaConfig(TimeSpan mainLoopPeriod)
    {
        MainLoopPeriod = mainLoopPeriod;
    }
}

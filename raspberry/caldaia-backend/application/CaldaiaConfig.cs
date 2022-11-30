namespace application;

public class CaldaiaConfig
{
    // Specifies how often the main loop will cycle.
    // If loop does not end within one MainLoopPeriod, 
    // the next will start after another full loop cycle
    //
    // |-- MainLoopPeriod --|-- MainLoopPeriod --|-- MainLoopPeriod --|-- MainLoopPeriod --|
    // |-- Loop 1 --|       |-------- Loop 2 --------|                |-- Loop 3 --|
    public TimeSpan MainLoopPeriod { get; }

    // Temperatura sotto alla quale la CALDAIA PARTE
    public decimal ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA { get; }

    // Temperatura sopra alla quale la CALDAIA SI FERMA
    public decimal ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA { get; }

    // Temperatura di innesco della Pompa Camino con Duty Cycle 50%
    public decimal CAMINO_T_INNESCO_50_50 { get; }

    // Temperatura di innesco della Pompa Camino
    public decimal CAMINO_T_INNESCO_75_25 { get; }
    public decimal CAMINO_T_INNESCO_100 { get; }
    public decimal CAMINO_T_INNESCO_BYPASS_AMBIENTI { get; }
    public decimal CAMINO_T_DISINNESCO_BYPASS_AMBIENTI { get; }

    public decimal DELTA_T_CAMINO_T_ROTEX_INNESCO { get; }

    public CaldaiaConfig(
        TimeSpan mainLoopPeriod,
        decimal rOTEX_T_SOGLIA_ACCENSIONE_CALDAIA = 45m,
        decimal rOTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA = 52m,
        decimal cAMINO_T_INNESCO_50_50 = 50,
        decimal cAMINO_T_INNESCO_75_25 = 55,
        decimal cAMINO_T_INNESCO_100 = 60,
        decimal cAMINO_T_BYPASS_AMBIENTI = 62,
        decimal dELTA_T_CAMINO_T_ROTEX_INNESCO = 5)
    {
        MainLoopPeriod = mainLoopPeriod;
        ROTEX_T_SOGLIA_ACCENSIONE_CALDAIA = rOTEX_T_SOGLIA_ACCENSIONE_CALDAIA;
        ROTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA = rOTEX_T_SOGLIA_SPEGNIMENTO_CALDAIA;
        CAMINO_T_INNESCO_50_50 = cAMINO_T_INNESCO_50_50;
        CAMINO_T_INNESCO_75_25 = cAMINO_T_INNESCO_75_25;
        CAMINO_T_INNESCO_100 = cAMINO_T_INNESCO_100;
        CAMINO_T_INNESCO_BYPASS_AMBIENTI = cAMINO_T_BYPASS_AMBIENTI;
        DELTA_T_CAMINO_T_ROTEX_INNESCO = dELTA_T_CAMINO_T_ROTEX_INNESCO;
    }
}

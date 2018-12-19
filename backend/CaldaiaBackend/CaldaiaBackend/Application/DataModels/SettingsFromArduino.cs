using System;

namespace CaldaiaBackend.Application.DataModels
{
    public class SettingsFromArduino
    {
        public string timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public int rotexTermoMin { get; set; }
        public int rotexTermoMax { get; set; }
        public int deltaSolare { get; set; }
        public int rotexMaxTempConCamino { get; set; }
        public int rotexMinTempConCamino { get; set; }
        public int T_ISTERESI_CALDAIA { get; set; }
        public int TEMP_SAMPLING_INTERVAL { get; set; }
        public int deltaTInnescoPompaCamino { get; set; }
        public int TCaminoPerAccensionePompa { get; set; }
        public int TInnescoSeRotexNonDisponibile { get; set; }
        public int TDisinnescoSeRotexNonDisponibile { get; set; }
        public int TInnescoOverrideTermostatoSeRotexNonDisponibile { get; set; }
        public int TDisinnescoOverrideTermostatoSeRotexNonDisponibile { get; set; }
        public int TDisinnescoOverrideSeRotexDisponibile { get; set; }
    }

}

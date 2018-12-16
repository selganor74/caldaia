using System;
using CaldaiaBackend.Application.DataModels;

namespace CaldaiaBackend.Application.Interfaces.Mocks
{
    public class ArduinoMock : IArduinoCommandIssuer, IArduinoDataReader 
    {
        private Action<DataFromArduino> _observer;
        private Action<SettingsFromArduino> _settingsObserver;
        private readonly Random _random = new Random();
        private SettingsFromArduino RandomSettings => new SettingsFromArduino
        {
            deltaSolare = _random.Next(5, 15),
            rotexMaxTempConCamino = _random.Next(68, 74),
            rotexMinTempConCamino = _random.Next(65, 68),
            rotexTermoMax = _random.Next(68, 74),
            rotexTermoMin = _random.Next(65, 68),
            TEMP_SAMPLING_INTERVAL = _random.Next(1, 10) * 1000,
            T_ISTERESI_CALDAIA = _random.Next(1, 15) * 1000
        };

        private DataFromArduino RandomData => new DataFromArduino
        {
            rotexTK = _random.Next(0, 100),
            rotexTS = _random.Next(0, 100),
            rotexP1 = _random.Next(0, 2),
            ainTempCaminoValueCentigradi = _random.Next(0, 100),
            outPompaCaminoValue = _random.Next(0, 2),
            outPompaValue = _random.Next(0, 2),
            outOverrideTermoAmbienteValue = _random.Next(0, 2)
        };

        public void SendGetCommand()
        {
            _observer?.Invoke(RandomData);
        }

        public void SendGetAndResetAccumulatorsCommand()
        {
            _observer?.Invoke(RandomData);
        }

        public void SendGetRunTimeSettingsCommand()
        {
            _settingsObserver?.Invoke(RandomSettings);
        }

        public DataFromArduino Latest => RandomData;
        public SettingsFromArduino LatestSettings => RandomSettings;

        public Action RegisterObserver(Action<DataFromArduino> observer)
        {
            _observer = observer;
            return () => { };
        }

        public Action RegisterSettingsObserver(Action<SettingsFromArduino> observer)
        {
            _settingsObserver = observer;
            return () => { };
        }
    }
}

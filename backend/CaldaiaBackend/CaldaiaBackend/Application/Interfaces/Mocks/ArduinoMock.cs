using System;
using CaldaiaBackend.Application.DataModels;

namespace CaldaiaBackend.Application.Interfaces.Mocks
{
    public class ArduinoMock : IArduinoCommandIssuer, IArduinoDataReader 
    {
        private Action<DataFromArduino> _observer;
        private Action<SettingsFromArduino> _settingsObserver;

        public void SendGetCommand()
        {
            _observer?.Invoke(new DataFromArduino());
        }

        public void SendGetAndResetAccumulatorsCommand()
        {
            _observer?.Invoke(new DataFromArduino());
        }

        public void SendGetRunTimeSettingsCommand()
        {
            _settingsObserver?.Invoke(new SettingsFromArduino());
        }

        public DataFromArduino Latest => new DataFromArduino();
        public SettingsFromArduino LatestSettings
        {
            get { return new SettingsFromArduino(); }
        }

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

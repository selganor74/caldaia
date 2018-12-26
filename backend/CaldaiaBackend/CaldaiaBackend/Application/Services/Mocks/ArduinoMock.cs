using System;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Events;
using Infrastructure.DomainEvents;

namespace CaldaiaBackend.Application.Services.Mocks
{
    public class ArduinoMock : IArduinoCommandIssuer, IArduinoDataReader 
    {
        private Action<DataFromArduino> _observer;
        private Action<SettingsFromArduino> _settingsObserver;
        private readonly Random _random = new Random();
        private int _rotexTermoMax = 43;
        private int _rotexTermoMin = 45;
        private IEventDispatcher _dispatcher;

        private SettingsFromArduino RandomSettings => new SettingsFromArduino
        {
            _type="settings",
            deltaSolare = _random.Next(5, 15),
            rotexMaxTempConCamino = _random.Next(68, 74),
            rotexMinTempConCamino = _random.Next(65, 68),
            rotexTermoMax = _rotexTermoMax,
            rotexTermoMin = _rotexTermoMin,
            TEMP_SAMPLING_INTERVAL = _random.Next(1, 10) * 1000,
            T_ISTERESI_CALDAIA = _random.Next(1, 15) * 1000
        };

        private DataFromArduino RandomData => new DataFromArduino
        {
            _type = _random.Next(0,2)<1?"data":"accumulators",
            timestamp = DateTime.UtcNow.ToString("o"),
            rotexTK = _random.Next(0, 100),
            rotexTS = _random.Next(0, 100),
            rotexP1 = _random.Next(0, 2),
            ainTempCaminoValueCentigradi = _random.Next(0, 100),
            outPompaCaminoValue = _random.Next(0, 2),
            outPompaValue = _random.Next(0, 2),
            outOverrideTermoAmbienteValue = _random.Next(0, 2)
        };

        public ArduinoMock(
            IEventDispatcher dispatcher
            )
        {
            _dispatcher = dispatcher;
        }

        public void PullOutData()
        {
            _observer?.Invoke(RandomData);
        }

        public void SendGetAndResetAccumulatorsCommand()
        {
            var toSend = RandomData;
            _observer?.Invoke(toSend);
            if (toSend._type == "accumulators")
            {
                _dispatcher.Dispatch(AccumulatorsReceived.FromData(toSend));
            }
        }

        public void PullOutSettings()
        {
            _settingsObserver?.Invoke(RandomSettings);
        }

        public void IncrementRotexTermoMax()
        {
            _rotexTermoMax = _rotexTermoMax + 1 < 65 ? _rotexTermoMax+1 : _rotexTermoMax;
        }

        public void DecrementRotexTermoMax()
        {
            _rotexTermoMax = _rotexTermoMax - 1 > _rotexTermoMin ? _rotexTermoMax - 1 : _rotexTermoMax;
        }

        public void DecrementRotexTermoMin()
        {
            _rotexTermoMin = _rotexTermoMin - 1 > 35 ? _rotexTermoMin - 1 : _rotexTermoMin;
        }

        public void IncrementRotexTermoMin()
        {
            _rotexTermoMin = _rotexTermoMin + 1 < _rotexTermoMax ? _rotexTermoMin + 1 : _rotexTermoMin;
        }

        public void SaveSettings()
        {
            
        }

        public void SendString(string toSend)
        {
            
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

        public Action RegisterRawDataObserver(Action<string> observer)
        {
            return () => { };
        }
    }
}

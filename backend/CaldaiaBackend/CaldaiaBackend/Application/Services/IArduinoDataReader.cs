using System;
using CaldaiaBackend.Application.DataModels;

namespace CaldaiaBackend.Application.Services
{
    public interface IArduinoDataReader
    {
        DataFromArduino Latest { get; }
        SettingsFromArduino LatestSettings { get; }
        Action RegisterObserver(Action<DataFromArduino> observer);
        Action RegisterSettingsObserver(Action<SettingsFromArduino> observer);
        Action RegisterRawDataObserver(Action<string> observer);
    }
}
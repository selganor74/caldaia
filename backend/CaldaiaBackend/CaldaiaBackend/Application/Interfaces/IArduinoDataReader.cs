using System;
using CaldaiaBackend.Application.DataModels;

namespace CaldaiaBackend.Application.Interfaces
{
    public interface IArduinoDataReader
    {
        DataFromArduino Latest { get; }
        SettingsFromArduino LatestSettings { get; }
        Action RegisterObserver(Action<DataFromArduino> observer);
        Action RegisterSettingsObserver(Action<SettingsFromArduino> observer);
    }
}
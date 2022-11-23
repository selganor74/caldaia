namespace raspberry_gpio;

public interface IAnalogDigitalConverter
{
        // you can read and write configuration register, because it's enabled so why not. After writeConfig you have to init. readContinuous.
        void writeConfig(byte[] config);
        Task<byte[]> readConfig();

        // methods with the treshold registers
        void TurnAlertIntoConversionReady();
        Task writeTreshold(UInt16 loTreshold, UInt16 highTreshold);

        // methods with the conversion registers. After readSingleShot or before using first time read continuous you have to init. readContinuous.
        Task readContinuousInit(ADS1115SensorSetting setting);
        int readContinuous();
        Task<ADS1115SensorData> readSingleShot(ADS1115SensorSetting setting);
}

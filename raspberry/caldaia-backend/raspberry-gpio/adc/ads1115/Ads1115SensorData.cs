namespace raspberry_gpio;

    /// <summary>
    /// After every single-shot read you get this struct. It contains a decimal value and a calculated voltage value.
    /// </summary>
    public struct ADS1115SensorData
    {
        public int DecimalValue { get; set; }
        public double VoltageValue { get; set; }
    }

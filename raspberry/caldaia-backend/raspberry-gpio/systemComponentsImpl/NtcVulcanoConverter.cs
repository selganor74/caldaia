using domain.measures;
using domain.systemComponents;
using Microsoft.Extensions.Logging;

namespace raspberry_gpio;

// Converts a value from the ADS1115 ADC to a Temperature using a model based on a pre-calculated Beta value
// see https://www.giangrandi.org/electronics/ntc/ntc.shtml
// and https://svelto.faculty.polimi.it/didattica/materiale_didattico/materiale_didattico_MA/Sensori%20di%20Temperatura.pdf
public class NtcVulcanoConverter : AnalogInputConverter<PureNumber, Temperature>
{
    const decimal Vdd = 3.3m;                   // tensione di alimentazione del circuito
    const decimal steps = 32767m;               // 2^15 - 1 valori positivi
    const decimal Radc = 1000m;                 // Resistenza di carico

    const decimal Beta = 3327.356479m;          // Beta calcolato tra 0° e 100° (vedi "Calcolo Beta Ntc.xlsx")
    const decimal Tbeta0 = 273.15m;             // T riferimento calcolo Beta = 0°C = 273.15 K
    const decimal Rntc0 = 16452.46338m;         // Resistenza dell'ntc a 0°

    private static ILogger<NtcVulcanoConverter> logger;
    private static Func<decimal, decimal> valueConverter =
        (decimal adcReading) =>
        {
            try
            {
                var Vradc = (Vdd / steps) * adcReading;     // La tensione letta dall'adc
                decimal Rntc = Radc * (Vdd - Vradc) / Vradc;

                // Make all calculations as Double ...
                var Tk = 1 / ((Math.Log((double)(Rntc / Rntc0)) / (double)Beta) + (double)(1 / Tbeta0));

                // ... and then convert it back to Decimal
                var Tcelsius = CapConvertDoubleToDecimal(Tk) - 273.15m;

                logger?.LogDebug($"adc value {adcReading} converted to Resistance {Rntc} and Temperature {Tcelsius}");

                return Tcelsius;
            }
            catch (Exception e)
            {
                logger?.LogError($"Errors converting adc Reading: {adcReading}.{Environment.NewLine}{e}");
                throw;
            }
        };

    private static decimal CapConvertDoubleToDecimal(double doubleInput)
    {
        decimal toReturn;
        if (doubleInput < (double)Decimal.MinValue)
        {
            toReturn = Decimal.MinValue;
            return toReturn;
        }

        if (doubleInput > (double)Decimal.MaxValue)
        {
            toReturn = Decimal.MaxValue;
            return toReturn;
        }

        return (decimal)doubleInput;
    }

    public NtcVulcanoConverter(
        string name,
        AnalogInput<PureNumber> adcInput,
        ILogger<NtcVulcanoConverter> log
        ) : base(name, adcInput, valueConverter, log)
    {
        logger = logger ?? log;

        log.LogDebug(
$@"
Registered {Name} reading from {adcInput.Name}

        Vdd ⃝   {Vdd} V  
            ┃
           ┏┻┓
      Rntc ┃/┃  @ 0°C:  {Rntc0} Ω
           ┗┳┛  Beta:   {Beta} K
            ┃
       Vadc ●━━━━━━━━┓   
            ┃        ┃
           ┏┻┓       ⃝͏
      Radc ┃ ┃    Adc Input
           ┗┳┛       ⃝
            ┃        ┃
            ●━━━━━━━━┛
            ┃
            ⏚
"
            );
    }
}

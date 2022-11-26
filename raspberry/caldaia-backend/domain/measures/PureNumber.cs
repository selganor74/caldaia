namespace domain.measures;

public class PureNumber : SimpleMeasure
{
    public override string UoM => "";

    public override string FormattedValue
    {
        get
        {
            var absValue = Math.Abs(Value);
            for (var exponent = -12d; exponent <= 12d; exponent += 3d)
            {
                var from = (decimal)Math.Pow(10d, exponent);
                var to = (decimal)Math.Pow(10d, exponent + 3d);
                if (from <= absValue && absValue < to)
                {
                    var value = Value / from;
                    switch (exponent)
                    {
                        case -12: return $"{value:F3}⋅10⁻¹²";
                        case -9: return $"{value:F3}⋅10⁻⁹";
                        case -6: return $"{value:F3}⋅10⁻⁶";
                        case -3: return $"{value:F3}⋅10⁻³";
                        case 0: return $"{value:F3}";
                        case 3: return $"{value:F3}⋅10³";
                        case 6: return $"{value:F3}⋅10⁶";
                        case 9: return $"{value:F3}⋅10⁹";
                        case 12: return $"{value:F3}⋅10¹²";
                    }
                }
            }
            return $"{Value:F3} {UoM}";
        }
    }

    public PureNumber(decimal value, DateTime? utcTimeStamp = null) : base(value, utcTimeStamp)
    {
    }
}

using Api.Models.Tariffs;

namespace Api.Interfaces;

public interface IWaterChargeCalculator
{
    WaterChargeResult Calculate(WaterCalculationContext context);
}

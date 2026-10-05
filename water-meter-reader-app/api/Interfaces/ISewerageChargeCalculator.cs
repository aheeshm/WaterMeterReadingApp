using Api.Models.Tariffs;

namespace Api.Interfaces;

public interface ISewerageChargeCalculator
{
    SewerageChargeResult Calculate(SewerageCalculationContext context);
}

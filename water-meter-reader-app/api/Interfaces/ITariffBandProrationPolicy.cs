using Api.Models.Tariffs;

namespace Api.Interfaces;

public interface ITariffBandProrationPolicy
{
    IReadOnlyList<TariffBand> Prorate(IReadOnlyList<TariffBand> bands, DateOnly billingStart, DateOnly billingEnd);
}

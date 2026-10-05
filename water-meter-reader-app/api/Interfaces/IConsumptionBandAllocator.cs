using Api.Models.Tariffs;

namespace Api.Interfaces;

public interface IConsumptionBandAllocator
{
    IReadOnlyList<BandAllocation> Allocate(decimal consumptionKl, IReadOnlyList<TariffBand> bands);
}

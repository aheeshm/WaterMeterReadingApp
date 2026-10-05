using Api.Interfaces;
using Api.Models.Tariffs;

namespace Api.Services;

public class ConsumptionBandAllocator : IConsumptionBandAllocator
{
    public IReadOnlyList<BandAllocation> Allocate(decimal consumptionKl, IReadOnlyList<TariffBand> bands)
    {
        var allocations = new List<BandAllocation>();
        var sortedBands = bands
            .OrderBy(band => band.SortOrder)
            .ThenBy(band => band.LowerBoundKl)
            .ToList();

        foreach (var band in sortedBands)
        {
            decimal allocatedVolume;
            if (band.UpperBoundKl.HasValue)
            {
                allocatedVolume = Math.Max(0m, Math.Min(consumptionKl, band.UpperBoundKl.Value) - band.LowerBoundKl);
            }
            else
            {
                allocatedVolume = Math.Max(0m, consumptionKl - band.LowerBoundKl);
            }

            allocatedVolume = decimal.Round(allocatedVolume, 4, MidpointRounding.AwayFromZero);
            if (allocatedVolume <= 0m)
            {
                continue;
            }

            allocations.Add(new BandAllocation
            {
                TariffBand = band,
                VolumeAllocatedKl = allocatedVolume
            });
        }

        return allocations;
    }
}

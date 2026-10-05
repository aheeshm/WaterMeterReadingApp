using Api.Interfaces;
using Api.Models.Tariffs;

namespace Api.Services;

public class WaterChargeCalculator : IWaterChargeCalculator
{
    private readonly IConsumptionBandAllocator _bandAllocator;

    public WaterChargeCalculator(IConsumptionBandAllocator bandAllocator)
    {
        _bandAllocator = bandAllocator;
    }

    public WaterChargeResult Calculate(WaterCalculationContext context)
    {
        var eligibleBands = context.Bands
            .Where(band => IsBandEligibleForPropertyValue(band, context.PropertyRateableValue))
            .OrderBy(band => band.SortOrder)
            .ToList();

        var allocations = _bandAllocator.Allocate(context.ConsumptionKl, eligibleBands);

        var lineItems = allocations
            .Select(allocation => BuildLine(allocation.TariffBand, allocation.VolumeAllocatedKl))
            .ToList();

        return new WaterChargeResult
        {
            WaterAllocations = allocations,
            LineItems = lineItems,
            TotalIncludingVat = lineItems.Sum(line => line.LineAmountIncludingVat),
            TotalExcludingVat = lineItems.Sum(line => line.LineAmountExcludingVat)
        };
    }

    private static bool IsBandEligibleForPropertyValue(TariffBand band, decimal propertyRateableValue)
    {
        var withinMaximum = !band.PropertyValueMaximum.HasValue || propertyRateableValue <= band.PropertyValueMaximum.Value;
        var aboveMinimumExclusive = !band.PropertyValueMinimumExclusive.HasValue || propertyRateableValue > band.PropertyValueMinimumExclusive.Value;
        return withinMaximum && aboveMinimumExclusive;
    }

    private static ChargeLineDraft BuildLine(TariffBand band, decimal volume)
    {
        var unroundedIncl = decimal.Round(volume * band.RateIncludingVat, 4, MidpointRounding.AwayFromZero);
        var unroundedExcl = decimal.Round(volume * band.RateExcludingVat, 4, MidpointRounding.AwayFromZero);

        return new ChargeLineDraft
        {
            ServiceType = ServiceType.WATER,
            ChargeCode = band.ChargeCode,
            BandLowerKl = band.LowerBoundKl,
            BandUpperKl = band.UpperBoundKl,
            VolumeAllocatedKl = volume,
            UnitRateIncludingVat = band.RateIncludingVat,
            UnitRateExcludingVat = band.RateExcludingVat,
            UnroundedLineAmountIncludingVat = unroundedIncl,
            UnroundedLineAmountExcludingVat = unroundedExcl,
            LineAmountIncludingVat = decimal.Round(unroundedIncl, 2, MidpointRounding.AwayFromZero),
            LineAmountExcludingVat = decimal.Round(unroundedExcl, 2, MidpointRounding.AwayFromZero),
            SourcePage = band.SourcePage
        };
    }
}

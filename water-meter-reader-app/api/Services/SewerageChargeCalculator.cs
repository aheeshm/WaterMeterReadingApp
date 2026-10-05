using Api.Interfaces;
using Api.Models.Tariffs;

namespace Api.Services;

public class SewerageChargeCalculator : ISewerageChargeCalculator
{
    public SewerageChargeResult Calculate(SewerageCalculationContext context)
    {
        var sewerBands = context.SewerBands
            .Where(band => IsBandEligibleForPropertyValue(band, context.PropertyRateableValue))
            .OrderBy(band => band.SortOrder)
            .ToList();

        var sewerBandLookup = sewerBands.ToDictionary(band => (band.LowerBoundKl, band.UpperBoundKl));
        var lineItems = new List<ChargeLineDraft>();

        foreach (var waterAllocation in context.WaterAllocations)
        {
            if (!sewerBandLookup.TryGetValue((waterAllocation.TariffBand.LowerBoundKl, waterAllocation.TariffBand.UpperBoundKl), out var sewerBand))
            {
                throw new InvalidOperationException("Missing corresponding sewer tariff band for allocated water consumption band.");
            }

            var dischargePercentage = ResolveDischargePercentage(context, sewerBand);
            var billableSewerVolume = decimal.Round(waterAllocation.VolumeAllocatedKl * dischargePercentage, 4, MidpointRounding.AwayFromZero);
            var unroundedIncl = decimal.Round(billableSewerVolume * sewerBand.RateIncludingVat, 4, MidpointRounding.AwayFromZero);
            var unroundedExcl = decimal.Round(billableSewerVolume * sewerBand.RateExcludingVat, 4, MidpointRounding.AwayFromZero);

            lineItems.Add(new ChargeLineDraft
            {
                ServiceType = ServiceType.SEWERAGE,
                ChargeCode = sewerBand.ChargeCode,
                BandLowerKl = sewerBand.LowerBoundKl,
                BandUpperKl = sewerBand.UpperBoundKl,
                VolumeAllocatedKl = waterAllocation.VolumeAllocatedKl,
                DischargePercentage = dischargePercentage,
                BillableSewerVolumeKl = billableSewerVolume,
                UnitRateIncludingVat = sewerBand.RateIncludingVat,
                UnitRateExcludingVat = sewerBand.RateExcludingVat,
                UnroundedLineAmountIncludingVat = unroundedIncl,
                UnroundedLineAmountExcludingVat = unroundedExcl,
                LineAmountIncludingVat = decimal.Round(unroundedIncl, 2, MidpointRounding.AwayFromZero),
                LineAmountExcludingVat = decimal.Round(unroundedExcl, 2, MidpointRounding.AwayFromZero),
                SourcePage = sewerBand.SourcePage
            });
        }

        return new SewerageChargeResult
        {
            LineItems = lineItems,
            TotalIncludingVat = lineItems.Sum(line => line.LineAmountIncludingVat),
            TotalExcludingVat = lineItems.Sum(line => line.LineAmountExcludingVat),
            BillableSewerVolumeKl = lineItems.Sum(line => line.BillableSewerVolumeKl ?? 0m)
        };
    }

    private static bool IsBandEligibleForPropertyValue(TariffBand band, decimal propertyRateableValue)
    {
        var withinMaximum = !band.PropertyValueMaximum.HasValue || propertyRateableValue <= band.PropertyValueMaximum.Value;
        var aboveMinimumExclusive = !band.PropertyValueMinimumExclusive.HasValue || propertyRateableValue > band.PropertyValueMinimumExclusive.Value;
        return withinMaximum && aboveMinimumExclusive;
    }

    private static decimal ResolveDischargePercentage(SewerageCalculationContext context, TariffBand sewerBand)
    {
        if (context.AgreedSewerDischargePercentage.HasValue)
        {
            return context.AgreedSewerDischargePercentage.Value;
        }

        if (context.DevelopmentType == DevelopmentType.SECTIONAL_OVER_2_STOREYS)
        {
            return 0.90m;
        }

        if (sewerBand.DischargePercentage.HasValue)
        {
            return sewerBand.DischargePercentage.Value;
        }

        return sewerBand.LowerBoundKl switch
        {
            0m => 0.95m,
            6m => 0.75m,
            25m => 0.75m,
            30m => 0.65m,
            _ => 0.60m
        };
    }
}

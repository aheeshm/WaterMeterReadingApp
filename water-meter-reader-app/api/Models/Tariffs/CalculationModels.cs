namespace Api.Models.Tariffs;

public class BandAllocation
{
    public TariffBand TariffBand { get; set; } = default!;
    public decimal VolumeAllocatedKl { get; set; }
}

public class ChargeLineDraft
{
    public int TariffScheduleId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public ServiceType ServiceType { get; set; }
    public string ChargeCode { get; set; } = string.Empty;
    public decimal BandLowerKl { get; set; }
    public decimal? BandUpperKl { get; set; }
    public decimal VolumeAllocatedKl { get; set; }
    public decimal? DischargePercentage { get; set; }
    public decimal? BillableSewerVolumeKl { get; set; }
    public decimal UnitRateIncludingVat { get; set; }
    public decimal UnitRateExcludingVat { get; set; }
    public decimal UnroundedLineAmountIncludingVat { get; set; }
    public decimal UnroundedLineAmountExcludingVat { get; set; }
    public decimal LineAmountIncludingVat { get; set; }
    public decimal LineAmountExcludingVat { get; set; }
    public string SourcePage { get; set; } = string.Empty;
}

public class WaterCalculationContext
{
    public decimal ConsumptionKl { get; set; }
    public decimal PropertyRateableValue { get; set; }
    public IReadOnlyList<TariffBand> Bands { get; set; } = Array.Empty<TariffBand>();
}

public class SewerageCalculationContext
{
    public decimal PropertyRateableValue { get; set; }
    public DevelopmentType DevelopmentType { get; set; }
    public decimal? AgreedSewerDischargePercentage { get; set; }
    public IReadOnlyList<TariffBand> SewerBands { get; set; } = Array.Empty<TariffBand>();
    public IReadOnlyList<BandAllocation> WaterAllocations { get; set; } = Array.Empty<BandAllocation>();
}

public class WaterChargeResult
{
    public IReadOnlyList<BandAllocation> WaterAllocations { get; set; } = Array.Empty<BandAllocation>();
    public IReadOnlyList<ChargeLineDraft> LineItems { get; set; } = Array.Empty<ChargeLineDraft>();
    public decimal TotalIncludingVat { get; set; }
    public decimal TotalExcludingVat { get; set; }
}

public class SewerageChargeResult
{
    public IReadOnlyList<ChargeLineDraft> LineItems { get; set; } = Array.Empty<ChargeLineDraft>();
    public decimal TotalIncludingVat { get; set; }
    public decimal TotalExcludingVat { get; set; }
    public decimal BillableSewerVolumeKl { get; set; }
}

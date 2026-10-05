namespace Api.Models.Tariffs;

public class BillingCalculationRequest
{
    public string Municipality { get; set; } = string.Empty;
    public DateOnly BillingPeriodStart { get; set; }
    public DateOnly BillingPeriodEnd { get; set; }
    public decimal PreviousReadingKl { get; set; }
    public decimal CurrentReadingKl { get; set; }
    public AccountBillingType? AccountBillingType { get; set; }
    public SupplyType? SupplyType { get; set; }
    public DevelopmentType? DevelopmentType { get; set; }
    public decimal? PropertyRateableValue { get; set; }
    public bool IncludeSewerage { get; set; }
    public bool IncludeInfrastructureSurcharges { get; set; }
    public decimal? AgreedSewerDischargePercentage { get; set; }
    public SewerInfrastructureBasis? SewerInfrastructureBasis { get; set; }
    public bool IsUnoccupiedProperty { get; set; }
    public int? ExistingPropertyAccountId { get; set; }
    public int? UserId { get; set; }
    public int DwellingUnitCount { get; set; } = 1;
    public string MunicipalAccountNumber { get; set; } = string.Empty;
    public int? ParentBulkMeterId { get; set; }
}

public class BillingCalculationResponse
{
    public int? TariffScheduleId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    public decimal ConsumptionKl { get; set; }
    public decimal TotalIncludingVat { get; set; }
    public decimal TotalExcludingVat { get; set; }
    public SewerInfrastructureBasis? SewerInfrastructureBasis { get; set; }
    public List<TariffScheduleSegmentDto> TariffSegments { get; set; } = new();
    public List<BillingLineItemDto> LineItems { get; set; } = new();
}

public class TariffScheduleSegmentDto
{
    public int TariffScheduleId { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public DateOnly ScheduleEffectiveFrom { get; set; }
    public DateOnly ScheduleEffectiveTo { get; set; }
    public DateOnly SegmentStart { get; set; }
    public DateOnly SegmentEnd { get; set; }
}

public class BillingLineItemDto
{
    public string ServiceType { get; set; } = string.Empty;
    public string ChargeCode { get; set; } = string.Empty;
    public decimal BandLowerKl { get; set; }
    public decimal? BandUpperKl { get; set; }
    public decimal VolumeAllocatedKl { get; set; }
    public decimal? DischargePercentage { get; set; }
    public decimal? BillableSewerVolumeKl { get; set; }
    public decimal UnitRateIncludingVat { get; set; }
    public decimal UnitRateExcludingVat { get; set; }
    public string VatStatus { get; set; } = "INCLUSIVE";
    public decimal UnroundedLineAmountIncludingVat { get; set; }
    public decimal UnroundedLineAmountExcludingVat { get; set; }
    public decimal LineAmountIncludingVat { get; set; }
    public decimal LineAmountExcludingVat { get; set; }
    public int TariffScheduleId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string SourcePage { get; set; } = string.Empty;
}

public class TariffScheduleResponse
{
    public int TariffScheduleId { get; set; }
    public string Municipality { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    public decimal VatRate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public List<TariffCategoryDto> Categories { get; set; } = new();
}

public class TariffCategoryDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TariffBandDto> Bands { get; set; } = new();
}

public class TariffBandDto
{
    public string ServiceType { get; set; } = string.Empty;
    public string ChargeCode { get; set; } = string.Empty;
    public decimal LowerBoundKl { get; set; }
    public decimal? UpperBoundKl { get; set; }
    public decimal RateExcludingVat { get; set; }
    public decimal RateIncludingVat { get; set; }
    public decimal? PropertyValueMaximum { get; set; }
    public decimal? PropertyValueMinimumExclusive { get; set; }
    public decimal? DischargePercentage { get; set; }
    public int SortOrder { get; set; }
    public string SourcePage { get; set; } = string.Empty;
}

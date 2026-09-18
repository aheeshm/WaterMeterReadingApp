namespace Api.Models.Tariffs;

public class CalculationLineItem
{
    public int CalculationLineItemId { get; set; }
    public int BillingCalculationId { get; set; }
    public int LineNumber { get; set; }
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

    public BillingCalculation? BillingCalculation { get; set; }
}

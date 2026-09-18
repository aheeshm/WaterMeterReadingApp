namespace Api.Models.Tariffs;

public class TariffSchedule
{
    public int TariffScheduleId { get; set; }
    public string Municipality { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string SourceDocument { get; set; } = string.Empty;
    public string SourcePage { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<TariffBand> TariffBands { get; set; } = new List<TariffBand>();
    public ICollection<BillingCalculation> BillingCalculations { get; set; } = new List<BillingCalculation>();
}

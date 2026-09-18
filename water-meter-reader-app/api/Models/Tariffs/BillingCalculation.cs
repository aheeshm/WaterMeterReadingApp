namespace Api.Models.Tariffs;

public class BillingCalculation
{
    public int BillingCalculationId { get; set; }
    public DateTime CalculationDate { get; set; }
    public int TariffScheduleId { get; set; }
    public int PropertyAccountId { get; set; }
    public decimal PreviousReadingKl { get; set; }
    public decimal CurrentReadingKl { get; set; }
    public decimal ConsumptionKl { get; set; }
    public decimal TotalIncludingVat { get; set; }
    public decimal TotalExcludingVat { get; set; }
    public string CalculationRequestJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public TariffSchedule? TariffSchedule { get; set; }
    public PropertyAccount? PropertyAccount { get; set; }
    public ICollection<CalculationLineItem> LineItems { get; set; } = new List<CalculationLineItem>();
}

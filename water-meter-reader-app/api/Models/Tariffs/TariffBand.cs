namespace Api.Models.Tariffs;

public class TariffBand
{
    public int TariffBandId { get; set; }
    public int TariffScheduleId { get; set; }
    public int TariffCategoryId { get; set; }
    public ServiceType ServiceType { get; set; }
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

    public TariffSchedule? TariffSchedule { get; set; }
    public TariffCategory? TariffCategory { get; set; }
}

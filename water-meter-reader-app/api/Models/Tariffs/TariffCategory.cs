namespace Api.Models.Tariffs;

public class TariffCategory
{
    public int TariffCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<TariffBand> TariffBands { get; set; } = new List<TariffBand>();
}

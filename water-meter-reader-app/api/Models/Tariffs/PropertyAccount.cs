using Api.Models;

namespace Api.Models.Tariffs;

public class PropertyAccount
{
    public int PropertyAccountId { get; set; }
    public AccountBillingType AccountBillingType { get; set; }
    public SupplyType SupplyType { get; set; }
    public DevelopmentType DevelopmentType { get; set; }
    public decimal PropertyRateableValue { get; set; }
    public int DwellingUnitCount { get; set; }
    public decimal? AgreedSewerDischargePercentage { get; set; }
    public string MunicipalAccountNumber { get; set; } = string.Empty;
    public int? ParentBulkMeterId { get; set; }
    public int UserId { get; set; }

    public User? User { get; set; }
    public ICollection<BillingCalculation> BillingCalculations { get; set; } = new List<BillingCalculation>();
}

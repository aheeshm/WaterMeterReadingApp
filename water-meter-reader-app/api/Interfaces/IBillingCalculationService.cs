using Api.Models.Tariffs;

namespace Api.Interfaces;

public interface IBillingCalculationService
{
    Task<BillingCalculationResponse> CalculateAsync(BillingCalculationRequest request, CancellationToken cancellationToken = default);
}

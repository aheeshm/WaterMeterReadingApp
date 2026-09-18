using Api.Models.Tariffs;

namespace Api.Interfaces;

public interface ITariffRepository
{
    Task<TariffSchedule?> GetApplicableScheduleAsync(string municipality, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TariffCategory>> GetTariffCategoriesAsync(int tariffScheduleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TariffBand>> GetTariffBandsAsync(int tariffScheduleId, CancellationToken cancellationToken = default);
}

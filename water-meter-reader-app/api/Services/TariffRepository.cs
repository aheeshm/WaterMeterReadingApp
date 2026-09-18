using Api.Data;
using Api.Interfaces;
using Api.Models.Tariffs;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class TariffRepository : ITariffRepository
{
    private readonly WaterMeterContext _context;

    public TariffRepository(WaterMeterContext context)
    {
        _context = context;
    }

    public async Task<TariffSchedule?> GetApplicableScheduleAsync(string municipality, DateOnly date, CancellationToken cancellationToken = default)
    {
        var normalizedMunicipality = municipality.Trim();
        return await _context.TariffSchedules
            .Where(schedule => schedule.IsActive
                && schedule.ApprovalStatus == "APPROVED"
                && EF.Functions.Collate(schedule.Municipality, "NOCASE") == normalizedMunicipality
                && schedule.EffectiveFrom <= date
                && schedule.EffectiveTo >= date)
            .OrderByDescending(schedule => schedule.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TariffCategory>> GetTariffCategoriesAsync(int tariffScheduleId, CancellationToken cancellationToken = default)
    {
        return await _context.TariffBands
            .Where(band => band.TariffScheduleId == tariffScheduleId)
            .Select(band => band.TariffCategory!)
            .Distinct()
            .OrderBy(category => category.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TariffBand>> GetTariffBandsAsync(int tariffScheduleId, CancellationToken cancellationToken = default)
    {
        return await _context.TariffBands
            .Include(band => band.TariffCategory)
            .Where(band => band.TariffScheduleId == tariffScheduleId)
            .OrderBy(band => band.ServiceType)
            .ThenBy(band => band.TariffCategory!.Code)
            .ThenBy(band => band.SortOrder)
            .ToListAsync(cancellationToken);
    }
}

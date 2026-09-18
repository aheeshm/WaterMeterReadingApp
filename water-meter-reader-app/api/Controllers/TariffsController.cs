using Api.Interfaces;
using Api.Models.Tariffs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/tariffs")]
public class TariffsController : ControllerBase
{
    private readonly ITariffRepository _tariffRepository;

    public TariffsController(ITariffRepository tariffRepository)
    {
        _tariffRepository = tariffRepository;
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule([FromQuery] string municipality, [FromQuery] DateOnly date, [FromQuery] SupplyType? supplyType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(municipality))
        {
            return BadRequest("Municipality is required.");
        }

        if (!supplyType.HasValue)
        {
            return BadRequest("Supply type is required.");
        }

        var schedule = await _tariffRepository.GetApplicableScheduleAsync(municipality, date, cancellationToken);
        if (schedule == null)
        {
            return NotFound("No approved tariff schedule was found for the requested date.");
        }

        var categories = await _tariffRepository.GetTariffCategoriesAsync(schedule.TariffScheduleId, cancellationToken);
        var bands = await _tariffRepository.GetTariffBandsAsync(schedule.TariffScheduleId, cancellationToken);

        var selectedCategoryCode = supplyType.Value == SupplyType.FULL_PRESSURE
            ? "DOMESTIC_FULL_PRESSURE"
            : "DOMESTIC_BREAK_PRESSURE";

        var categoryResults = categories
            .Where(category => category.Code == selectedCategoryCode
                || category.Code == "WATER_INFRASTRUCTURE"
                || category.Code == "SEWER_INFRASTRUCTURE_WATER"
                || category.Code == "SEWER_INFRASTRUCTURE_SEWERAGE")
            .Select(category => new TariffCategoryDto
            {
                Code = category.Code,
                Description = category.Description,
                Bands = bands
                    .Where(band => band.TariffCategoryId == category.TariffCategoryId)
                    .OrderBy(band => band.SortOrder)
                    .Select(band => new TariffBandDto
                    {
                        ServiceType = band.ServiceType.ToString(),
                        ChargeCode = band.ChargeCode,
                        LowerBoundKl = band.LowerBoundKl,
                        UpperBoundKl = band.UpperBoundKl,
                        RateExcludingVat = band.RateExcludingVat,
                        RateIncludingVat = band.RateIncludingVat,
                        PropertyValueMaximum = band.PropertyValueMaximum,
                        PropertyValueMinimumExclusive = band.PropertyValueMinimumExclusive,
                        DischargePercentage = band.DischargePercentage,
                        SortOrder = band.SortOrder,
                        SourcePage = band.SourcePage
                    })
                    .ToList()
            })
            .ToList();

        return Ok(new TariffScheduleResponse
        {
            TariffScheduleId = schedule.TariffScheduleId,
            Municipality = schedule.Municipality,
            FinancialYear = schedule.FinancialYear,
            EffectiveFrom = schedule.EffectiveFrom,
            EffectiveTo = schedule.EffectiveTo,
            VatRate = schedule.VatRate,
            CurrencyCode = schedule.CurrencyCode,
            Categories = categoryResults
        });
    }
}

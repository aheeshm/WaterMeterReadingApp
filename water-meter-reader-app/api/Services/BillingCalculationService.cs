using System.Text.Json;
using Api.Data;
using Api.Interfaces;
using Api.Models.Tariffs;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class BillingCalculationService : IBillingCalculationService
{
    private readonly ITariffRepository _tariffRepository;
    private readonly IWaterChargeCalculator _waterChargeCalculator;
    private readonly ISewerageChargeCalculator _sewerageChargeCalculator;
    private readonly ITariffBandProrationPolicy? _prorationPolicy;
    private readonly WaterMeterContext _context;

    public BillingCalculationService(
        ITariffRepository tariffRepository,
        IWaterChargeCalculator waterChargeCalculator,
        ISewerageChargeCalculator sewerageChargeCalculator,
        IEnumerable<ITariffBandProrationPolicy> prorationPolicies,
        WaterMeterContext context)
    {
        _tariffRepository = tariffRepository;
        _waterChargeCalculator = waterChargeCalculator;
        _sewerageChargeCalculator = sewerageChargeCalculator;
        _prorationPolicy = prorationPolicies.FirstOrDefault();
        _context = context;
    }

    public async Task<BillingCalculationResponse> CalculateAsync(BillingCalculationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var supplyType = request.SupplyType!.Value;
        var developmentType = request.DevelopmentType ?? DevelopmentType.DWELLING_HOUSE;
        var accountBillingType = request.AccountBillingType!.Value;
        var propertyRateableValue = request.PropertyRateableValue!.Value;

        if (accountBillingType == AccountBillingType.PRIVATE_SUBMETER)
        {
            throw new ArgumentException("Private sub-meters require a bulk-allocation method and cannot be calculated directly against municipal tariffs.");
        }

        if (!IsCalendarMonthPeriod(request.BillingPeriodStart, request.BillingPeriodEnd) && _prorationPolicy == null)
        {
            throw new ArgumentException("Non-calendar-month billing periods require a configured proration policy.");
        }

        var tariffSegments = await BuildTariffSegmentsAsync(request.Municipality, request.BillingPeriodStart, request.BillingPeriodEnd, cancellationToken);
        if (!tariffSegments.Any())
        {
            throw new ArgumentException("No approved active tariff schedule exists for the billing period.");
        }

        if (tariffSegments.Count > 1 && _prorationPolicy == null)
        {
            throw new ArgumentException("Billing periods spanning tariff schedule boundaries require a configured proration policy.");
        }

        var waterCategoryCode = supplyType == SupplyType.FULL_PRESSURE
            ? "DOMESTIC_FULL_PRESSURE"
            : "DOMESTIC_BREAK_PRESSURE";

        var consumptionKl = decimal.Round(request.CurrentReadingKl - request.PreviousReadingKl, 4, MidpointRounding.AwayFromZero);
        var lines = new List<ChargeLineDraft>();
        var waterResult = new WaterChargeResult
        {
            LineItems = Array.Empty<ChargeLineDraft>(),
            TotalIncludingVat = 0m,
            TotalExcludingVat = 0m,
            WaterAllocations = Array.Empty<BandAllocation>()
        };

        var sewerResult = new SewerageChargeResult
        {
            BillableSewerVolumeKl = 0m,
            LineItems = Array.Empty<ChargeLineDraft>(),
            TotalIncludingVat = 0m,
            TotalExcludingVat = 0m
        };

        var segmentDayTotal = tariffSegments.Sum(segment => segment.DayCount);
        decimal allocatedConsumption = 0m;

        for (var i = 0; i < tariffSegments.Count; i++)
        {
            var segment = tariffSegments[i];
            var segmentConsumption = i == tariffSegments.Count - 1
                ? decimal.Round(consumptionKl - allocatedConsumption, 4, MidpointRounding.AwayFromZero)
                : decimal.Round(consumptionKl * segment.DayCount / segmentDayTotal, 4, MidpointRounding.AwayFromZero);

            allocatedConsumption += segmentConsumption;

            var bands = await _tariffRepository.GetTariffBandsAsync(segment.Schedule.TariffScheduleId, cancellationToken);
            ValidateBands(bands);

            var waterBands = bands
                .Where(band => band.ServiceType == ServiceType.WATER
                    && band.TariffCategory != null
                    && band.TariffCategory.Code == waterCategoryCode)
                .ToList();

            if (_prorationPolicy != null && !IsCalendarMonthPeriod(segment.StartDate, segment.EndDate))
            {
                waterBands = _prorationPolicy.Prorate(waterBands, segment.StartDate, segment.EndDate).ToList();
            }

            if (!waterBands.Any())
            {
                throw new InvalidOperationException("No water tariff bands found for the requested supply type.");
            }

            var segmentWaterResult = _waterChargeCalculator.Calculate(new WaterCalculationContext
            {
                ConsumptionKl = segmentConsumption,
                PropertyRateableValue = propertyRateableValue,
                Bands = waterBands
            });

            foreach (var line in segmentWaterResult.LineItems)
            {
                line.TariffScheduleId = segment.Schedule.TariffScheduleId;
                line.EffectiveDate = segment.StartDate;
                lines.Add(line);
            }

            waterResult = new WaterChargeResult
            {
                WaterAllocations = waterResult.WaterAllocations.Concat(segmentWaterResult.WaterAllocations).ToList(),
                LineItems = waterResult.LineItems.Concat(segmentWaterResult.LineItems).ToList(),
                TotalIncludingVat = waterResult.TotalIncludingVat + segmentWaterResult.TotalIncludingVat,
                TotalExcludingVat = waterResult.TotalExcludingVat + segmentWaterResult.TotalExcludingVat
            };

            SewerageChargeResult? segmentSewerResult = null;
            if (request.IncludeSewerage)
            {
                var sewerBands = bands
                    .Where(band => band.ServiceType == ServiceType.SEWERAGE
                        && band.TariffCategory != null
                        && band.TariffCategory.Code == waterCategoryCode)
                    .ToList();

                if (_prorationPolicy != null && !IsCalendarMonthPeriod(segment.StartDate, segment.EndDate))
                {
                    sewerBands = _prorationPolicy.Prorate(sewerBands, segment.StartDate, segment.EndDate).ToList();
                }

                segmentSewerResult = _sewerageChargeCalculator.Calculate(new SewerageCalculationContext
                {
                    PropertyRateableValue = propertyRateableValue,
                    DevelopmentType = developmentType,
                    AgreedSewerDischargePercentage = request.AgreedSewerDischargePercentage,
                    SewerBands = sewerBands,
                    WaterAllocations = segmentWaterResult.WaterAllocations
                });

                foreach (var line in segmentSewerResult.LineItems)
                {
                    line.TariffScheduleId = segment.Schedule.TariffScheduleId;
                    line.EffectiveDate = segment.StartDate;
                    lines.Add(line);
                }

                sewerResult = new SewerageChargeResult
                {
                    BillableSewerVolumeKl = sewerResult.BillableSewerVolumeKl + segmentSewerResult.BillableSewerVolumeKl,
                    LineItems = sewerResult.LineItems.Concat(segmentSewerResult.LineItems).ToList(),
                    TotalIncludingVat = sewerResult.TotalIncludingVat + segmentSewerResult.TotalIncludingVat,
                    TotalExcludingVat = sewerResult.TotalExcludingVat + segmentSewerResult.TotalExcludingVat
                };
            }

            if (request.IncludeInfrastructureSurcharges)
            {
                var waterInfrastructureBand = FindSingleBand(bands, ServiceType.WATER, "WATER_INFRASTRUCTURE", "CCWTRINF");
                var waterInfrastructureLine = BuildInfrastructureLine(waterInfrastructureBand, ServiceType.WATER, segmentConsumption, null);
                waterInfrastructureLine.TariffScheduleId = segment.Schedule.TariffScheduleId;
                waterInfrastructureLine.EffectiveDate = segment.StartDate;
                lines.Add(waterInfrastructureLine);

                if (request.IncludeSewerage)
                {
                    if (!request.SewerInfrastructureBasis.HasValue)
                    {
                        throw new ArgumentException("A sewer infrastructure basis setting is required when sewerage infrastructure surcharges are included.");
                    }

                    var sewerVolumeBasis = request.SewerInfrastructureBasis.Value == SewerInfrastructureBasis.MeteredWaterConsumption
                        ? segmentConsumption
                        : segmentSewerResult?.BillableSewerVolumeKl ?? 0m;

                    var sewerInfrastructureBand = FindSingleBand(bands, ServiceType.SEWERAGE, "SEWER_INFRASTRUCTURE_SEWERAGE", "CCSEWINF");
                    var sewerInfrastructureLine = BuildInfrastructureLine(sewerInfrastructureBand, ServiceType.SEWERAGE, sewerVolumeBasis, null);
                    sewerInfrastructureLine.TariffScheduleId = segment.Schedule.TariffScheduleId;
                    sewerInfrastructureLine.EffectiveDate = segment.StartDate;
                    lines.Add(sewerInfrastructureLine);
                }
            }
        }

        if (request.IncludeInfrastructureSurcharges && request.IsUnoccupiedProperty && propertyRateableValue > 250000m)
        {
            var fixedChargeSegment = tariffSegments.Last();
            var fixedChargeBands = await _tariffRepository.GetTariffBandsAsync(fixedChargeSegment.Schedule.TariffScheduleId, cancellationToken);
            var fixedBand = FindSingleBand(fixedChargeBands, ServiceType.WATER, "SEWER_INFRASTRUCTURE_WATER", "CCUNOCCFIX");
            var fixedLine = BuildInfrastructureLine(fixedBand, ServiceType.WATER, 1m, null);
            fixedLine.TariffScheduleId = fixedChargeSegment.Schedule.TariffScheduleId;
            fixedLine.EffectiveDate = fixedChargeSegment.StartDate;
            lines.Add(fixedLine);
        }

        var responseLines = lines
            .Select((line, index) => new BillingLineItemDto
            {
                ServiceType = line.ServiceType.ToString(),
                ChargeCode = line.ChargeCode,
                BandLowerKl = line.BandLowerKl,
                BandUpperKl = line.BandUpperKl,
                VolumeAllocatedKl = line.VolumeAllocatedKl,
                DischargePercentage = line.DischargePercentage,
                BillableSewerVolumeKl = line.BillableSewerVolumeKl,
                UnitRateIncludingVat = line.UnitRateIncludingVat,
                UnitRateExcludingVat = line.UnitRateExcludingVat,
                VatStatus = "INCLUSIVE",
                UnroundedLineAmountIncludingVat = line.UnroundedLineAmountIncludingVat,
                UnroundedLineAmountExcludingVat = line.UnroundedLineAmountExcludingVat,
                LineAmountIncludingVat = line.LineAmountIncludingVat,
                LineAmountExcludingVat = line.LineAmountExcludingVat,
                TariffScheduleId = line.TariffScheduleId,
                EffectiveDate = line.EffectiveDate,
                SourcePage = line.SourcePage
            })
            .ToList();

        var totalIncludingVat = responseLines.Sum(line => line.LineAmountIncludingVat);
        var totalExcludingVat = lines.Sum(line => line.LineAmountExcludingVat);

        var propertyAccount = await EnsurePropertyAccountAsync(request, supplyType, developmentType, propertyRateableValue, accountBillingType, cancellationToken);
        if (propertyAccount != null)
        {
            var calculation = new BillingCalculation
            {
                CalculationDate = DateTime.UtcNow,
                TariffScheduleId = tariffSegments.First().Schedule.TariffScheduleId,
                PropertyAccountId = propertyAccount.PropertyAccountId,
                PreviousReadingKl = request.PreviousReadingKl,
                CurrentReadingKl = request.CurrentReadingKl,
                ConsumptionKl = consumptionKl,
                TotalIncludingVat = totalIncludingVat,
                TotalExcludingVat = totalExcludingVat,
                CalculationRequestJson = JsonSerializer.Serialize(request),
                CreatedAt = DateTime.UtcNow,
                LineItems = responseLines.Select((line, index) => new CalculationLineItem
                {
                    LineNumber = index + 1,
                    ServiceType = Enum.Parse<ServiceType>(line.ServiceType),
                    ChargeCode = line.ChargeCode,
                    BandLowerKl = line.BandLowerKl,
                    BandUpperKl = line.BandUpperKl,
                    VolumeAllocatedKl = line.VolumeAllocatedKl,
                    DischargePercentage = line.DischargePercentage,
                    BillableSewerVolumeKl = line.BillableSewerVolumeKl,
                    UnitRateIncludingVat = line.UnitRateIncludingVat,
                    UnitRateExcludingVat = line.UnitRateExcludingVat,
                    UnroundedLineAmountIncludingVat = line.UnroundedLineAmountIncludingVat,
                    UnroundedLineAmountExcludingVat = line.UnroundedLineAmountExcludingVat,
                    LineAmountIncludingVat = line.LineAmountIncludingVat,
                    LineAmountExcludingVat = line.LineAmountExcludingVat,
                    SourcePage = line.SourcePage
                }).ToList()
            };

            _context.BillingCalculations.Add(calculation);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new BillingCalculationResponse
        {
            TariffScheduleId = tariffSegments.First().Schedule.TariffScheduleId,
            EffectiveFrom = tariffSegments.First().Schedule.EffectiveFrom,
            EffectiveTo = tariffSegments.Last().Schedule.EffectiveTo,
            ConsumptionKl = consumptionKl,
            TotalIncludingVat = totalIncludingVat,
            TotalExcludingVat = totalExcludingVat,
            SewerInfrastructureBasis = request.SewerInfrastructureBasis,
            LineItems = responseLines
        };
    }

    private static void ValidateRequest(BillingCalculationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Municipality))
        {
            throw new ArgumentException("Municipality is required.");
        }

        if (request.PreviousReadingKl < 0m || request.CurrentReadingKl < 0m)
        {
            throw new ArgumentException("Negative meter readings are not supported.");
        }

        if (request.CurrentReadingKl < request.PreviousReadingKl)
        {
            throw new ArgumentException("Current reading cannot be lower than previous reading unless rollover is explicitly supported.");
        }

        if (request.CurrentReadingKl - request.PreviousReadingKl < 0m)
        {
            throw new ArgumentException("Calculated consumption cannot be negative.");
        }

        if (!request.PropertyRateableValue.HasValue)
        {
            throw new ArgumentException("Property rateable value is required.");
        }

        if (!request.SupplyType.HasValue)
        {
            throw new ArgumentException("Supply type is required.");
        }

        if (!request.AccountBillingType.HasValue)
        {
            throw new ArgumentException("Account billing type is required.");
        }

        if (request.IncludeSewerage && !request.DevelopmentType.HasValue)
        {
            throw new ArgumentException("Development type is required when sewerage is requested.");
        }

        if (request.BillingPeriodEnd < request.BillingPeriodStart)
        {
            throw new ArgumentException("Billing period end cannot be before billing period start.");
        }
    }

    private static bool IsCalendarMonthPeriod(DateOnly start, DateOnly end)
    {
        var firstDay = new DateOnly(start.Year, start.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        return start == firstDay && end == lastDay;
    }

    private async Task<List<TariffSegment>> BuildTariffSegmentsAsync(
        string municipality,
        DateOnly billingStart,
        DateOnly billingEnd,
        CancellationToken cancellationToken)
    {
        var segments = new List<TariffSegment>();
        TariffSchedule? currentSchedule = null;
        DateOnly? segmentStart = null;
        var cursor = billingStart;

        while (cursor <= billingEnd)
        {
            var schedule = await _tariffRepository.GetApplicableScheduleAsync(municipality, cursor, cancellationToken);
            if (schedule == null)
            {
                return new List<TariffSegment>();
            }

            if (currentSchedule == null)
            {
                currentSchedule = schedule;
                segmentStart = cursor;
            }
            else if (currentSchedule.TariffScheduleId != schedule.TariffScheduleId)
            {
                var previousDay = cursor.AddDays(-1);
                segments.Add(new TariffSegment(currentSchedule, segmentStart!.Value, previousDay));
                currentSchedule = schedule;
                segmentStart = cursor;
            }

            cursor = cursor.AddDays(1);
        }

        if (currentSchedule != null && segmentStart.HasValue)
        {
            segments.Add(new TariffSegment(currentSchedule, segmentStart.Value, billingEnd));
        }

        return segments;
    }

    private static void ValidateBands(IReadOnlyList<TariffBand> bands)
    {
        foreach (var band in bands)
        {
            if (band.UpperBoundKl.HasValue && band.UpperBoundKl.Value <= band.LowerBoundKl)
            {
                throw new InvalidOperationException($"Invalid tariff band range detected for charge code {band.ChargeCode}.");
            }
        }

        var grouped = bands
            .GroupBy(band => new
            {
                band.TariffScheduleId,
                band.TariffCategoryId,
                band.ServiceType,
                band.PropertyValueMaximum,
                band.PropertyValueMinimumExclusive
            });

        foreach (var group in grouped)
        {
            TariffBand? previous = null;
            foreach (var band in group.OrderBy(item => item.SortOrder))
            {
                if (previous != null && previous.UpperBoundKl.HasValue && band.LowerBoundKl < previous.UpperBoundKl.Value)
                {
                    throw new InvalidOperationException("Overlapping tariff bands were detected in the configured schedule.");
                }

                previous = band;
            }
        }
    }

    private static TariffBand FindSingleBand(IEnumerable<TariffBand> bands, ServiceType serviceType, string categoryCode, string chargeCode)
    {
        var match = bands.FirstOrDefault(band =>
            band.ServiceType == serviceType
            && band.TariffCategory != null
            && band.TariffCategory.Code == categoryCode
            && band.ChargeCode == chargeCode);

        if (match == null)
        {
            throw new InvalidOperationException($"Missing infrastructure tariff configuration for category {categoryCode} ({chargeCode}).");
        }

        return match;
    }

    private static ChargeLineDraft BuildInfrastructureLine(TariffBand band, ServiceType serviceType, decimal volume, decimal? dischargePercentage)
    {
        var roundedVolume = decimal.Round(volume, 4, MidpointRounding.AwayFromZero);
        var unroundedIncl = decimal.Round(roundedVolume * band.RateIncludingVat, 4, MidpointRounding.AwayFromZero);
        var unroundedExcl = decimal.Round(roundedVolume * band.RateExcludingVat, 4, MidpointRounding.AwayFromZero);

        return new ChargeLineDraft
        {
            ServiceType = serviceType,
            ChargeCode = band.ChargeCode,
            BandLowerKl = band.LowerBoundKl,
            BandUpperKl = band.UpperBoundKl,
            VolumeAllocatedKl = roundedVolume,
            DischargePercentage = dischargePercentage,
            BillableSewerVolumeKl = serviceType == ServiceType.SEWERAGE ? roundedVolume : null,
            UnitRateIncludingVat = band.RateIncludingVat,
            UnitRateExcludingVat = band.RateExcludingVat,
            UnroundedLineAmountIncludingVat = unroundedIncl,
            UnroundedLineAmountExcludingVat = unroundedExcl,
            LineAmountIncludingVat = decimal.Round(unroundedIncl, 2, MidpointRounding.AwayFromZero),
            LineAmountExcludingVat = decimal.Round(unroundedExcl, 2, MidpointRounding.AwayFromZero),
            SourcePage = band.SourcePage
        };
    }

    private async Task<PropertyAccount?> EnsurePropertyAccountAsync(
        BillingCalculationRequest request,
        SupplyType supplyType,
        DevelopmentType developmentType,
        decimal propertyRateableValue,
        AccountBillingType accountBillingType,
        CancellationToken cancellationToken)
    {
        if (!request.UserId.HasValue || request.UserId <= 0)
        {
            return null;
        }

        if (request.ExistingPropertyAccountId.HasValue)
        {
            var existing = await _context.PropertyAccounts
                .FirstOrDefaultAsync(account => account.PropertyAccountId == request.ExistingPropertyAccountId.Value, cancellationToken);

            if (existing != null)
            {
                return existing;
            }
        }

        var propertyAccount = new PropertyAccount
        {
            AccountBillingType = accountBillingType,
            SupplyType = supplyType,
            DevelopmentType = developmentType,
            PropertyRateableValue = propertyRateableValue,
            DwellingUnitCount = request.DwellingUnitCount <= 0 ? 1 : request.DwellingUnitCount,
            AgreedSewerDischargePercentage = request.AgreedSewerDischargePercentage,
            MunicipalAccountNumber = request.MunicipalAccountNumber,
            ParentBulkMeterId = request.ParentBulkMeterId,
            UserId = request.UserId.Value
        };

        _context.PropertyAccounts.Add(propertyAccount);
        await _context.SaveChangesAsync(cancellationToken);
        return propertyAccount;
    }

    private sealed record TariffSegment(TariffSchedule Schedule, DateOnly StartDate, DateOnly EndDate)
    {
        public decimal DayCount => EndDate.DayNumber - StartDate.DayNumber + 1;
    }
}

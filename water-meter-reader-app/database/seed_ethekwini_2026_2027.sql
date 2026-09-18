INSERT INTO TariffSchedules
(
    Municipality,
    FinancialYear,
    EffectiveFrom,
    EffectiveTo,
    ApprovalStatus,
    VatRate,
    CurrencyCode,
    SourceDocument,
    SourcePage,
    CreatedDate,
    IsActive
)
SELECT
    'eThekwini',
    '2026/2027',
    '2026-07-01',
    '2027-06-30',
    'APPROVED',
    0.1500,
    'ZAR',
    'eThekwini Final 2026/2027 Tariff Tables',
    '265,280,281,284,285',
    CURRENT_TIMESTAMP,
    1
WHERE NOT EXISTS
(
    SELECT 1
    FROM TariffSchedules
    WHERE Municipality = 'eThekwini'
      AND FinancialYear = '2026/2027'
      AND EffectiveFrom = '2026-07-01'
      AND EffectiveTo = '2027-06-30'
);

INSERT OR IGNORE INTO TariffCategories (Code, Description) VALUES
('DOMESTIC_FULL_PRESSURE', 'Domestic full-pressure water and sewerage tariffs'),
('DOMESTIC_BREAK_PRESSURE', 'Domestic break-pressure water and sewerage tariffs'),
('SEWER_INFRASTRUCTURE_WATER', 'Sewerage-related fixed water infrastructure tariffs'),
('SEWER_INFRASTRUCTURE_SEWERAGE', 'Sewerage infrastructure volumetric tariffs'),
('WATER_INFRASTRUCTURE', 'Water infrastructure volumetric tariffs');

INSERT OR IGNORE INTO TariffBands
(
    TariffScheduleId,
    TariffCategoryId,
    ServiceType,
    ChargeCode,
    LowerBoundKl,
    UpperBoundKl,
    RateExcludingVat,
    RateIncludingVat,
    PropertyValueMaximum,
    PropertyValueMinimumExclusive,
    DischargePercentage,
    SortOrder,
    SourcePage
)
SELECT
    schedule.TariffScheduleId,
    category.TariffCategoryId,
    rows.ServiceType,
    rows.ChargeCode,
    rows.LowerBoundKl,
    rows.UpperBoundKl,
    rows.RateExcludingVat,
    rows.RateIncludingVat,
    rows.PropertyValueMaximum,
    rows.PropertyValueMinimumExclusive,
    rows.DischargePercentage,
    rows.SortOrder,
    rows.SourcePage
FROM
(
    VALUES
    -- Domestic full-pressure water
    ('DOMESTIC_FULL_PRESSURE', 'WATER', 'CCDOMFPRC', 0.0, 6.0, 0.00, 0.00, 350000.00, NULL, NULL, 1, '265'),
    ('DOMESTIC_FULL_PRESSURE', 'WATER', 'CCDOMFPRC', 0.0, 6.0, 44.10, 50.70, NULL, 350000.00, NULL, 2, '265'),
    ('DOMESTIC_FULL_PRESSURE', 'WATER', 'CCDOMFPCC', 6.0, 25.0, 52.30, 60.10, NULL, NULL, NULL, 3, '265'),
    ('DOMESTIC_FULL_PRESSURE', 'WATER', 'CCDOMFPCC', 25.0, 30.0, 69.60, 80.00, NULL, NULL, NULL, 4, '265'),
    ('DOMESTIC_FULL_PRESSURE', 'WATER', 'CCDOMFPCC', 30.0, 45.0, 107.40, 123.50, NULL, NULL, NULL, 5, '265'),
    ('DOMESTIC_FULL_PRESSURE', 'WATER', 'CCDOMFPCC', 45.0, NULL, 118.00, 135.70, NULL, NULL, NULL, 6, '265'),

    -- Domestic break-pressure water
    ('DOMESTIC_BREAK_PRESSURE', 'WATER', 'CCDOMSPCC', 0.0, 6.0, 0.00, 0.00, NULL, NULL, NULL, 1, '265'),
    ('DOMESTIC_BREAK_PRESSURE', 'WATER', 'CCDOMSPCC', 6.0, 25.0, 35.50, 40.80, NULL, NULL, NULL, 2, '265'),
    ('DOMESTIC_BREAK_PRESSURE', 'WATER', 'CCDOMSPCC', 25.0, 30.0, 48.60, 55.90, NULL, NULL, NULL, 3, '265'),
    ('DOMESTIC_BREAK_PRESSURE', 'WATER', 'CCDOMSPCC', 30.0, 45.0, 107.40, 123.50, NULL, NULL, NULL, 4, '265'),
    ('DOMESTIC_BREAK_PRESSURE', 'WATER', 'CCDOMSPCC', 45.0, NULL, 118.00, 135.70, NULL, NULL, NULL, 5, '265'),

    -- Domestic full-pressure sewerage (CCSWDOM)
    ('DOMESTIC_FULL_PRESSURE', 'SEWERAGE', 'CCSWDOM', 0.0, 6.0, 0.00, 0.00, 350000.00, NULL, 0.95, 1, '280,281,284'),
    ('DOMESTIC_FULL_PRESSURE', 'SEWERAGE', 'CCSWDOM', 0.0, 6.0, 6.53, 7.50, NULL, 350000.00, 0.95, 2, '280,281,284'),
    ('DOMESTIC_FULL_PRESSURE', 'SEWERAGE', 'CCSWDOM', 6.0, 25.0, 11.00, 12.70, NULL, NULL, 0.75, 3, '280,281,284'),
    ('DOMESTIC_FULL_PRESSURE', 'SEWERAGE', 'CCSWDOM', 25.0, 30.0, 21.00, 24.20, NULL, NULL, 0.75, 4, '280,281,284'),
    ('DOMESTIC_FULL_PRESSURE', 'SEWERAGE', 'CCSWDOM', 30.0, 45.0, 32.80, 37.70, NULL, NULL, 0.65, 5, '280,281,284'),
    ('DOMESTIC_FULL_PRESSURE', 'SEWERAGE', 'CCSWDOM', 45.0, NULL, 37.60, 43.20, NULL, NULL, 0.60, 6, '280,281,284'),

    -- Domestic break-pressure sewerage (CCSEWSP)
    ('DOMESTIC_BREAK_PRESSURE', 'SEWERAGE', 'CCSEWSP', 0.0, 6.0, 0.00, 0.00, NULL, NULL, 0.95, 1, '280,281,284'),
    ('DOMESTIC_BREAK_PRESSURE', 'SEWERAGE', 'CCSEWSP', 6.0, 25.0, 10.60, 12.20, NULL, NULL, 0.75, 2, '280,281,284'),
    ('DOMESTIC_BREAK_PRESSURE', 'SEWERAGE', 'CCSEWSP', 25.0, 30.0, 14.60, 16.80, NULL, NULL, 0.75, 3, '280,281,284'),
    ('DOMESTIC_BREAK_PRESSURE', 'SEWERAGE', 'CCSEWSP', 30.0, 45.0, 32.20, 37.00, NULL, NULL, 0.65, 4, '280,281,284'),
    ('DOMESTIC_BREAK_PRESSURE', 'SEWERAGE', 'CCSEWSP', 45.0, NULL, 36.50, 42.00, NULL, NULL, 0.60, 5, '280,281,284'),

    -- Infrastructure surcharges
    ('WATER_INFRASTRUCTURE', 'WATER', 'CCWTRINF', 0.0, NULL, 1.48, 1.70, NULL, NULL, NULL, 1, '285'),
    ('SEWER_INFRASTRUCTURE_SEWERAGE', 'SEWERAGE', 'CCSEWINF', 0.0, NULL, 1.48, 1.70, NULL, NULL, NULL, 1, '285'),
    ('SEWER_INFRASTRUCTURE_WATER', 'WATER', 'CCUNOCCFIX', 0.0, NULL, 50.00, 57.50, NULL, 250000.00, NULL, 1, '285')
) AS rows(CategoryCode, ServiceType, ChargeCode, LowerBoundKl, UpperBoundKl, RateExcludingVat, RateIncludingVat, PropertyValueMaximum, PropertyValueMinimumExclusive, DischargePercentage, SortOrder, SourcePage)
JOIN TariffSchedules schedule
    ON schedule.Municipality = 'eThekwini'
   AND schedule.FinancialYear = '2026/2027'
JOIN TariffCategories category
    ON category.Code = rows.CategoryCode;

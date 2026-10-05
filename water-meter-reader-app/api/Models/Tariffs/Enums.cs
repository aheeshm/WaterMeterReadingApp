using System.Text.Json.Serialization;

namespace Api.Models.Tariffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceType
{
    WATER,
    SEWERAGE
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountBillingType
{
    INDIVIDUAL_MUNICIPAL,
    COMPLEX_BULK_MUNICIPAL,
    PRIVATE_SUBMETER
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupplyType
{
    FULL_PRESSURE,
    BREAK_PRESSURE_TANK
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DevelopmentType
{
    DWELLING_HOUSE,
    SECTIONAL_MAX_2_STOREYS,
    SECTIONAL_OVER_2_STOREYS
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SewerInfrastructureBasis
{
    MeteredWaterConsumption,
    CalculatedSewerVolume
}

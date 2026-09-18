using Api.Interfaces;
using Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddScoped<Api.Interfaces.IUserService, Api.Services.UserService>();
builder.Services.AddScoped<Api.Interfaces.IWaterReadingService, Api.Services.WaterReadingService>();
builder.Services.AddScoped<Api.Services.ImageAnalysisService>();
builder.Services.AddScoped<ITariffRepository, TariffRepository>();
builder.Services.AddScoped<IConsumptionBandAllocator, ConsumptionBandAllocator>();
builder.Services.AddScoped<IWaterChargeCalculator, WaterChargeCalculator>();
builder.Services.AddScoped<ISewerageChargeCalculator, SewerageChargeCalculator>();
builder.Services.AddScoped<IBillingCalculationService, BillingCalculationService>();
builder.Services.AddDbContext<Api.Data.WaterMeterContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

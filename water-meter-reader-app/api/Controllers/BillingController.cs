using Api.Interfaces;
using Api.Models.Tariffs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingCalculationService _billingCalculationService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingCalculationService billingCalculationService, ILogger<BillingController> logger)
    {
        _billingCalculationService = billingCalculationService;
        _logger = logger;
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate([FromBody] BillingCalculationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _billingCalculationService.CalculateAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Billing calculation failed because tariff configuration is invalid.");
            return UnprocessableEntity("Billing calculation could not be completed due to tariff configuration issues.");
        }
    }
}

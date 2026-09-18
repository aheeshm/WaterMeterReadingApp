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

    public BillingController(IBillingCalculationService billingCalculationService)
    {
        _billingCalculationService = billingCalculationService;
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
            return Problem(title: "Billing calculation configuration error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

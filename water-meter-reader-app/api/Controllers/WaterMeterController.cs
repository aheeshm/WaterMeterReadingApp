using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Api.Models;
using Api.Services;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Api.Interfaces;
using System;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaterMeterController : ControllerBase
    {
        private readonly WaterMeterContext _context;
        private readonly ImageAnalysisService _imageAnalysisService;
        private readonly IWaterReadingService _waterReadingService;

        public WaterMeterController(WaterMeterContext context, ImageAnalysisService imageAnalysisService, IWaterReadingService waterReadingService)
        {
            _context = context;
            _imageAnalysisService = imageAnalysisService;
            _waterReadingService = waterReadingService;
        }

        [HttpGet("readings/{userId}")]
        public async Task<IActionResult> GetReadings(int userId)
        {
            var readings = await _waterReadingService.GetUserReadingsAsync(userId);
            return Ok(readings);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] int userId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (userId <= 0)
            {
                return BadRequest("A valid user is required.");
            }

            var tempFilePath = Path.GetTempFileName();

            try
            {
                await using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var analysisResult = await _imageAnalysisService.AnalyzeImageAsync(tempFilePath);
                if (!analysisResult.IsMeterImage)
                {
                    return BadRequest(analysisResult.Message);
                }

                var previousReading = await _waterReadingService.GetLatestUserReadingAsync(userId);
                if (previousReading != null && analysisResult.Reading < previousReading.Reading)
                {
                    return BadRequest(
                        $"Detected reading {analysisResult.Reading} is lower than the previous reading {previousReading.Reading}. Upload a current meter image with a reading greater than or equal to the previous month.");
                }

                var success = await _waterReadingService.AddUserReadingAsync(
                    1,
                    analysisResult.Reading,
                    DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    userId);

                if (!success)
                {
                    return StatusCode(500, "Failed to add reading.");
                }

                var activeRate = await _context.RateConfigs
                    .OrderByDescending(rate => rate.EffectiveDate)
                    .Select(rate => (decimal?)rate.Rate)
                    .FirstOrDefaultAsync();

                var cost = activeRate.HasValue
                    ? decimal.Round(analysisResult.Reading * activeRate.Value, 2)
                    : (decimal?)null;

                return Ok(new
                {
                    Reading = analysisResult.Reading,
                    Cost = cost,
                    Date = DateTime.UtcNow,
                    UserId = userId,
                    Message = "Upload successful!"
                });
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }

        [HttpGet("cost/{unitId}")]
        public async Task<IActionResult> CalculateCost(int unitId)
        {
            var rateConfig = await _context.RateConfigs
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();

            if (rateConfig == null)
            {
                return NotFound("Rate configuration not found.");
            }

            var readings = await _context.WaterReadings
                .Where(r => r.UnitId == unitId)
                .ToListAsync();

            if (!readings.Any())
            {
                return NotFound("No readings found for the specified unit.");
            }

            var totalUsage = readings.Sum(r => r.Reading);
            var totalCost = totalUsage * rateConfig.Rate;

            return Ok(new { TotalUsage = totalUsage, TotalCost = totalCost });
        }
    }
}
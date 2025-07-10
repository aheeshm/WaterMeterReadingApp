using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Api.Models;
using Api.Services;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Api.Interfaces;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaterMeterController : ControllerBase
    {
        private readonly WaterMeterContext _context;
        private readonly ImageAnalysisService _imageAnalysisService;

        public WaterMeterController(WaterMeterContext context, ImageAnalysisService imageAnalysisService)
        {
            _context = context;
            _imageAnalysisService = imageAnalysisService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] int userId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Save the uploaded file to a temporary location
            var tempFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Analyze the image using the file path
            var readingValue = await _imageAnalysisService.AnalyzeImageAsync(tempFilePath);

            // Delete the temp file after analysis
            System.IO.File.Delete(tempFilePath);

            // Since AnalyzeImageAsync returns int, check for a valid value (e.g., > 0)
            if (readingValue <= 0)
            {
                return BadRequest("Could not analyze the image.");
            }

            var success = await _waterReadingService.AddUserReadingAsync(1, readingValue, DateTime.Now.ToString("yyyy-MM-dd"), userId);
            if (!success)
                return StatusCode(500, "Failed to add reading.");
            return Ok(new { Reading = readingValue, Date = DateTime.Now, UserId = userId });
        }

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
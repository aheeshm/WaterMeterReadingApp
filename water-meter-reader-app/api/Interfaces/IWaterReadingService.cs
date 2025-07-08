using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Interfaces
{
    public interface IWaterReadingService
    {
        Task<List<WaterReading>> GetUserReadingsAsync(int userId);
    }
}

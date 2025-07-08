using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Api.Interfaces;
using Api.Models;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class WaterReadingService : IWaterReadingService
    {
        private readonly WaterMeterContext _context;
        public WaterReadingService(WaterMeterContext context)
        {
            _context = context;
        }

        public async Task<List<WaterReading>> GetUserReadingsAsync(int userId)
        {
            string sqlPath = Path.Combine("SqlScripts", "GetUserReadings.sql");
            string sql = File.ReadAllText(sqlPath);
            // SQLite does not support named parameters in FromSqlRaw, so use string.Replace
            sql = sql.Replace("@userId", userId.ToString());
            return await _context.WaterReadings.FromSqlRaw(sql).ToListAsync();
        }
    }
}

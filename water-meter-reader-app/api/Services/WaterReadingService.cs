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
            var sqlPath = Path.Combine("SqlScripts", "GetUserReadings.sql");
            var sql = File.ReadAllText(sqlPath);
            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@userId";
                    param.Value = userId;
                    cmd.Parameters.Add(param);
                    var result = new List<WaterReading>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new WaterReading
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("WaterReadingId")),
                                UnitId = reader.GetInt32(reader.GetOrdinal("UnitId")),
                                Reading = reader.GetInt32(reader.GetOrdinal("Reading")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                            });
                        }
                    }
                    return result;
                }
            }
        }

        public async Task<bool> AddUserReadingAsync(int unitId, int reading, string date, int userId)
        {
            var sqlPath = Path.Combine("SqlScripts", "AddUserReading.sql");
            var sql = File.ReadAllText(sqlPath);
            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var p1 = cmd.CreateParameter();
                    p1.ParameterName = "@unitId";
                    p1.Value = unitId;
                    cmd.Parameters.Add(p1);
                    var p2 = cmd.CreateParameter();
                    p2.ParameterName = "@reading";
                    p2.Value = reading;
                    cmd.Parameters.Add(p2);
                    var p3 = cmd.CreateParameter();
                    p3.ParameterName = "@date";
                    p3.Value = date;
                    cmd.Parameters.Add(p3);
                    var p4 = cmd.CreateParameter();
                    p4.ParameterName = "@userId";
                    p4.Value = userId;
                    cmd.Parameters.Add(p4);
                    var rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }
    }
}

using System.Threading.Tasks;
using Api.Models;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Api.Interfaces;

namespace Api.Services
{
    public class UserService : IUserService
    {
        private readonly WaterMeterContext _context;
        public UserService(WaterMeterContext context)
        {
            _context = context;
        }


        public async Task<bool> UsernameExistsAsync(string username)
        {
            var sqlPath = Path.Combine("SqlScripts", "LoginUser.sql");
            var sql = File.ReadAllText(sqlPath);
            sql = sql.Replace("@username", $"'{username}'").Replace("@passwordHash", "'dummy'");
            // Only check username, ignore password for existence check
            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT 1 FROM Users WHERE Username = @username";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@username";
                    param.Value = username;
                    cmd.Parameters.Add(param);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }


        public async Task<bool> RegisterAsync(string username, string password, string propertyAddress)
        {
            var sqlPath = Path.Combine("SqlScripts", "RegisterUser.sql");
            var sql = File.ReadAllText(sqlPath);
            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var param1 = cmd.CreateParameter();
                    param1.ParameterName = "@username";
                    param1.Value = username;
                    cmd.Parameters.Add(param1);
                    var param2 = cmd.CreateParameter();
                    param2.ParameterName = "@passwordHash";
                    param2.Value = HashPassword(password);
                    cmd.Parameters.Add(param2);
                    var param3 = cmd.CreateParameter();
                    param3.ParameterName = "@propertyAddress";
                    param3.Value = propertyAddress;
                    cmd.Parameters.Add(param3);
                    var rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }


        public async Task<User> LoginAsync(string username, string password)
        {
            var sqlPath = Path.Combine("SqlScripts", "LoginUser.sql");
            var sql = File.ReadAllText(sqlPath);
            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var param1 = cmd.CreateParameter();
                    param1.ParameterName = "@username";
                    param1.Value = username;
                    cmd.Parameters.Add(param1);
                    var param2 = cmd.CreateParameter();
                    param2.ParameterName = "@passwordHash";
                    param2.Value = HashPassword(password);
                    cmd.Parameters.Add(param2);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                                PropertyAddress = reader.GetString(reader.GetOrdinal("PropertyAddress"))
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}

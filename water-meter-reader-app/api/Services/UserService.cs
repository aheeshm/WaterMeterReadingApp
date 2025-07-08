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
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User> RegisterAsync(string username, string password, string propertyAddress)
        {
            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                PropertyAddress = propertyAddress
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.PasswordHash != HashPassword(password))
                return null;
            return user;
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

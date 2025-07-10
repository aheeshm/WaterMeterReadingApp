using System.Threading.Tasks;
using Api.Models;

namespace Api.Interfaces
{
    public interface IUserService
    {
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> RegisterAsync(string username, string password, string propertyAddress);
        Task<User> LoginAsync(string username, string password);
        string HashPassword(string password);
    }
}

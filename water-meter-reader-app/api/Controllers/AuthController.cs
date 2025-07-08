using Microsoft.AspNetCore.Mvc;
using Api.Data;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _userService.UsernameExistsAsync(request.Username))
                return BadRequest("Username already exists.");

            var user = await _userService.RegisterAsync(request.Username, request.Password, request.PropertyAddress);
            return Ok("Registration successful.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.LoginAsync(request.Username, request.Password);
            if (user == null)
                return Unauthorized("Invalid username or password.");
            return Ok(new { userId = user.UserId, username = user.Username, address = user.PropertyAddress });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PropertyAddress { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

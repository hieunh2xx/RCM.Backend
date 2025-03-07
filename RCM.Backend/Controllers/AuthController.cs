using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RCM.Backend.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RCM.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RetailChainContext _context;
        private readonly IConfiguration _config;

        public AuthController(RetailChainContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Đăng ký tài khoản
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            var existingUser = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == request.Username);
            if (existingUser != null)
                return BadRequest("Username already exists.");

            var newAccount = new Account
            {
                EmployeeId = request.EmployeeId,
                Username = request.Username,
                PasswordHash = request.Password, // Lưu trực tiếp mật khẩu (Không mã hóa - Không an toàn)
                Role = request.Role
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account registered successfully!" });
        }

        // Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == request.Username && a.PasswordHash == request.Password);

            if (account == null)
                return Unauthorized("Invalid username or password.");

            return Ok(account);
        }

     
    }
}

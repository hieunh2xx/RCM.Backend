using DataLayerObject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using RCM.Backend.DTO;
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

     
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login request)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == request.Username);

            if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
                return Unauthorized("Invalid username or password.");

            // Tạo JWT token
            var tokenString = GenerateJwtToken(account);
             
            return Ok(new
            {
                token = tokenString,
                role = account.Role,
                username = account.Username,
           employeeId = account.EmployeeId
        });
        }

        private string GenerateJwtToken(Account account)
        {
            var jwtKey = _config["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("JWT SecretKey is missing in configuration.");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, account.Username),
        new Claim("EmployeeId", account.EmployeeId.ToString())
    };

            if (account.Role.HasValue)
            {
                var roleName = ((UserRole)account.Role.Value).ToString(); // Chuyển byte -> Enum -> String
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

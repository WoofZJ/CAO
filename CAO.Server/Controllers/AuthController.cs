using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CAO.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CAO.Server.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") 
                            ?? _configuration["AdminSettings:Password"];

        if (request.Password != adminPassword)
        {
            return Unauthorized();
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!); 
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Role, "Administrator")
            ]),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString });
    }
}
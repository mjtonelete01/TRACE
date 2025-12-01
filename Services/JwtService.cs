using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TraceWebApi.Interfaces;
using TraceWebApi.Models.Users;

namespace TraceWebApi.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime Exp) GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Surname, user.Lastname),
            new Claim(ClaimTypes.Name, user.Firstname),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        var exp = DateTime.UtcNow.AddMinutes(30);

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"]!,
            _configuration["JwtSettings:Issuer"]!,
            claims,
            expires: exp,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), exp);
    }
}

using BankingManagement.Data;
using BankingManagement.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankingManagement.Services
{

    public interface IJwtTokenService
    {
        string GenerateToken(AllUser user);
    }
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(AllUser user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role)
        };
            
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
                );

                var credentials = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
            
            
        }

    }
}

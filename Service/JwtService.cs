using Microsoft.IdentityModel.Tokens;
using QLCSV.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QLCSV.Service
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config) => _config = config;

        public string GenerateToken(User user)
        {
            // Try environment variable first, fallback to config
            var keyString = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                           ?? _config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(keyString))
                throw new InvalidOperationException("JWT_SECRET_KEY is not configured");

            // Lấy số ngày hết hạn, nếu không cấu hình thì mặc định 7 ngày
            var expireDaysStr = Environment.GetEnvironmentVariable("JWT_EXPIRE_DAYS");
            var expireDays = int.TryParse(expireDaysStr, out var days) ? days : 7;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "pending")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "qlcsv-api",
                audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "qlcsv-client",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

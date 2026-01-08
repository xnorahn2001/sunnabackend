using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SonnaBackend.Data;
using SonnaBackend.Models;
using BCrypt.Net;

namespace SonnaBackend.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(string identifier, string password);
        Task<User?> RegisterAsync(User user, string plainPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly SonnaDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(SonnaDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User?> RegisterAsync(User user, string plainPassword)
        {
            // Hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<string?> LoginAsync(string identifier, string password)
        {
            // Check by Phone OR CRNumber
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier || u.CRNumber == identifier);
            if (user == null) return null;

            bool isPasswordValid = false;
            
            // Try BCrypt verify
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch
            {
                // Fallback: Check plain text (Dev only logic as requested)
                if (user.PasswordHash == password) isPasswordValid = true;
            }

            // Explicit fallback check if BCrypt failed but didn't throw (returns false)
            if (!isPasswordValid && user.PasswordHash == password)
            {
                isPasswordValid = true;
            }

            if (!isPasswordValid) return null;

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            if (string.IsNullOrEmpty(secretKey)) throw new Exception("JWT SecretKey is missing");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.UserType),
                new Claim("FullName", user.FullName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

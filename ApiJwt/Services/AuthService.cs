using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ApiJwt.Data;
using ApiJwt.Entities;
using ApiJwt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiJwt.Services
{
    public class AuthService(UserDbContext dbContext, IConfiguration configuration) : IAuthService
    {
        private readonly UserDbContext _dbContext = dbContext;
        private readonly IConfiguration _config = configuration;


        public async Task<TokenResponseDto?> LoginAsync(UserDto reqData)
        {
            var user = await GetUserByName(reqData.Name);
            if (user is null) return null;

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, reqData.Password) == PasswordVerificationResult.Failed)
                return null;

            return await CreateTokenResponse(user);
        }

        public async Task<User?> RegisterAsync(UserDto reqData)
        {
            var userExists = await GetUserByName(reqData.Name);
            Console.WriteLine($"{userExists}");
     
            if (userExists != null) return null;

            User user = new(); 

            var hashedPassword = new PasswordHasher<User>()
                    .HashPassword(user, reqData.Password);

            user.UserName = reqData.Name;
            user.PasswordHash = hashedPassword;

            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetUserByName(string name)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(
                                u => u.UserName == name
            );
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null) return null;

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User? user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user!),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user!)
            };
        }

        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefTokenExpTime <= DateTime.UtcNow)
                return null;
            
            return user;
        }
    
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            Console.WriteLine($"{randomNumber}");

            using var rng = RandomNumberGenerator.Create();
            Console.WriteLine($"{rng}");

            rng.GetBytes(randomNumber);
            Console.WriteLine($"{rng}");

            string number = Convert.ToBase64String(randomNumber);
            Console.WriteLine($"{rng}");

            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            Console.WriteLine($"{refreshToken}");
            user.RefreshToken = refreshToken;
            user.RefTokenExpTime = DateTime.UtcNow.AddDays(1);
            await _dbContext.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claim = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetValue<string>("AppSettings:Token")!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _config.GetValue<string>("AppSettings:Issuer"),
                audience: _config.GetValue<string>("AppSettings:Audience"),
                claims: claim,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        }


    }
}
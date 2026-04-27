using AuthService.Data;
using AuthService.Models;
using AuthService.Models.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RefreshTokenProvider _refreshTokenProvider;
        private readonly AuthDbContext _context;
        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            RefreshTokenProvider refreshTokenProvider,
            AuthDbContext context
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _refreshTokenProvider = refreshTokenProvider;
            _context = context;
        }

        // register returns token so user skips unneccessary login after registering
        public async Task<(IdentityResult Result, string? Token)> RegisterUserAsync(RegisterRequestDto dto)
        {
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return (result, null);

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.User.ToString());

            if (!roleResult.Succeeded)
                return (roleResult, null);

            var token = GenerateJwtToken(user);

            return (IdentityResult.Success, token);
        }


        public async Task<SignInResult> LoginUserAsync(LoginRequestDto dto)
        {
            return await _signInManager.PasswordSignInAsync(
                dto.Username,
                dto.Password,
                isPersistent: false,
                lockoutOnFailure: true);
        }

        public async Task<RefreshResult> RefreshAsync(RefreshTokenDto dto)
        {
            var incomingHash = _refreshTokenProvider.HashToken(dto.RefreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    t => t.TokenHash == incomingHash &&
                    !t.IsRevoked &&
                    t.ExpiresAt > DateTime.UtcNow);

            if (storedToken == null)
                return new RefreshResult(false, null, null);

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
                return new RefreshResult(false, null, null);

            // revoke old refresh token
            storedToken.IsRevoked = true;

            // create new refresh token
            var refreshTokenValue = _refreshTokenProvider.GenerateRefreshToken();
            var refreshTokenHash = _refreshTokenProvider.HashToken(refreshTokenValue);

            var refreshToken = new RefreshToken
            {
                TokenHash = refreshTokenHash,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var newJwtToken = GenerateJwtToken(user);

            return new RefreshResult(true, newJwtToken, refreshTokenValue);
        }



        public async Task<User?> GetUserByUsername(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }
        public async Task<User?> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public string GenerateJwtToken(User user)
        {
            var roles = _userManager.GetRolesAsync(user).Result;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpiresMinutes"]!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
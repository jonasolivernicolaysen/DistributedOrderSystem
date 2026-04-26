using AuthService.Models;
using AuthService.Models.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AuthService.Data;

namespace IdentityPlatformApi.Controllers
{

    [ApiController]
    [Route("/api/auth")]
    public class AuthController : ControllerBase
    {

        private readonly UserService _userService;
        private readonly RefreshTokenProvider _refreshTokenProvider;
        private readonly AuthDbContext _context;


        public AuthController(
            UserService userService,
            RefreshTokenProvider refreshTokenProvider,
            AuthDbContext context
            )
        {
            _userService = userService;
            _refreshTokenProvider = refreshTokenProvider;
            _context = context;
        }


        // register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var (result, token) = await _userService.RegisterUserAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Registration failed"
                });
            }
            return Ok(new { token });
        }


        // login
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var result = await _userService.LoginUserAsync(dto);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return Forbid();
                }

                return Unauthorized(new ErrorResponseDto
                {
                    Message = "Invalid username or password"
                });
            }

            var user = await _userService.GetUserByUsername(dto.Username);

            if (user == null)
                return Unauthorized(new ErrorResponseDto
                {
                    Message = "Invalid username or password"
                });

            var JwtToken = _userService.GenerateJwtToken(user);

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

            return Ok(new
            {
                JwtToken,
                RefreshToken = refreshTokenValue
            });
        }

        // refresh token issuance
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                return BadRequest(
                    new ErrorResponseDto
                    {
                        Message = "Invalid refresh token"
                    });
            }

            var result = await _userService.RefreshAsync(dto);

            if (!result.Succeeded)
                return Unauthorized(
                    new ErrorResponseDto
                    {
                        Message = "Invalid refresh token"
                    });

            return Ok(new
            {
                JwtToken = result.JwtToken,
                RefreshToken = result.RefreshToken
            });
        }



        // check if authorization works
        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            return Ok(new
            {
                Message = "JWT is working",
                User = User.Identity!.Name ?? "no name",
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }


        // check if admin role works
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return Ok("You are an admin");
        }

        [HttpGet("user_or_admin")]
        [Authorize(Roles = "Admin,User")]


        // check if roles work
        public IActionResult AdminOrUser()
        {
            return Ok("You are an admin or an user");
        }

    }
}
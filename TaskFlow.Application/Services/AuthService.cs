using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Entities;

namespace TaskFlow.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<User> userManager,
            IConfiguration configuration,
            IMapper mapper)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<ResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return ResponseDto<AuthResponseDto>.FailureResponse(
                        "User with this email already exists",
                        new List<string> { "Email already registered" });
                }

                var existingUsername = await _userManager.FindByNameAsync(registerDto.UserName);
                if (existingUsername != null)
                {
                    return ResponseDto<AuthResponseDto>.FailureResponse(
                        "Username already taken",
                        new List<string> { "Username already exists" });
                }

                // Create new user
                var user = _mapper.Map<User>(registerDto);
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ResponseDto<AuthResponseDto>.FailureResponse(
                        "Registration failed",
                        errors);
                }

                // Generate JWT token
                var token = await GenerateJwtToken(user);

                var authResponse = new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60"))
                };

                return ResponseDto<AuthResponseDto>.SuccessResponse(
                    authResponse,
                    "Registration successful");
            }
            catch (Exception ex)
            {
                return ResponseDto<AuthResponseDto>.FailureResponse(
                    "An error occurred during registration",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    return ResponseDto<AuthResponseDto>.FailureResponse(
                        "Invalid credentials",
                        new List<string> { "Email or password is incorrect" });
                }

                if (!user.IsActive)
                {
                    return ResponseDto<AuthResponseDto>.FailureResponse(
                        "Account is inactive",
                        new List<string> { "Please contact administrator" });
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

                if (!isPasswordValid)
                {
                    return ResponseDto<AuthResponseDto>.FailureResponse(
                        "Invalid credentials",
                        new List<string> { "Email or password is incorrect" });
                }

                // Generate JWT token
                var token = await GenerateJwtToken(user);

                var authResponse = new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60"))
                };

                return ResponseDto<AuthResponseDto>.SuccessResponse(
                    authResponse,
                    "Login successful");
            }
            catch (Exception ex)
            {
                return ResponseDto<AuthResponseDto>.FailureResponse(
                    "An error occurred during login",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ResponseDto<UserDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return ResponseDto<UserDto>.FailureResponse(
                        "User not found",
                        new List<string> { "No user found with the provided ID" });
                }

                var userDto = _mapper.Map<UserDto>(user);

                return ResponseDto<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ResponseDto<UserDto>.FailureResponse(
                    "An error occurred while retrieving user",
                    new List<string> { ex.Message });
            }
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
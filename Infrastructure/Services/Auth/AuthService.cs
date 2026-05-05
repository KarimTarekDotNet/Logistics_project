using Application.DTOs.Auth;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Auth;
using AutoMapper;
using Domain.Entities.Users;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper mapper;
        private readonly IUnitOfWork work;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IPhoneOtpService _phoneOtpService;
        private readonly IRefreshTokenService _refreshTokenSerivce;
        public AuthService(IConfiguration configuration, UserManager<ApplicationUser> userManager, IMapper mapper,
            IUnitOfWork work, IEmailVerificationService emailVerificationService, IPhoneOtpService phoneOtpService, IRefreshTokenService refreshTokenSerivce)
        {
            _configuration = configuration;
            _userManager = userManager;
            this.mapper = mapper;
            this.work = work;
            _emailVerificationService = emailVerificationService;
            _phoneOtpService = phoneOtpService;
            _refreshTokenSerivce = refreshTokenSerivce;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Identity) ??
                    await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.Identity) ??
                    await _userManager.FindByNameAsync(request.Identity);

            if (user == null)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid email, username, phone number, or password.",
                    Expiration = DateTime.UtcNow,
                    AccessToken = string.Empty
                };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid email, username, phone number, or password.",
                    Expiration = DateTime.UtcNow,
                    AccessToken = string.Empty
                };
            }

            if (!user.EmailConfirmed)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Please confirm your email before logging in.",
                    Expiration = DateTime.UtcNow,
                    AccessToken = string.Empty
                };
            }

            var dto = mapper.Map<AuthResponse>(user);

            var refresh = await _refreshTokenSerivce.GenerateAsync(user.Id, ipAddress);

            await work.SaveChangesAsync();

            dto.IsAuthenticated = true;
            dto.Message = "Login successful.";
            dto.AccessToken = await GenerateJwtToken(user);
            dto.Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!));
            dto.RefreshToken = refresh.RawToken;
            return dto;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(request.Email) ??
                    await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber) ??
                    await _userManager.FindByNameAsync(request.UserName);

                if (userExists != null)
                {
                    return new AuthResponse
                    {
                        IsAuthenticated = false,
                        Message = "Email, username, or phone number already exists.",
                        Expiration = DateTime.UtcNow,
                        AccessToken = string.Empty
                    };
                }

                var user = mapper.Map<ApplicationUser>(request);


                user.PhoneNumber = request.CountryCode + request.PhoneNumber;
                user.EmailConfirmed = false;
                user.PhoneNumberConfirmed = false;

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return new AuthResponse
                    {
                        IsAuthenticated = false,
                        Message = string.Join(" | ", result.Errors.Select(e => e.Description)),
                        Expiration = DateTime.UtcNow,
                        AccessToken = string.Empty
                    };
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user, Role.User.ToString());

                if (!addToRoleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return new AuthResponse
                    {
                        IsAuthenticated = false,
                        Message = string.Join(" | ", addToRoleResult.Errors.Select(e => e.Description))
                    };
                }
                await _emailVerificationService.SendEmailConfirmationAsync(user.Id);
                await _phoneOtpService.SendOtpAsync(request.CountryCode + request.PhoneNumber);
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Account created successfully. Please check your email to confirm your account before logging in.",
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                };
            }
            catch (Exception)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "An error occurred during registration."
                };
            }
        }

        public async Task<AuthResponse> ConfirmPhoneAsync(ConfirmPhoneRequest request)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);

            if (user == null)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid phone number."
                };
            }

            var isValid = await _phoneOtpService.VerifyOtpAsync(request.PhoneNumber, request.Code);

            if (!isValid)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid or expired OTP code."
                };
            }

            user.PhoneNumberConfirmed = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = string.Join(" | ", result.Errors.Select(e => e.Description))
                };
            }

            return new AuthResponse
            {
                IsAuthenticated = true,
                Message = "Phone number confirmed successfully.",
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName
            };
        }


        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(userClaims);
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ExpiryMinutes"]!)
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, string? ipAddress)
        {
            var oldToken = await _refreshTokenSerivce.GetByRawTokenAsync(request.RefreshToken);
            if (oldToken == null || !oldToken.IsActive)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid refresh token.",
                    Expiration = DateTime.UtcNow,
                    AccessToken = string.Empty
                };
            }
            var token = await _refreshTokenSerivce.RotateAsync(oldToken,
            oldToken.ApplicationUserId, ipAddress);

            var accessToken = await GenerateJwtToken(oldToken.ApplicationUser);
            await work.SaveChangesAsync();
            return new AuthResponse
            {
                IsAuthenticated = true,
                Message = "Token refreshed successfully.",
                Id = oldToken.ApplicationUserId,
                Email = oldToken.ApplicationUser.Email,
                UserName = oldToken.ApplicationUser.UserName,
                AccessToken = accessToken,
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                RefreshToken = token.RawToken
            };
        }

        public async Task<bool> LogoutAsync(RefreshTokenRequest request, string? ipAddress)
        {
            var token = await _refreshTokenSerivce.GetByRawTokenAsync(request.RefreshToken);

            if (token == null || !token.IsActive)
                return false;

            var revoked = await _refreshTokenSerivce.RevokeAsync(token.ApplicationUserId, request.RefreshToken, ipAddress);

            await work.SaveChangesAsync();

            return revoked;
        }

        public async Task<bool> LogoutAllAsync(string userId, string? ipAddress)
        {
            var tokens = await _refreshTokenSerivce.GetByListTokenUserIdAsync(userId);
            if (!tokens!.Any())
                return false;

            foreach (var item in tokens!)
            {
                item.RevokedAt = DateTimeOffset.UtcNow;
                item.RevokedByIp = ipAddress;
            }

            await work.SaveChangesAsync();
            return true;
        }
    }
}
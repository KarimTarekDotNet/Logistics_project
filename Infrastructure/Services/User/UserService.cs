using Application.DTOs.Shipments.User;
using Application.DTOs.User;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.User;
using AutoMapper;
using Domain.Entities.Users;
using Domain.Exceptions;
using Infrastructure.Data.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.User
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IPhoneOtpService _phoneOtpService;

        public UserService(UserManager<ApplicationUser> userManager, IMapper mapper,
        IEmailVerificationService emailVerificationService, IPhoneOtpService phoneOtpService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _mapper = mapper;
            _emailVerificationService = emailVerificationService;
            _phoneOtpService = phoneOtpService;
            _context = context;
        }

        public async Task<ProfileResponse> GetProfileAsync(string userId)
        {
            var user = await _context.ApplicationUsers
            .Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new BusinessRuleException("User not found");
            }
            var dto = _mapper.Map<ProfileResponse>(user);
            dto.Name = $"{user.FirstName} {user.LastName}".Trim();
            var customer = user.CustomerProfile;
            if (customer != null)
                dto.Customer = _mapper.Map<CustomerResponse>(customer);
            return dto;
        }

        public async Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BusinessRuleException("User not found.");

            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessRuleException(errors);
            }

            return true;
        }

        public async Task<ProfileUpdateResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BusinessRuleException("User not found");

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var existingUser = await _userManager.FindByNameAsync(request.Username);

                if (existingUser != null && existingUser.Id != user.Id)
                    throw new BusinessRuleException("Profile update request could not be completed.");

                user.UserName = request.Username;
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var existingEmailUser = await _userManager.FindByEmailAsync(request.Email);

                if (existingEmailUser != null && existingEmailUser.Id != user.Id)
                    throw new BusinessRuleException("Profile update request could not be completed.");

                user.PendingEmail = request.Email;
                await _userManager.UpdateAsync(user);

                await _emailVerificationService.SendChangeEmailConfirmationAsync(user.Id, user.PendingEmail);

                return new ProfileUpdateResponse
                {
                    IsEmailVerificationSent = true,
                    UpdatedProfile = _mapper.Map<ProfileResponse>(user),
                    message = "Email change requested. Please verify your new email to complete the update."
                };
            }

            if(!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
            {
                user.PendingPhoneNumber = request.PhoneNumber;
                await _userManager.UpdateAsync(user);
                await _phoneOtpService.SendOtpAsync(request.PhoneNumber);
                return new ProfileUpdateResponse
                {
                    IsPhoneVerificationSent = true,
                    UpdatedProfile = _mapper.Map<ProfileResponse>(user),
                    message = "Phone number change requested. Please verify your new phone number to complete the update."
                };
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BusinessRuleException("Failed to update profile");

            return new ProfileUpdateResponse
            {
                UpdatedProfile = _mapper.Map<ProfileResponse>(user),
                message = "Profile updated successfully."
            };
        }

        public async Task<ProfileUpdateResponse> ConfirmPendingEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new BusinessRuleException("User not found");

            if (string.IsNullOrWhiteSpace(user.PendingEmail))
                throw new BusinessRuleException("No pending email change request.");

            var result = await _userManager.ChangeEmailAsync(user, user.PendingEmail, token);

            if (!result.Succeeded)
                throw new BusinessRuleException("Invalid or expired email change token.");

            user.PendingEmail = null;

            var Emailresult = await _userManager.UpdateAsync(user);
            if (!Emailresult.Succeeded)
                throw new BusinessRuleException("Failed to confirm email change.");

            return new ProfileUpdateResponse
            {
                UpdatedProfile = _mapper.Map<ProfileResponse>(user),
                message = "Email updated successfully."
            };
        }

        public async Task<ProfileUpdateResponse> VerifyPendingPhoneAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new BusinessRuleException("User not found");

            if (string.IsNullOrWhiteSpace(user.PendingPhoneNumber))
                throw new BusinessRuleException("No pending phone change request.");

            var isValid = await _phoneOtpService.VerifyOtpAsync(user.PendingPhoneNumber, code);

            if (!isValid)
                throw new BusinessRuleException("Invalid phone verification code.");

            user.PhoneNumber = user.PendingPhoneNumber;
            user.PhoneNumberConfirmed = true;
            user.PendingPhoneNumber = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BusinessRuleException("Failed to Confirm Phone");

            return new ProfileUpdateResponse
            {
                UpdatedProfile = _mapper.Map<ProfileResponse>(user),
                message = "Phone number updated successfully."
            };
        }
    }
}

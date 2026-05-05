using Application.DTOs.User;

namespace Application.Interfaces.Services.User
{
    public interface IUserService
    {
        Task<ProfileResponse> GetProfileAsync(string userId);
        Task<ProfileUpdateResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<bool> UpdatePasswordAsync(string userId, UpdatePasswordRequest request);
        Task<ProfileUpdateResponse> ConfirmPendingEmailAsync(string userId, string token);
        Task<ProfileUpdateResponse> VerifyPendingPhoneAsync(string userId, string code);
    }
}
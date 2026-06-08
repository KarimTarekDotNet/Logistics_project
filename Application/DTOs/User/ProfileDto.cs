using Application.DTOs.Shipments.User;

namespace Application.DTOs.User
{
    public record ProfileResponse
    {
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool PhoneNumberConfirmed { get; set; }
        public CustomerResponse? Customer { get; set; }
    }
    public record ProfileUpdateResponse
    {
        public bool IsEmailVerificationSent { get; set; }
        public bool IsPhoneVerificationSent { get; set; }
        public string? message { get; set; }
        public ProfileResponse? UpdatedProfile { get; set; }
    }

    public record UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public record UpdatePasswordRequest
    {
        public string CurrentPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
        public string ConfirmPassword { get; init; } = string.Empty;
    }

    public record VerifyPendingPhoneRequest
    {
        public string Code { get; set; } = null!;
    }
}

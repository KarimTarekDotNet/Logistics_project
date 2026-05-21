using System.Text.Json.Serialization;

namespace Application.DTOs.Auth
{
    public record LoginRequest
    {
        public string Identity { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public record RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty; // +20
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; init; } = string.Empty;
    }
    public class ConfirmPhoneRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public record ResendEmailConfirmationRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public record AuthResponse
    {
        public bool IsAuthenticated { get; set; } = false;
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public record RefreshTokenRequest
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}

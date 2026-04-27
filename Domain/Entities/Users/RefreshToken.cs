using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Users
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string HashedToken { get; set; } = null!;
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
        public string? ReplacedByHashedToken { get; set; }
        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }

        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;

        [NotMapped]
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

        [NotMapped]
        public bool IsRevoked => RevokedAt != null;
    }
}

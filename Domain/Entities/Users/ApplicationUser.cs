using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PendingEmail { get; set; }
        public string? PendingPhoneNumber { get; set; }
        public Customer? CustomerProfile { get; set; }
    }
}

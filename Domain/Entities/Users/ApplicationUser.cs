using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Customer? CustomerProfile { get; set; }
    }
}

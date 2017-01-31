namespace Unosquare.Swan.AspNetCore.Models
{
    using System.Security.Claims;

    public class ApplicationUser : ClaimsIdentity
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}

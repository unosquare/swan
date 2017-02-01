namespace Unosquare.Swan.AspNetCore.Models
{
    using System.Security.Claims;

    /// <summary>
    /// Represents an User
    /// </summary>
    /// <seealso cref="System.Security.Claims.ClaimsIdentity" />
    public class ApplicationUser : ClaimsIdentity
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [phone number confirmed].
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the password hash.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [two factor enabled].
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
    }
}
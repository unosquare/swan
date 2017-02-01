namespace Unosquare.Swan.AspNetCore.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a common Log Entry to use with the EF Logger
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// The maximum exception length
        /// </summary>
        public const int MaximumExceptionLength = 2000;
        /// <summary>
        /// The maximum message length
        /// </summary>
        public const int MaximumMessageLength = 4000;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the thread.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Thread { get; set; }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Level { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Logger { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [Required]
        [StringLength(MaximumMessageLength)]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        [StringLength(MaximumExceptionLength)]
        public string Exception { get; set; }

        /// <summary>
        /// Gets or sets the host address.
        /// </summary>
        [StringLength(20)]
        public string HostAddress { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [StringLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the browser.
        /// </summary>
        [StringLength(200)]
        public string Browser { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        [StringLength(100)]
        public string Url { get; set; }
    }
}
namespace Unosquare.Swan.AspNetCore.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class LogEntry
    {
        public const int MaximumExceptionLength = 2000;
        public const int MaximumMessageLength = 4000;

        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Required]
        [StringLength(255)]
        public string Thread { get; set; }

        [Required]
        [StringLength(50)]
        public string Level { get; set; }

        [Required]
        [StringLength(255)]
        public string Logger { get; set; }

        [Required]
        [StringLength(MaximumMessageLength)]
        public string Message { get; set; }

        [StringLength(MaximumExceptionLength)]
        public string Exception { get; set; }

        [StringLength(20)]
        public string HostAddress { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        [StringLength(200)]
        public string Browser { get; set; }

        [StringLength(100)]
        public string Url { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Unosquare.Swan.Test.Mocks
{
    public class AppSettingMock
    {
        /// <summary>
        /// Gets or sets the web server port.
        /// </summary>
        [Display(Name = "Web Server Port", Description = "The port on which the web server listens for requests", GroupName = "Administration")]
        [DefaultValue(9898)]
        public int WebServerPort { get; set; } = 9898;

        /// <summary>
        /// Gets or sets the web server hostname.
        /// </summary>
        [Display(Name = "Web Server Host Name",
            Description = "The hostname to which the web server binds, it can be localhost, a specific IP address or a '+' sign to bind to all IP addresses",
            GroupName = "Administration")]
        [DefaultValue("localhost")]
        public string WebServerHostname { get; set; } = "localhost";

        [Display(Name = "Background Image", Description = "Background image set for standby view", GroupName = "Administration")]
        public string[] BackgroundImage { get; set; } = { "https://source.unsplash.com/category/nature" };
    }
}

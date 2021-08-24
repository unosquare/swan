using System.Linq;
using System.Text;

namespace Swan
{
    /// <summary>
    /// Contains useful constants and definitions.
    /// </summary>
    public static partial class Definitions
    {
        /// <summary>
        /// Initializes the <see cref="Definitions"/> class.
        /// </summary>
        static Definitions()
        {
            CurrentAnsiEncoding = Encoding.GetEncoding(default(int));
            var windowsEncoding = Encoding.GetEncodings().FirstOrDefault(c => c.CodePage == 1252)?.GetEncoding();
            Windows1252Encoding = windowsEncoding ?? CurrentAnsiEncoding;
        }

        /// <summary>
        /// The MS Windows codepage 1252 encoding used in some legacy scenarios
        /// such as default CSV text encoding from Excel.
        /// </summary>
        public static Encoding Windows1252Encoding { get; }

        /// <summary>
        /// The encoding associated with the default ANSI code page in the operating 
        /// system's regional and language settings.
        /// </summary>
        public static Encoding CurrentAnsiEncoding { get; }
    }
}
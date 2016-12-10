namespace Unosquare.Swan
{
    using System.Text;

    /// <summary>
    /// Contains useful constants
    /// </summary>
    static public partial class Constants
    {

        static Constants()
        {

        }

        /// <summary>
        /// The MS Windows codepage 1252 encoding used in some legacy scenarios
        /// such as default CSV text encoding from Excel
        /// </summary>
        static public readonly Encoding Windows1252Encoding = Encoding.GetEncoding(1252);

    }
}

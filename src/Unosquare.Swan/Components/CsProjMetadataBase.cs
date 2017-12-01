namespace Unosquare.Swan.Components
{
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a CsProj metadata abstract class
    /// to use with <c>CsProjFile</c> parser.
    /// </summary>
    public abstract class CsProjMetadataBase
    {
        private XDocument _xmlDocument;

        /// <summary>
        /// Gets the package identifier.
        /// </summary>
        /// <value>
        /// The package identifier.
        /// </value>
        public string PackageId => FindElement(nameof(PackageId))?.Value;

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        public string AssemblyName => FindElement(nameof(AssemblyName))?.Value;

        /// <summary>
        /// Gets the target frameworks.
        /// </summary>
        /// <value>
        /// The target frameworks.
        /// </value>
        public string TargetFrameworks => FindElement(nameof(TargetFrameworks))?.Value;

        /// <summary>
        /// Gets the target framework.
        /// </summary>
        /// <value>
        /// The target framework.
        /// </value>
        public string TargetFramework => FindElement(nameof(TargetFramework))?.Value;

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version => FindElement(nameof(Version))?.Value;

        /// <summary>
        /// Parses the cs proj tags.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public abstract void ParseCsProjTags(ref string[] args);

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="xmlDocument">The XML document.</param>
        public void SetData(XDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        /// <summary>
        /// Finds the element.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>A XElement.</returns>
        protected XElement FindElement(string elementName)
        {
            return _xmlDocument.Descendants(elementName).FirstOrDefault();
        }
    }
}

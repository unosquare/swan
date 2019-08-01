namespace Swan.Components
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a CsProjFile (and FsProjFile) parser.
    /// </summary>
    /// <remarks>
    /// Based on https://github.com/maartenba/dotnetcli-init.
    /// </remarks>
    /// <typeparam name="T">The type of <c>CsProjMetadataBase</c>.</typeparam>
    /// <seealso cref="System.IDisposable" />
    public class CsProjFile<T>
        : IDisposable
        where T : CsProjMetadataBase
    {
        private readonly Stream _stream;
        private readonly bool _leaveOpen;
        private readonly XDocument _xmlDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsProjFile{T}"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public CsProjFile(string filename = null)
            : this(OpenFile(filename))
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsProjFile{T}"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="leaveOpen">if set to <c>true</c> [leave open].</param>
        /// <exception cref="ArgumentException">Project file is not of the new .csproj type.</exception>
        public CsProjFile(Stream stream, bool leaveOpen = false)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;

            _xmlDocument = XDocument.Load(stream);

            var projectElement = _xmlDocument.Descendants("Project").FirstOrDefault();
            var sdkAttribute = projectElement?.Attribute("Sdk");
            var sdk = sdkAttribute?.Value;
            if (sdk != "Microsoft.NET.Sdk" && sdk != "Microsoft.NET.Sdk.Web")
            {
                throw new ArgumentException("Project file is not of the new .csproj type.");
            }

            Metadata = Activator.CreateInstance<T>();
            Metadata.SetData(_xmlDocument);
        }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>
        /// The nu get metadata.
        /// </value>
        public T Metadata { get; }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            _stream.SetLength(0);
            _stream.Position = 0;

            _xmlDocument.Save(_stream);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_leaveOpen)
            {
                _stream?.Dispose();
            }
        }

        private static FileStream OpenFile(string filename)
        {
            if (filename == null)
            {
                filename = Directory
                    .EnumerateFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            }

            if (filename == null)
            {
                filename = Directory
                    .EnumerateFiles(Directory.GetCurrentDirectory(), "*.fsproj", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            return File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
    }
}
using System;
using System.IO;

namespace Unosquare.Swan.Test.Mocks
{
    internal class Helper
    {
        /// <summary>
        /// Creates the temporary binary file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="sizeInMb">The size in mb.</param>
        internal static void CreateTempBinaryFile(string fileName, int sizeInMb)
        {
            // Note: block size must be a factor of 1MB to avoid rounding errors :)
            const int blockSize = 1024 * 8;
            const int blocksPerMb = (1024 * 1024) / blockSize;
            var data = new byte[blockSize];

            var rng = new Random();
            using (var stream = File.OpenWrite(fileName))
            {
                // There 
                for (var i = 0; i < sizeInMb * blocksPerMb; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
    }
}

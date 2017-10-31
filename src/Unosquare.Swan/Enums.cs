namespace Unosquare.Swan
{
    /// <summary>
    /// Enumeration of Operating Systems
    /// </summary>
    public enum OperatingSystem
    {
        /// <summary>
        /// Unknown OS
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Windows
        /// </summary>
        Windows,
        
        /// <summary>
        /// UNIX/Linux
        /// </summary>
        Unix,
        
        /// <summary>
        /// macOS (OSX)
        /// </summary>
        Osx
    }

    /// <summary>
    /// Enumerates the different Application Worker States
    /// </summary>
    public enum AppWorkerState
    {
        /// <summary>
        /// The stopped
        /// </summary>
        Stopped,
        
        /// <summary>
        /// The running
        /// </summary>
        Running,
    }

    /// <summary>
    /// Enumerates the possible causes of the DataReceived event occurring.
    /// </summary>
    public enum ConnectionDataReceivedTrigger
    {
        /// <summary>
        /// The trigger was a forceful flush of the buffer
        /// </summary>
        Flush,
        
        /// <summary>
        /// The new line sequence bytes were received
        /// </summary>
        NewLineSequenceEncountered,
        
        /// <summary>
        /// The buffer was full
        /// </summary>
        BufferFull,
        
        /// <summary>
        /// The block size reached
        /// </summary>
        BlockSizeReached
    }

    /// <summary>
    /// Defines Endianness, big or little
    /// </summary>
    public enum Endianness
    {
        /// <summary>
        /// In big endian, you store the most significant byte in the smallest address. 
        /// </summary>
        Big,

        /// <summary>
        /// In little endian, you store the least significant byte in the smallest address.
        /// </summary>
        Little,
    }
}

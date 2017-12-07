namespace Unosquare.Swan
{
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
}

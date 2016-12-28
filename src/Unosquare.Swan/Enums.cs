namespace Unosquare.Swan
{
    using System;

    /// <summary>
    /// Defines a set of bitwise standard terminal writers
    /// </summary>
    [Flags]
    public enum TerminalWriters
    {
        /// <summary>
        /// Prevents output
        /// </summary>
        None = 0,
        /// <summary>
        /// Writes to the Console.Out
        /// </summary>
        StandardOutput = 1,
        /// <summary>
        /// Writes to the Console.Error
        /// </summary>
        StandardError = 2,
        /// <summary>
        /// Writes to the System.Diagnostics.Debug
        /// </summary>
        Diagnostics = 4
    }

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
        /// Mac OSX
        /// </summary>
        Osx
    }

    /// <summary>
    /// Defines Endiness, big or little
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

    /// <summary>
    /// Defines the bitwise flags to determine
    /// which types of messages get printed on the current console
    /// </summary>
    [Flags]
    public enum LogMessageType
    {
        /// <summary>
        /// The none message type
        /// </summary>
        None = 0,

        /// <summary>
        /// The information message type
        /// </summary>
        Info = 1,

        /// <summary>
        /// The debug message type
        /// </summary>
        Debug = 2,

        /// <summary>
        /// The trace message type
        /// </summary>
        Trace = 4,

        /// <summary>
        /// The error message type
        /// </summary>
        Error = 8,

        /// <summary>
        /// The warning message type
        /// </summary>
        Warning = 16,
    }

    /// <summary>
    /// Enumerates the different DNS record types
    /// </summary>
    public enum DnsRecordType
    {
        /// <summary>
        /// A records
        /// </summary> 
        A = 1,
        /// <summary>
        /// Nameserver records
        /// </summary> 
        NS = 2,
        /// <summary>
        /// CNAME records
        /// </summary> 
        CNAME = 5,
        /// <summary>
        /// SOA records
        /// </summary> 
        SOA = 6,
        /// <summary>
        /// WKS records
        /// </summary> 
        WKS = 11,
        /// <summary>
        /// PTR records
        /// </summary> 
        PTR = 12,
        /// <summary>
        /// MX records
        /// </summary> 
        MX = 15,
        /// <summary>
        /// TXT records
        /// </summary> 
        TXT = 16,
        /// <summary>
        /// A records fot IPv6
        /// </summary> 
        AAAA = 28,
        /// <summary>
        /// SRV records
        /// </summary> 
        SRV = 33,
        /// <summary>
        /// ANY records
        /// </summary> 
        ANY = 255,
    }

    /// <summary>
    /// Enumerates the different DNS record classes
    /// </summary>
    public enum DnsRecordClass
    {
        /// <summary>
        /// IN records
        /// </summary> 
        IN = 1,
        /// <summary>
        /// ANY records
        /// </summary> 
        ANY = 255,
    }

    /// <summary>
    /// Enumerates the different DNS operation codes
    /// </summary>
    public enum DnsOperationCode
    {
        /// <summary>
        /// Query operation
        /// </summary> 
        Query = 0,
        /// <summary>
        /// IQuery operation
        /// </summary> 
        IQuery,
        /// <summary>
        /// Status operation
        /// </summary> 
        Status,
        // Reserved = 3
        /// <summary>
        /// Notify operation
        /// </summary> 
        Notify = 4,
        /// <summary>
        /// Update operation
        /// </summary> 
        Update,
    }

    /// <summary>
    /// Enumerates the different DNS query response codes
    /// </summary>
    public enum DnsResponseCode
    {
        /// <summary>
        /// No error
        /// </summary> 
        NoError = 0,
        /// <summary>
        /// No error
        /// </summary> 
        FormatError,
        /// <summary>
        /// Format error
        /// </summary> 
        ServerFailure,
        /// <summary>
        /// Server failure error
        /// </summary> 
        NameError,
        /// <summary>
        /// Name error
        /// </summary> 
        NotImplemented,
        /// <summary>
        /// Not implemented error
        /// </summary> 
        Refused,
        /// <summary>
        /// Refused error
        /// </summary> 
        YXDomain,
        /// <summary>
        /// YXRR error
        /// </summary> 
        YXRRSet,
        /// <summary>
        /// NXRR Set error
        /// </summary> 
        NXRRSet,
        /// <summary>
        /// Not authorized error
        /// </summary> 
        NotAuth,
        /// <summary>
        /// Not zone error
        /// </summary> 
        NotZone,
    }
}

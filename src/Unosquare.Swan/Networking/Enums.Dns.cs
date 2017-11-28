// ReSharper disable InconsistentNaming
namespace Unosquare.Swan.Networking
{
    #region DNS

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

    #endregion

}

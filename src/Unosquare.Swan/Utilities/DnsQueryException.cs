namespace Unosquare.Swan.Utilities
{
    using System;
    using static DnsClient;

    /// <summary>
    /// An exception thrown when the DNS query fails.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DnsQueryException : Exception
    {
        #region Constructors

        internal DnsQueryException() { }
        internal DnsQueryException(string message) : base(message) { }
        internal DnsQueryException(string message, Exception e) : base(message, e) { }

        internal DnsQueryException(IDnsResponse response) : this(response, Format(response)) { }

        internal DnsQueryException(IDnsResponse response, Exception e)
            : base(Format(response), e)
        {
            Response = response;
        }

        internal DnsQueryException(IDnsResponse response, string message)
            : base(message)
        {
            Response = response;
        }

        #endregion

        #region Properties and Methods

        private static string Format(IDnsResponse response)
        {
            return string.Format("Invalid response received with code {0}", response.ResponseCode);
        }

        internal IDnsResponse Response
        {
            get;
            private set;
        }

        #endregion
    }
}

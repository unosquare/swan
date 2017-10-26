namespace Unosquare.Swan.Exceptions
{
    using System;
    using Networking;

    /// <summary>
    /// An exception thrown when the DNS query fails.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DnsQueryException : Exception
    {
        internal DnsQueryException(string message)
            : base(message)
        {
        }

        internal DnsQueryException(string message, Exception e)
            : base(message, e)
        {
        }

        internal DnsQueryException(DnsClient.IDnsResponse response)
            : this(response, Format(response))
        {
        }

        internal DnsQueryException(DnsClient.IDnsResponse response, string message)
            : base(message)
        {
            Response = response;
        }

        internal DnsClient.IDnsResponse Response { get; }

        private static string Format(DnsClient.IDnsResponse response)
        {
            return $"Invalid response received with code {response.ResponseCode}";
        }
    }
}
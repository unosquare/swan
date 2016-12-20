namespace Unosquare.Swan.Utilities
{
    using System;

    partial class DnsClient
    {

        public class DnsResponseException : Exception
        {
            private static string Format(IDnsResponse response)
            {
                return string.Format("Invalid response received with code {0}", response.ResponseCode);
            }

            public DnsResponseException() { }
            public DnsResponseException(string message) : base(message) { }
            public DnsResponseException(string message, Exception e) : base(message, e) { }

            public DnsResponseException(IDnsResponse response) : this(response, Format(response)) { }

            public DnsResponseException(IDnsResponse response, Exception e)
                : base(Format(response), e)
            {
                Response = response;
            }

            public DnsResponseException(IDnsResponse response, string message)
                : base(message)
            {
                Response = response;
            }

            public IDnsResponse Response
            {
                get;
                private set;
            }
        }

    }
}

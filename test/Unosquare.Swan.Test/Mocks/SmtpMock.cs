namespace Unosquare.Swan.Test.Mocks
{
    using Attributes;
    using System.Collections.Generic;

    public class From
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("args")]
        public bool Args { get; set; }
    }

    public class To
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("args")]
        public bool Args { get; set; }
    }

    public class Envelope
    {
        [JsonProperty("from")]
        public From From { get; set; }

        [JsonProperty("to")]
        public List<To> To { get; set; }
        
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("remoteAddress")]
        public string RemoteAddress { get; set; }
    }

    public class SmtpMock
    {
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty("priority")]
        public string Priority { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("read")]
        public bool Read { get; set; }

        [JsonProperty("envelope")]
        public Envelope Envelope { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}

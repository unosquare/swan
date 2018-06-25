namespace Unosquare.Swan.Test.Mocks
{
    using Attributes;
    using System.Collections.Generic;

    public class SmtpMock
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("secure")]
        public bool Secure { get; set; }
        [JsonProperty("localAddress")]
        public string LocalAddress { get; set; }
        [JsonProperty("localPort")]
        public int LocalPort { get; set; }
        [JsonProperty("remoteAddress")]
        public string RemoteAddress { get; set; }
        [JsonProperty("remotePort")]
        public int RemotePort { get; set; }
        [JsonProperty("clientHostname")]
        public string ClientHostname { get; set; }
        [JsonProperty("openingCommand")]
        public string OpeningCommand { get; set; }
        [JsonProperty("hostNameAppearsAs")]
        public string HostNameAppearsAs { get; set; }
        [JsonProperty("transmissionType")]
        public string TransmissionType { get; set; }
        [JsonProperty("tlsOptions")]
        public bool TlsOptions { get; set; }
        [JsonProperty("transaction")]
        public int Transaction { get; set; }
        [JsonProperty("user")]
        public int User { get; set; }
        [JsonProperty("envelope")]
        public Envelope Envelope { get; set; }
    }
    
    public class GenericMail
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("args")]
        public bool Args { get; set; }
    }

    public class Envelope
    {
        [JsonProperty("mailFrom")]
        public GenericMail MailFrom { get; set; }
        [JsonProperty("rcptTo")]
        public List<GenericMail> RcptTo { get; set; }
    }
}

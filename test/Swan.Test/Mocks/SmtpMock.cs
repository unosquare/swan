using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Swan.Test.Mocks
{
    public class SmtpMock
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("secure")]
        public bool Secure { get; set; }
        [JsonPropertyName("localAddress")]
        public string LocalAddress { get; set; }
        [JsonPropertyName("localPort")]
        public int LocalPort { get; set; }
        [JsonPropertyName("remoteAddress")]
        public string RemoteAddress { get; set; }
        [JsonPropertyName("remotePort")]
        public int RemotePort { get; set; }
        [JsonPropertyName("clientHostname")]
        public string ClientHostname { get; set; }
        [JsonPropertyName("openingCommand")]
        public string OpeningCommand { get; set; }
        [JsonPropertyName("hostNameAppearsAs")]
        public string HostNameAppearsAs { get; set; }
        [JsonPropertyName("transmissionType")]
        public string TransmissionType { get; set; }
        [JsonPropertyName("tlsOptions")]
        public bool TlsOptions { get; set; }
        [JsonPropertyName("transaction")]
        public int Transaction { get; set; }
        [JsonPropertyName("user")]
        public int User { get; set; }
        [JsonPropertyName("envelope")]
        public Envelope Envelope { get; set; }
    }

    public class GenericMail
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("args")]
        public bool Args { get; set; }
    }

    public class Envelope
    {
        [JsonPropertyName("mailFrom")]
        public GenericMail MailFrom { get; set; }
        [JsonPropertyName("rcptTo")]
        public List<GenericMail> RcptTo { get; set; }
    }
}
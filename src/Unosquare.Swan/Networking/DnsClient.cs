namespace Unosquare.Swan.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Exceptions;

    partial class DnsClient
    {
        private readonly IPEndPoint dns;
        private readonly IDnsRequestResolver resolver;

        public DnsClient(IPEndPoint dns, IDnsRequestResolver resolver = null)
        {
            this.dns = dns;
            this.resolver = resolver ?? new DnsUdpRequestResolver(new DnsTcpRequestResolver());
        }

        public DnsClient(IPAddress ip, int port = Definitions.DnsDefaultPort, IDnsRequestResolver resolver = null) 
            : this(new IPEndPoint(ip, port), resolver)
        {
        }

        public DnsClient(string ip, int port = Definitions.DnsDefaultPort, IDnsRequestResolver resolver = null) 
            : this(IPAddress.Parse(ip), port, resolver)
        {
        }

        public DnsClientRequest FromArray(byte[] message)
        {
            var request = DnsRequest.FromArray(message);
            return new DnsClientRequest(dns, request, resolver);
        }

        public DnsClientRequest Create(IDnsRequest request = null)
        {
            return new DnsClientRequest(dns, request, resolver);
        }

        public IList<IPAddress> Lookup(string domain, DnsRecordType type = DnsRecordType.A)
        {
            if (type != DnsRecordType.A && type != DnsRecordType.AAAA)
            {
                throw new ArgumentException("Invalid record type " + type);
            }

            var response = Resolve(domain, type);
            var ips = response.AnswerRecords
                .Where(r => r.Type == type)
                .Cast<DnsIPAddressResourceRecord>()
                .Select(r => r.IPAddress)
                .ToList();

            if (ips.Count == 0)
            {
                throw new DnsQueryException(response, "No matching records");
            }

            return ips;
        }

        public string Reverse(string ip) => Reverse(IPAddress.Parse(ip));

        public string Reverse(IPAddress ip)
        {
            var response = Resolve(DnsDomain.PointerName(ip), DnsRecordType.PTR);
            var ptr = response.AnswerRecords.FirstOrDefault(r => r.Type == DnsRecordType.PTR);

            if (ptr == null)
            {
                throw new DnsQueryException(response, "No matching records");
            }

            return ((DnsPointerResourceRecord)ptr).PointerDomainName.ToString();
        }

        public DnsClientResponse Resolve(string domain, DnsRecordType type)
        {
            return Resolve(new DnsDomain(domain), type);
        }

        public DnsClientResponse Resolve(DnsDomain domain, DnsRecordType type)
        {
            var request = Create();
            var question = new DnsQuestion(domain, type);

            request.Questions.Add(question);
            request.OperationCode = DnsOperationCode.Query;
            request.RecursionDesired = true;

            return request.Resolve();
        }
    }
}

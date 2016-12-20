using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Unosquare.Swan.Utilities
{
    public partial class DnsClient
    {
        private static readonly Random RANDOM = new Random();

        private IPEndPoint dns;
        private IDnsRequestResolver resolver;

        public DnsClient(IPEndPoint dns, IDnsRequestResolver resolver = null)
        {
            this.dns = dns;
            this.resolver = resolver == null ? new DnsUdpRequestResolver(new DnsTcpRequestResolver()) : resolver;
        }

        public DnsClient(IPAddress ip, int port = Constants.DnsDefaultPort, IDnsRequestResolver resolver = null) :
            this(new IPEndPoint(ip, port), resolver)
        { }

        public DnsClient(string ip, int port = Constants.DnsDefaultPort, IDnsRequestResolver resolver = null) :
            this(IPAddress.Parse(ip), port, resolver)
        { }

        public DnsClientRequest FromArray(byte[] message)
        {
            DnsRequest request = DnsRequest.FromArray(message);
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

            DnsClientResponse response = Resolve(domain, type);
            IList<IPAddress> ips = response.AnswerRecords
                .Where(r => r.Type == type)
                .Cast<DnsIPAddressResourceRecord>()
                .Select(r => r.IPAddress)
                .ToList();

            if (ips.Count == 0)
            {
                throw new DnsResponseException(response, "No matching records");
            }

            return ips;
        }

        public string Reverse(string ip)
        {
            return Reverse(IPAddress.Parse(ip));
        }

        public string Reverse(IPAddress ip)
        {
            DnsClientResponse response = Resolve(DnsDomain.PointerName(ip), DnsRecordType.PTR);
            IDnsResourceRecord ptr = response.AnswerRecords.FirstOrDefault(r => r.Type == DnsRecordType.PTR);

            if (ptr == null)
            {
                throw new DnsResponseException(response, "No matching records");
            }

            return ((DnsPointerResourceRecord)ptr).PointerDomainName.ToString();
        }

        public DnsClientResponse Resolve(string domain, DnsRecordType type)
        {
            return Resolve(new DnsDomain(domain), type);
        }

        public DnsClientResponse Resolve(DnsDomain domain, DnsRecordType type)
        {
            DnsClientRequest request = Create();
            DnsQuestion question = new DnsQuestion(domain, type);

            request.Questions.Add(question);
            request.OperationCode = DnsOperationCode.Query;
            request.RecursionDesired = true;

            return request.Resolve();
        }
    }

}

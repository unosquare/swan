﻿namespace Swan.Net.Dns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// DnsClient public methods.
    /// </summary>
    internal partial class DnsClient
    {
        private readonly IPEndPoint _dns;
        private readonly IDnsRequestResolver _resolver;

        public DnsClient(IPEndPoint dns, IDnsRequestResolver? resolver = null)
        {
            _dns = dns;
            _resolver = resolver ?? new DnsUdpRequestResolver(new DnsTcpRequestResolver());
        }

        public DnsClient(IPAddress ip, int port = Network.DnsDefaultPort, IDnsRequestResolver? resolver = null)
            : this(new IPEndPoint(ip, port), resolver)
        {
        }

        public DnsClientRequest Create(IDnsRequest? request = null)
            => new DnsClientRequest(_dns, request, _resolver);

        public async Task<IList<IPAddress>> Lookup(string domain, DnsRecordType type = DnsRecordType.A)
        {
            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentNullException(nameof(domain));

            if (type != DnsRecordType.A && type != DnsRecordType.AAAA)
            {
                throw new ArgumentException("Invalid record type " + type);
            }

            var response = await Resolve(domain, type).ConfigureAwait(false);
            var ips = response.AnswerRecords
                .Where(r => r.Type == type)
                .Cast<DnsIPAddressResourceRecord>()
                .Select(r => r.IPAddress)
                .ToList();

            return ips.Count == 0 ? throw new DnsQueryException(response, "No matching records") : ips;
        }

        public async Task<string> Reverse(IPAddress ip)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));

            var response = await Resolve(DnsDomain.PointerName(ip), DnsRecordType.PTR).ConfigureAwait(false);
            var ptr = response.AnswerRecords.FirstOrDefault(r => r.Type == DnsRecordType.PTR);

            return ptr == null
                ? throw new DnsQueryException(response, "No matching records")
                : ((DnsPointerResourceRecord) ptr).PointerDomainName.ToString();
        }

        public Task<DnsClientResponse> Resolve(string domain, DnsRecordType type) =>
            Resolve(new DnsDomain(domain), type);

        public Task<DnsClientResponse> Resolve(DnsDomain domain, DnsRecordType type)
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

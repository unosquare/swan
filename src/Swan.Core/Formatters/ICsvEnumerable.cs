using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swan.Formatters
{
    public interface ICsvEnumerable<out TLine> : IEnumerable<TLine>, IAsyncEnumerable<TLine>, IDisposable
    {
        TLine? Current { get; }

        bool EndOfStream { get; }

        bool TryRead(bool trimValues);

        Task<bool> TryReadAsync(bool trimValues);
    }
}

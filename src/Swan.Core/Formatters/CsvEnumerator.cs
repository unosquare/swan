using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swan.Formatters
{
    internal sealed class CsvEnumerator<TReader, TRecord> : IEnumerator<TRecord>, IAsyncEnumerator<TRecord>
        where TReader : ICsvEnumerable<TRecord>
    {
        public CsvEnumerator(ICsvEnumerable<TRecord> reader)
        {
            Reader = reader;
        }

        public TRecord Current => Reader.Current;

        object IEnumerator.Current => Reader.Current!;

        private ICsvEnumerable<TRecord> Reader { get; }

        public void Dispose() => Reader.Dispose();

        public ValueTask DisposeAsync()
        {
            Reader.Dispose();
            return ValueTask.CompletedTask;
        }

        public bool MoveNext() => !Reader.EndOfStream && Reader.TryRead(true);

        public async ValueTask<bool> MoveNextAsync() => !Reader.EndOfStream && await Reader.TryReadAsync(true);

        public void Reset() => throw new NotSupportedException("Unable to reset the state of the reader.");
    }
}

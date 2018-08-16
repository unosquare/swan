namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A simple benchmarking class.
    /// </summary>
    /// <example>
    /// The following code demonstrates how to create a simple benchmark.
    /// <code>
    /// namespace Examples.Benchmark.Simple
    /// {
    ///     using Unosquare.Swan.Components;
    /// 
    ///     public class SimpleBenchmark
    ///     {
    ///         public static void Main()
    ///         {
    ///              using (Benchmark.Start("Test"))
    ///              {
    ///                 // do some logic in here
    ///              }
    ///              
    ///             // dump results into a string
    ///             var results = Benchmark.Dump();
    ///         }
    ///     }
    ///     
    /// }
    /// </code>
    /// </example>
    public static class Benchmark
    {
        private static readonly object SyncLock = new object();
        private static readonly Dictionary<string, List<TimeSpan>> Measures = new Dictionary<string, List<TimeSpan>>();

        /// <summary>
        /// Starts measuring with the given identifier.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>A disposable object that when disposed, adds a benchmark result.</returns>
        public static IDisposable Start(string identifier) => new BenchmarkUnit(identifier);

        /// <summary>
        /// Outputs the benchmark statistics.
        /// </summary>
        /// <returns>A string containing human-readable statistics.</returns>
        public static string Dump()
        {
            var builder = new StringBuilder();

            lock (SyncLock)
            {
                foreach (var kvp in Measures)
                {
                    builder.Append($"BID: {kvp.Key,-30} | ")
                        .Append($"CNT: {kvp.Value.Count,6} | ")
                        .Append($"AVG: {kvp.Value.Average(t => t.TotalMilliseconds),8:0.000} ms. | ")
                        .Append($"MAX: {kvp.Value.Max(t => t.TotalMilliseconds),8:0.000} ms. | ")
                        .Append($"MIN: {kvp.Value.Min(t => t.TotalMilliseconds),8:0.000} ms. | ")
                        .Append(Environment.NewLine);
                }
            }
            
            return builder.ToString().TrimEnd();
        }

        /// <summary>
        /// Adds the specified result to the given identifier.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="elapsed">The elapsed.</param>
        private static void Add(string identifier, TimeSpan elapsed)
        {
            lock (SyncLock)
            {
                if (Measures.ContainsKey(identifier) == false)
                    Measures[identifier] = new List<TimeSpan>(1024 * 1024);

                Measures[identifier].Add(elapsed);
            }
        }

        /// <summary>
        /// Represents a disposable benchmark unit.
        /// </summary>
        /// <seealso cref="IDisposable" />
        private sealed class BenchmarkUnit : IDisposable
        {
            private readonly string _identifier;
            private bool _isDisposed; // To detect redundant calls
            private Stopwatch _stopwatch = new Stopwatch();

            /// <summary>
            /// Initializes a new instance of the <see cref="BenchmarkUnit" /> class.
            /// </summary>
            /// <param name="identifier">The identifier.</param>
            public BenchmarkUnit(string identifier)
            {
                _identifier = identifier;
                _stopwatch.Start();
            }

            /// <inheritdoc />
            public void Dispose() => Dispose(true);

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources.
            /// </summary>
            /// <param name="alsoManaged"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            private void Dispose(bool alsoManaged)
            {
                if (_isDisposed) return;

                if (alsoManaged)
                {
                    Add(_identifier, _stopwatch.Elapsed);
                    _stopwatch?.Stop();
                }

                _stopwatch = null;
                _isDisposed = true;
            }
        }
    }
}

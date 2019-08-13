using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Swan.Diagnostics
{
    /// <summary>
    /// A simple benchmarking class.
    /// </summary>
    /// <example>
    /// The following code demonstrates how to create a simple benchmark.
    /// <code>
    /// namespace Examples.Benchmark.Simple
    /// {
    ///     using Swan.Diagnostics;
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
    public static partial class Benchmark
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
        /// Measures the elapsed time of the given action as a TimeSpan
        /// This method uses a high precision Stopwatch if it is available.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>
        /// A  time interval that represents a specified time, where the specification is in units of ticks.
        /// </returns>
        /// <exception cref="ArgumentNullException">target.</exception>
        public static TimeSpan BenchmarkAction(Action target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var sw = Stopwatch.IsHighResolution ? new HighResolutionTimer() : new Stopwatch();

            try
            {
                sw.Start();
                target.Invoke();
            }
            catch
            {
                // swallow
            }
            finally
            {
                sw.Stop();
            }
            
            return TimeSpan.FromTicks(sw.ElapsedTicks);
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
    }
}

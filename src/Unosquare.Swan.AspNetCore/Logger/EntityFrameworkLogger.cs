namespace Unosquare.Swan.AspNetCore.Logger
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Models;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a Logger using EntityFramework
    /// Based on https://github.com/staff0rd/entityframework-logging
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TLog">The type of the log.</typeparam>
    /// <seealso cref="ILogger" />
    public class EntityFrameworkLogger<TDbContext, TLog> : ILogger
        where TLog : LogEntry, new()
        where TDbContext : DbContext
    {
        readonly string _name;
        readonly Func<string, LogLevel, bool> _filter;
        readonly IServiceProvider _services;
        private readonly ConcurrentQueue<TLog> _entryQueue = new ConcurrentQueue<TLog>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkLogger{TDbContext, TLog}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public EntityFrameworkLogger(string name, Func<string, LogLevel, bool> filter, IServiceProvider serviceProvider)
        {
            _name = name;
            _filter = filter ?? GetFilter(serviceProvider.GetService<IOptions<EntityFrameworkLoggerOptions>>());
            _services = serviceProvider;

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (_entryQueue.Count > 0)
                    {
                        try
                        {
                            var db = _services.GetService<TDbContext>();
                            db.ChangeTracker.AutoDetectChangesEnabled = false;
                            while (_entryQueue.Count > 0)
                            {
                                TLog entry;
                                if (_entryQueue.TryDequeue(out entry))
                                    db.Set<TLog>().Add(entry);
                            }
                            await db.SaveChangesAsync();
                        }
                        catch
                        {
                            // Ignored
                        }
                    }
                    await Task.Delay(50);
                }
            });
        }

        private Func<string, LogLevel, bool> GetFilter(IOptions<EntityFrameworkLoggerOptions> options)
        {
            if (options != null)
                return ((category, level) => GetFilter(options.Value, category, level));

            return ((category, level) => true);
        }

        private static bool GetFilter(EntityFrameworkLoggerOptions options, string category, LogLevel level)
        {
            var filter = options.Filters?.Keys.FirstOrDefault(category.StartsWith);
            if (filter != null)
                return (int) options.Filters[filter] <= (int) level;

            return true;
        }

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (_name.StartsWith("Microsoft.EntityFrameworkCore") || IsEnabled(logLevel) == false) return;

            string message;

            if (formatter != null)
            {
                message = formatter(state, exception);
            }
            else
            {
                message = state.Stringify();

                if (exception != null)
                    message += Environment.NewLine + exception.Stringify();
            }

            if (string.IsNullOrEmpty(message)) return;

            var log = new TLog
            {
                Message = message.Truncate(LogEntry.MaximumMessageLength),
                Date = DateTime.UtcNow,
                Level = logLevel.ToString(),
                Logger = _name,
                Thread = eventId.ToString()
            };

            if (exception != null)
                log.Exception = exception.ToString().Truncate(LogEntry.MaximumExceptionLength);

            var httpContext = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;

            if (httpContext != null)
            {
                log.Browser = httpContext.Request.Headers["User-Agent"];
                log.Username = httpContext.User.Identity.Name;
                try
                {
                    log.HostAddress = httpContext.Connection.LocalIpAddress?.ToString();
                }
                catch (ObjectDisposedException)
                {
                    log.HostAddress = "Disposed";
                }
                log.Url = httpContext.Request.Path;
            }

            _entryQueue.Enqueue(log);
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>
        ///   <c>true</c> if enabled.
        /// </returns>
        public bool IsEnabled(LogLevel logLevel) => _filter(_name, logLevel);

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An IDisposable that ends the logical operation scope on dispose.
        /// </returns>
        public IDisposable BeginScope<TState>(TState state) => new NoopDisposable();

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
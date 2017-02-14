namespace Unosquare.Swan.AspNetCore.Logger
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using Models;

    /// <summary>
    /// Represents a EF logger provider
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TLog">The type of the log.</typeparam>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerProvider" />
    public class EntityFrameworkLoggerProvider<TDbContext, TLog> : ILoggerProvider
        where TLog : LogEntry, new()
        where TDbContext : DbContext
    {
        readonly Func<string, LogLevel, bool> _filter;
        readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkLoggerProvider{TDbContext, TLog}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="filter">The filter.</param>
        public EntityFrameworkLoggerProvider(IServiceProvider serviceProvider, Func<string, LogLevel, bool> filter)
        {
            _filter = filter;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ILogger CreateLogger(string name)
        {
            return new EntityFrameworkLogger<TDbContext, TLog>(name, _filter, _serviceProvider);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Represents the EF Logger options
    /// </summary>
    public class EntityFrameworkLoggerOptions
    {
        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        public Dictionary<string, LogLevel> Filters { get; set; }
    }
}

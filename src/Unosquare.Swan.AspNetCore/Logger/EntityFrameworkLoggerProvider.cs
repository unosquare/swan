namespace Unosquare.Swan.AspNetCore.Logger
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using Models;

    public class EntityFrameworkLoggerProvider<TDbContext, TLog> : ILoggerProvider
        where TLog : LogEntry, new()
        where TDbContext : DbContext
    {
        readonly Func<string, LogLevel, bool> _filter;
        readonly IServiceProvider _serviceProvider;

        public EntityFrameworkLoggerProvider(IServiceProvider serviceProvider, Func<string, LogLevel, bool> filter)
        {
            _filter = filter;
            _serviceProvider = serviceProvider;
        }

        public ILogger CreateLogger(string name)
        {
            return new EntityFrameworkLogger<TDbContext, TLog>(name, _filter, _serviceProvider);
        }

        public void Dispose() { }
    }

    public class EntityFrameworkLoggerOptions
    {
        public Dictionary<string, LogLevel> Filters { get; set; }
    }
}

// TODO: This requires a concrete DbContext, and litelib as dep.
//using NLog;
//using NLog.Targets;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Unosquare.Swan.Nlog
//{
//    /// <summary>
//    /// A custom NLog compatible log target. It saves log entries to the database.
//    /// </summary>
//    /// <seealso cref="NLog.Targets.Target" />
//    public sealed class AppDbLogTarget<T> : Target
//    {
//        private readonly T AppDb = Activator.CreateInstance<T>();
//        private readonly ConcurrentQueue<LogEntry> EntryQueue = new ConcurrentQueue<LogEntry>();
//        private readonly Task FlushTask;
//        private readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

//        /// <summary>
//        /// Initializes a new instance of the <see cref="AppDbLogTarget"/> class.
//        /// </summary>
//        public AppDbLogTarget()
//        {
//            var token = TokenSource.Token;

//            FlushTask = Task.Factory.StartNew(async () =>
//            {
//                while (true)
//                {
//                    if (EntryQueue.Count > 0)
//                    {
//                        using (var transaction = (AppDb.Connection as IDbConnection).BeginTransaction())
//                        {
//                            LogEntry entry;
//                            while (EntryQueue.Count > 0)
//                                if (EntryQueue.TryDequeue(out entry))
//                                    await AppDb.LogEntries.InsertAsync(entry);

//                            transaction.Commit();
//                        }
//                    }

//                    if (token.IsCancellationRequested)
//                        break;

//                    await Task.Delay(50);
//                }
//            }, token);
//        }

//        /// <summary>
//        /// Closes the target and releases any unmanaged resources.
//        /// </summary>
//        protected override void CloseTarget()
//        {
//            TokenSource.Cancel();
//            FlushTask.Wait(500);
//            base.CloseTarget();
//        }

//        /// <summary>
//        /// Writes logging event to the log target.
//        /// classes.
//        /// </summary>
//        /// <param name="logEvent">Logging event to be written out.</param>
//        protected override void Write(LogEventInfo logEvent)
//        {
//            // Skip writing to the logger db -- we don't want to write the queries to the DB
//            if (logEvent.LoggerName.Equals(SqliteLogger.Current.LoggerName))
//                return;

//            // Create and insert the log entry to the queue
//            var entry = new LogEntry
//            {
//                ExceptionType = logEvent.Exception?.GetType().Name ?? string.Empty,
//                Level = logEvent.Level.ToString(),
//                Logger = logEvent.LoggerName.Substring(logEvent.LoggerName.LastIndexOf('.') + 1),
//                Message = logEvent.FormattedMessage,
//                SequenceID = logEvent.SequenceID,
//                EntryDateUtc = logEvent.TimeStamp.ToUniversalTime(),
//                SessionId = logEvent.Properties.ContainsKey("SessionId") ? logEvent.Properties["SessionId"].ToString() : null
//            };

//            EntryQueue.Enqueue(entry);
//        }
//    }
//}

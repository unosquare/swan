using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swan
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static partial class SwanExtensions
    {
        /// <summary>
        /// Does the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        public static void Retry(
            this Action action,
            TimeSpan retryInterval = default,
            int retryCount = 3)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Retry<object?>(() =>
                {
                    action();
                    return null;
                },
                retryInterval,
                retryCount);
        }

        /// <summary>
        /// Does the specified action.
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>
        /// The return value of the method that this delegate encapsulates.
        /// </returns>
        /// <exception cref="ArgumentNullException">action.</exception>
        /// <exception cref="AggregateException">Represents one or many errors that occur during application execution.</exception>
        public static T Retry<T>(
            this Func<T> action,
            TimeSpan retryInterval = default,
            int retryCount = 3)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (retryInterval == default)
                retryInterval = TimeSpan.FromSeconds(1);

            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                        Task.Delay(retryInterval).Wait();

                    return action();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
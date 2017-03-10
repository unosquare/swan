using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan.Components
{
    /// <summary>
    /// Retry helper based on http://stackoverflow.com/a/1563234/1096693
    /// </summary>
    public class Retry
    {
        /// <summary>
        /// Does the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        public static void Do(
            Action action,
            TimeSpan retryInterval = default(TimeSpan),
            int retryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }

        /// <summary>
        /// Does the specified action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public static T Do<T>(
            Func<T> action,
            TimeSpan retryInterval = default(TimeSpan),
            int retryCount = 3)
        {
            if (retryInterval == default(TimeSpan))
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
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
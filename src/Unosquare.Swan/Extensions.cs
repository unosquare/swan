namespace Unosquare.Swan
{
    using System.Threading.Tasks;
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static partial class Extensions
    {
        private static readonly Lazy<PropertyTypeCache> CopyPropertiesTargets = new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>Number of properties that was copied successful</returns>
        public static int CopyPropertiesTo<T>(this T source, object target)
        {
            return CopyPropertiesTo(source, target, null);
        }

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The destination.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>Returns the number of properties that were successfully copied</returns>
        public static int CopyPropertiesTo(this object source, object target, string[] ignoreProperties)
        {
            return Components.ObjectCopier.Copy(source, target, null, ignoreProperties);
        }

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>Number of properties that was copied successful</returns>
        public static int CopyOnlyPropertiesTo<T>(this T source, object target)
        {
            return CopyOnlyPropertiesTo(source, target, null);
        }

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The destination.</param>
        /// <param name="propertiesToCopy">Properties to copy.</param>
        /// <returns>Returns the number of properties that were successfully copied</returns>
        public static int CopyOnlyPropertiesTo(this object source, object target, string[] propertiesToCopy)
        {
            return Components.ObjectCopier.Copy(source, target, propertiesToCopy, null);
        }

        /// <summary>
        /// Copies the properties to new.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>Returns the specified type of properties that were successfully copied</returns>
        public static T CopyPropertiesToNew<T>(this object source, string[] ignoreProperties = null)
        {
            var target = Activator.CreateInstance<T>();
            source.CopyPropertiesTo(target, ignoreProperties);
            return target;
        }

        /// <summary>
        /// Copies the only properties to new.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertiesToCopy">The properties to copy.</param>
        /// <returns>Returns the specified type of properties that were successfully copied</returns>
        public static T CopyOnlyPropertiesToNew<T>(this object source, string[] propertiesToCopy = null)
        {
            var target = Activator.CreateInstance<T>();
            source.CopyOnlyPropertiesTo(target, propertiesToCopy);
            return target;
        }

        /// <summary>
        /// Iterates over the keys of the source and tries to write a compatible value to a public, 
        /// instance, writable property in the destination.
        /// 
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>Number of properties that was copied successful</returns>
        public static int CopyPropertiesTo(
            this IDictionary<string, object> source, 
            object target,
            string[] ignoreProperties)
        {
            var copiedProperties = 0;

            var targetType = target.GetType();
            var targetProperties = CopyPropertiesTargets.Value.Retrieve(targetType, () =>
            {
                return targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanWrite && Definitions.AllBasicTypes.Contains(x.PropertyType));
            });

            var targetPropertyNames = targetProperties.Select(t => t.Name.ToLowerInvariant());
            var filteredSourceKeys = source
                .Where(s => targetPropertyNames.Contains(s.Key.ToLowerInvariant()) && s.Value != null)
                .ToArray();

            var ignoredProperties = ignoreProperties?.Where(p => string.IsNullOrWhiteSpace(p) == false)
                                        .Select(p => p.ToLowerInvariant())
                                        .ToArray() ?? new string[] {};

            foreach (var sourceKey in filteredSourceKeys)
            {
                var targetProperty =
                    targetProperties.SingleOrDefault(s => s.Name.ToLowerInvariant() == sourceKey.Key.ToLowerInvariant());
                if (targetProperty == null) continue;

                if (ignoredProperties.Contains(targetProperty.Name.ToLowerInvariant()))
                {
                    continue;
                }

                try
                {
                    if (targetProperty.PropertyType == sourceKey.Value.GetType())
                    {
                        targetProperty.SetValue(target, sourceKey.Value);
                        copiedProperties++;
                        continue;
                    }

                    var sourceStringValue = sourceKey.Value.ToStringInvariant();

                    if (targetProperty.PropertyType == typeof(bool))
                    {
                        sourceStringValue = sourceStringValue == "1"
                            ? bool.TrueString.ToLowerInvariant()
                            : bool.FalseString.ToLowerInvariant();
                    }

                    object targetValue;
                    if (Definitions.BasicTypesInfo[targetProperty.PropertyType].TryParse(sourceStringValue,
                        out targetValue))
                    {
                        targetProperty.SetValue(target, targetValue);
                        copiedProperties++;
                    }
                }
                catch
                {
                    // swallow
                }
            }

            return copiedProperties;
        }

        /// <summary>
        /// Measures the elapsed time of the given action as a TimeSpan
        /// This method uses a high precision Stopwatch.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>
        /// A  time interval that represents a specified time, where the specification is in units of ticks
        /// </returns>
        public static TimeSpan Benchmark(this Action target)
        {
            var sw = new Stopwatch();

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
        /// Does the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        public static void Retry(
            this Action action,
            TimeSpan retryInterval = default(TimeSpan),
            int retryCount = 3)
        {
            Retry<object>(() =>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>The return value of the method that this delegate encapsulates</returns>
        /// <exception cref="AggregateException">
        ///     Represents one or many errors that occur during application execution
        /// </exception>
        public static T Retry<T>(
            this Func<T> action,
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

        /// <summary>
        /// Retrieves the exception message, plus all the inner exception messages separated by new lines
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="priorMessage">The prior message.</param>
        /// <returns>A <see cref="System.String" /> that represents this instance</returns>
        public static string ExceptionMessage(this Exception ex, string priorMessage = "")
        {
            while (true)
            {
                var fullMessage = string.IsNullOrWhiteSpace(priorMessage) ? ex.Message : priorMessage + "\r\n" + ex.Message;

                if (string.IsNullOrWhiteSpace(ex.InnerException?.Message))
                    return fullMessage;

                ex = ex.InnerException;
                priorMessage = fullMessage;
            }
        }
    }
}
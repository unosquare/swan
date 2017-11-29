namespace Unosquare.Swan
{
    using Attributes;
    using Reflection;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>Number of properties that was copied successful</returns>
        public static int CopyPropertiesTo<T>(this T source, object target)
        {
            var copyable = GetCopyableProperties(target);
            return copyable.Any()
                ? CopyOnlyPropertiesTo(source, target, copyable)
                : CopyPropertiesTo(source, target, null);
        }

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The destination.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// Number of properties that were successfully copied
        /// </returns>
        public static int CopyPropertiesTo(this object source, object target, string[] ignoreProperties = null)
        {
            return Components.ObjectMapper.Copy(source, target, null, ignoreProperties);
        }

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
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
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The destination.</param>
        /// <param name="propertiesToCopy">Properties to copy.</param>
        /// <returns>
        /// Number of properties that were successfully copied
        /// </returns>
        public static int CopyOnlyPropertiesTo(this object source, object target, string[] propertiesToCopy)
        {
            return Components.ObjectMapper.Copy(source, target, propertiesToCopy);
        }

        /// <summary>
        /// Copies the properties to new instance of T.
        /// </summary>
        /// <typeparam name="T">The new object type</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// The specified type with properties copied
        /// </returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static T DeepClone<T>(this T source, string[] ignoreProperties = null)
            where T : class
        {
            return source.CopyPropertiesToNew<T>(ignoreProperties);
        }

        /// <summary>
        /// Copies the properties to new instance of T.
        /// </summary>
        /// <typeparam name="T">The new object type</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// The specified type with properties copied
        /// </returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static T CopyPropertiesToNew<T>(this object source, string[] ignoreProperties = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var target = Activator.CreateInstance<T>();
            var copyable = target.GetCopyableProperties();

            if (copyable.Any())
                source.CopyOnlyPropertiesTo(target, copyable);
            else
                source.CopyPropertiesTo(target, ignoreProperties);

            return target;
        }

        /// <summary>
        /// Copies the only properties to new instance of T.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertiesToCopy">The properties to copy.</param>
        /// <returns>
        /// The specified type with properties copied
        /// </returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static T CopyOnlyPropertiesToNew<T>(this object source, string[] propertiesToCopy)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var target = Activator.CreateInstance<T>();
            source.CopyOnlyPropertiesTo(target, propertiesToCopy);
            return target;
        }

        /// <summary>
        /// Iterates over the keys of the source and tries to write a compatible value to a public, 
        /// instance, writable property in the destination.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreKeys">The ignore keys.</param>
        /// <returns>Number of properties that was copied successful</returns>
        public static int CopyKeyValuePairTo(
            this IDictionary<string, object> source, 
            object target,
            string[] ignoreKeys = null)
        {
            return Components.ObjectMapper.Copy(source, target, null, ignoreKeys);
        }

        /// <summary>
        /// Measures the elapsed time of the given action as a TimeSpan
        /// This method uses a high precision Stopwatch.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>
        /// A  time interval that represents a specified time, where the specification is in units of ticks
        /// </returns>
        /// <exception cref="ArgumentNullException">target</exception>
        public static TimeSpan Benchmark(this Action target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

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
            if (action == null)
                throw new ArgumentNullException(nameof(action));

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
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>
        /// The return value of the method that this delegate encapsulates
        /// </returns>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <exception cref="AggregateException">Represents one or many errors that occur during application execution</exception>
        public static T Retry<T>(
            this Func<T> action,
            TimeSpan retryInterval = default(TimeSpan),
            int retryCount = 3)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

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
                if (ex == null)
                    throw new ArgumentNullException(nameof(ex));

                var fullMessage = string.IsNullOrWhiteSpace(priorMessage)
                    ? ex.Message
                    : priorMessage + "\r\n" + ex.Message;

                if (string.IsNullOrWhiteSpace(ex.InnerException?.Message))
                    return fullMessage;

                ex = ex.InnerException;
                priorMessage = fullMessage;
            }
        }

        /// <summary>
        /// Gets the copyable properties.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Array of properties
        /// </returns>
        /// <exception cref="ArgumentNullException">model</exception>
        public static string[] GetCopyableProperties(this object model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var cachedProperties = Runtime.PropertyTypeCache.Value.Retrieve(model.GetType(),
                PropertyTypeCache.GetAllPropertiesFunc(model.GetType()));

            return cachedProperties
                .Select(x => new {x.Name, HasAttribute = x.GetCustomAttribute<CopyableAttribute>() != null})
                .Where(x => x.HasAttribute)
                .Select(x => x.Name)
                .ToArray();
        }

        internal static void CreateTarget(
            this object source, 
            Type targetType, 
            bool includeNonPublic, 
            ref object target)
        {
            // When using arrays, there is no default constructor, attempt to build a compatible array
            if (source is string)
            {
                // do nothing. Simply skip creation
            }
            else if (source is IList sourceObjectList && targetType.IsArray)
            {
                target = Array.CreateInstance(targetType.GetElementType(), sourceObjectList.Count);
            }
            else
            {
                target = Activator.CreateInstance(targetType, includeNonPublic);
            }
        }
    }
}
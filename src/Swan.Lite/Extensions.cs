using Swan.Formatters;
using Swan.Mappers;
using Swan.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swan
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination.
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// Number of properties that was copied successful.
        /// </returns>
        public static int CopyPropertiesTo<T>(this T source, object target, params string[]? ignoreProperties)
            where T : class =>
            ObjectMapper.Copy(source, target, GetCopyableProperties(target), ignoreProperties);

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The destination.</param>
        /// <param name="propertiesToCopy">Properties to copy.</param>
        /// <returns>
        /// Number of properties that were successfully copied.
        /// </returns>
        public static int CopyOnlyPropertiesTo(this object source, object target, params string[]? propertiesToCopy)
            => ObjectMapper.Copy(source, target, propertiesToCopy);

        /// <summary>
        /// Copies the properties to new instance of T.
        /// </summary>
        /// <typeparam name="T">The new object type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// The specified type with properties copied.
        /// </returns>
        /// <exception cref="ArgumentNullException">source.</exception>
        public static T CopyPropertiesToNew<T>(this object source, string[]? ignoreProperties = null)
            where T : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var target = Activator.CreateInstance<T>();
            ObjectMapper.Copy(source, target, GetCopyableProperties(target), ignoreProperties);

            return target;
        }

        /// <summary>
        /// Copies the only properties to new instance of T.
        /// </summary>
        /// <typeparam name="T">Object Type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertiesToCopy">The properties to copy.</param>
        /// <returns>
        /// The specified type with properties copied.
        /// </returns>
        /// <exception cref="ArgumentNullException">source.</exception>
        public static T CopyOnlyPropertiesToNew<T>(this object source, params string[] propertiesToCopy)
            where T : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var target = Activator.CreateInstance<T>();
            ObjectMapper.Copy(source, target, propertiesToCopy);

            return target;
        }

        /// <summary>
        /// Iterates over the keys of the source and tries to write a compatible value to a public, 
        /// instance, writable property in the destination.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreKeys">The ignore keys.</param>
        /// <returns>Number of properties that was copied successful.</returns>
        public static int CopyKeyValuePairTo(
            this IDictionary<string, object> source,
            object? target,
            params string[] ignoreKeys) =>
            source == null
                ? throw new ArgumentNullException(nameof(source))
                : ObjectMapper.Copy(source, target, null, ignoreKeys);

        /// <summary>
        /// Iterates over the keys of the source and tries to write a compatible value to a public,
        /// instance, writable property in the destination.
        /// </summary>
        /// <typeparam name="T">Object Type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="ignoreKeys">The ignore keys.</param>
        /// <returns>
        /// The specified type with properties copied.
        /// </returns>
        public static T CopyKeyValuePairToNew<T>(
            this IDictionary<string, object> source,
            params string[] ignoreKeys)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var target = Activator.CreateInstance<T>();
            source.CopyKeyValuePairTo(target, ignoreKeys);
            return target;
        }

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

        /// <summary>
        /// Gets the copyable properties.
        ///
        /// If there is no properties with the attribute <c>AttributeCache</c> returns all the properties.
        /// </summary>
        /// <param name="this">The object.</param>
        /// <returns>
        /// Array of properties.
        /// </returns>
        /// <exception cref="ArgumentNullException">model.</exception>
        /// <seealso cref="AttributeCache"/>
        public static IEnumerable<string> GetCopyableProperties(this object @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var collection = PropertyTypeCache.DefaultCache.Value
                .RetrieveAllProperties(@this.GetType(), true);

            var properties = collection
                .Select(x => new
                {
                    x.Name,
                    HasAttribute = AttributeCache.DefaultCache.Value.RetrieveOne<CopyableAttribute>(x) != null,
                })
                .Where(x => x.HasAttribute)
                .Select(x => x.Name);

            return properties.Any()
                ? properties
                : collection.Select(x => x.Name);
        }

        internal static void CreateTarget(
            this object source,
            Type targetType,
            bool includeNonPublic,
            ref object? target)
        {
            switch (source)
            {
                // do nothing. Simply skip creation
                case string _:
                    break;

                // When using arrays, there is no default constructor, attempt to build a compatible array
                case IList sourceObjectList when targetType.IsArray:
                    var elementType = targetType.GetElementType();

                    if (elementType != null)
                        target = Array.CreateInstance(elementType, sourceObjectList.Count);
                    break;
                default:
                    var constructors = ConstructorTypeCache.DefaultCache.Value
                        .RetrieveAllConstructors(targetType, includeNonPublic);

                    // Try to check if empty constructor is available
                    if (constructors.Any(x => x.Item2.Length == 0))
                    {
                        target = Activator.CreateInstance(targetType, includeNonPublic);
                    }
                    else
                    {
                        var firstCtor = constructors
                            .OrderBy(x => x.Item2.Length)
                            .FirstOrDefault();

                        target = Activator.CreateInstance(targetType,
                            firstCtor?.Item2.Select(arg => arg.GetType().GetDefault()).ToArray());
                    }

                    break;
            }
        }

        internal static string GetNameWithCase(this string name, JsonSerializerCase jsonSerializerCase) =>
            jsonSerializerCase switch
            {
                JsonSerializerCase.PascalCase => char.ToUpperInvariant(name[0]) + name.Substring(1),
                JsonSerializerCase.CamelCase => char.ToLowerInvariant(name[0]) + name.Substring(1),
                JsonSerializerCase.None => name,
                _ => throw new ArgumentOutOfRangeException(nameof(jsonSerializerCase), jsonSerializerCase, null)
            };
    }
}

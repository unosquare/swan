namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Generic;
    using Attributes;
#if NETSTANDARD1_3 || UWP
    using System.Reflection;
#endif

    /// <summary>
    /// Represents an object validator 
    /// </summary>
    public class ObjectValidator
    {
        private readonly Dictionary<Type, List<Tuple<Delegate,string>>> _predicates =
            new Dictionary<Type, List<Tuple<Delegate, string>>>();

        /// <summary>
        /// Checks if an object is valid based on the custom validator attributes
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="obj">The object</param>
        /// <returns>A bool indicating if it is a valid object</returns>
        public List<string> Validate<T>(T obj)
        {
            if (Equals(obj, null))
                throw new ArgumentNullException(nameof(obj));

            var errorList = new List<string>();

            if (_predicates.ContainsKey(typeof(T)))
            {
                foreach (var validation in _predicates[typeof(T)])
                {
                    if (!(bool)validation.Item1.DynamicInvoke(obj))
                        errorList.Add(validation.Item2);
                }
            }            

            var properties = Runtime.AttributeCache.Value.RetrieveFromType<T>(typeof(IValidator));

            foreach (var prop in properties)
            {
                foreach (var attribute in prop.Value)
                {
                    var val = (IValidator)attribute;

                    if (!val.IsValid(prop.Key.GetValue(obj, null)))
                        errorList.Add($"{prop.Key.Name}: {val.ErrorMessage}");
                }
            }

            return errorList;
        }

        public bool IsValid<T>(T obj)
        {
            if (Equals(obj, null))
                throw new ArgumentNullException(nameof(obj));
            if (!_predicates.ContainsKey(typeof(T)))
                throw new InvalidOperationException("There are no validators for this type");

            if (_predicates.ContainsKey(typeof(T)))
            {
                foreach (var validation in _predicates[typeof(T)])
                {
                    if (!(bool)validation.Item1.DynamicInvoke(obj))
                        return false;
                }
            }

            var properties = Runtime.AttributeCache.Value.RetrieveFromType<T>(typeof(IValidator));

            foreach (var prop in properties)
            {
                foreach (var attribute in prop.Value)
                {
                    var val = (IValidator)attribute;

                    if (!val.IsValid(prop.Key.GetValue(obj, null)))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a validator to a specific class
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="predicate">The predicate that will be evaluated</param>
        public void AddValidator<T>(Predicate<T> predicate, string message)
            where T : class
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(message);

            if (!_predicates.TryGetValue(typeof(T), out var existing))
            {
                existing = new List<Tuple<Delegate,string>>();
                _predicates[typeof(T)] = existing;
            }

            existing.Add(Tuple.Create((Delegate)predicate,message));
        }
    }
}

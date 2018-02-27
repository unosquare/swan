namespace Unosquare.Swan.Lite.Components
{
    using System;
    using System.Collections.Generic;
    using Unosquare.Swan.Lite.Attributes;
    using Unosquare.Swan.Reflection;
    using System.Reflection;

    /// <summary>
    /// Represents an object validator 
    /// </summary>
    public class ObjectValidator
    {
        private readonly Dictionary<Type, List<Delegate>> _predicates =
            new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Checks if an object is valid based on the custom validator attributes
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="obj">The object</param>
        /// <returns>A bool indicating if it is a valid object</returns>
        public static bool IsValid<T>(T obj)
        {
            if (Equals(obj, null))
                throw new ArgumentNullException(nameof(obj));

            var properties = Runtime.PropertyTypeCache.Value.Retrieve(typeof(T), 
                PropertyTypeCache.GetAllPublicPropertiesFunc(typeof(T)));

            foreach (var pi in properties)
            {   
                foreach (var attribute in pi.GetCustomAttributes(typeof(IValidator), true))
                {
                    var val = (IValidator)attribute;

                    if (!val.IsValid(pi.GetValue(obj, null)))
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
            public void AddValidator<T>(Predicate<T> predicate)
            where T : class
        {
            if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

            List<Delegate> existing;

            if (!_predicates.TryGetValue(typeof(T), out existing))
            {
                existing = new List<Delegate>();
                _predicates[typeof(T)] = existing;
            }

            existing.Add(predicate);
        }

        /// <summary>
        /// Validates an object
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="obj">the object</param>
        /// <returns>The result of the predicate</returns>
        public bool Validate<T>(T obj)
        {
            if (Equals(obj, null))
                throw new ArgumentNullException(nameof(obj));

            if (!_predicates.ContainsKey(typeof(T)))
                throw new InvalidOperationException("There are no validators for this type");

            foreach (var predicate in _predicates[typeof(T)])
            {
                if (!(bool)predicate.DynamicInvoke(obj))
                    return false;
            }

            return true;          
        }
    }
}

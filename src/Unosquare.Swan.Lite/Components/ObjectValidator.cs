namespace Unosquare.Swan.Lite.Components
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an object validator 
    /// </summary>
    public class ObjectValidator
    {
        private readonly Dictionary<Type, Delegate> _predicates =
            new Dictionary<Type, Delegate>();

        /// <summary>
        /// Adds a validator to a specific class
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="predicate">The predicate that will be evaluated</param>
        public void AddValidator<T>(Predicate<T> predicate)
            where T : class =>
            _predicates[typeof(T)] = predicate ?? throw new ArgumentNullException(nameof(predicate));

        /// <summary>
        /// Validates an object
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="obj">the object</param>
        /// <returns>The result of the predicate</returns>
        public bool Validate<T>(T obj)
        {
            if (!_predicates.ContainsKey(typeof(T)))
                throw new InvalidOperationException("There are no validators for this type");

            return (bool)_predicates[typeof(T)].DynamicInvoke(obj);            
        }
    }
}

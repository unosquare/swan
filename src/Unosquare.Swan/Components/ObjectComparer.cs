namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Reflection;

    /// <summary>
    /// Represents a quick object comparer using the public properties of an object
    /// or the public members in a structure
    /// </summary>
    public static class ObjectComparer
    {
        /// <summary>
        /// Compare if two variables of the same type are equal.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare</typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns><c>true</c> if the variables are equal; otherwise, <c>false</c></returns>
        public static bool AreEqual<T>(T left, T right) => AreEqual(left, right, typeof(T));

        /// <summary>
        /// Compare if two variables of the same type are equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>
        ///   <c>true</c> if the variables are equal; otherwise, <c>false</c>
        /// </returns>
        /// <exception cref="ArgumentNullException">targetType</exception>
        public static bool AreEqual(object left, object right, Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (Definitions.BasicTypesInfo.ContainsKey(targetType))
                return Equals(left, right);

            if (targetType.IsValueType() || targetType.IsArray)
                return AreStructsEqual(left, right, targetType);

            return AreObjectsEqual(left, right, targetType);
        }

        /// <summary>
        /// Compare if two objects of the same type are equal.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare</typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c></returns>
        public static bool AreObjectsEqual<T>(T left, T right)
            where T : class
        {
            return AreObjectsEqual(left, right, typeof(T));
        }

        /// <summary>
        /// Compare if two objects of the same type are equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c></returns>
        /// <exception cref="ArgumentNullException">targetType</exception>
        public static bool AreObjectsEqual(object left, object right, Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            var properties = RetrieveProperties(targetType).ToArray();

            foreach (var propertyTarget in properties)
            {
                var targetPropertyGetMethod = propertyTarget.GetGetMethod();

                if (propertyTarget.PropertyType.IsArray)
                {
                    var leftObj = targetPropertyGetMethod.Invoke(left, null) as IEnumerable;
                    var rightObj = targetPropertyGetMethod.Invoke(right, null) as IEnumerable;

                    if (!AreEnumerationsEquals(leftObj, rightObj))
                        return false;
                }
                else
                {
                    if (Equals(targetPropertyGetMethod.Invoke(left, null), targetPropertyGetMethod.Invoke(right, null)) == false)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compare if two structures of the same type are equal.
        /// </summary>
        /// <typeparam name="T">The type of structs to compare</typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns><c>true</c> if the structs are equal; otherwise, <c>false</c></returns>
        public static bool AreStructsEqual<T>(T left, T right)
            where T : struct
        {
            return AreStructsEqual(left, right, typeof(T));
        }

        /// <summary>
        /// Compare if two structures of the same type are equal.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>
        ///   <c>true</c> if the structs are equal; otherwise, <c>false</c>
        /// </returns>
        /// <exception cref="ArgumentNullException">targetType</exception>
        public static bool AreStructsEqual(object left, object right, Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            var fields = new List<MemberInfo>(RetrieveFields(targetType))
                .Union(RetrieveProperties(targetType));

            foreach (var targetMember in fields)
            {
                if (targetMember is FieldInfo targetField)
                {
                    if (Equals(targetField.GetValue(left), targetField.GetValue(right)) == false)
                        return false;
                }
                else
                {
                    var targetPropertyGetMethod = (targetMember as PropertyInfo)?.GetGetMethod();

                    if (targetPropertyGetMethod != null &&
                        Equals(targetPropertyGetMethod.Invoke(left, null), targetPropertyGetMethod.Invoke(right, null)) == false)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compare if two enumerables are equal.
        /// </summary>
        /// <typeparam name="T">The type of enums to compare</typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// True if two specified types are equal; otherwise, false
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// left
        /// or
        /// right
        /// </exception>
        public static bool AreEnumerationsEquals<T>(T left, T right)
            where T : IEnumerable
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            var leftEnumerable = left.Cast<object>().ToArray();
            var rightEnumerable = right.Cast<object>().ToArray();

            if (leftEnumerable.Length != rightEnumerable.Length)
                return false;

            for (var i = 0; i < leftEnumerable.Length; i++)
            {
                var leftEl = leftEnumerable[i];
                var rightEl = rightEnumerable[i];

                if (!AreEqual(leftEl, rightEl, leftEl.GetType()))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Retrieves PropertyInfo[] (both public and non-public) for the given type
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>Properties for the given type</returns>
        private static PropertyInfo[] RetrieveProperties(Type targetType)
            => Runtime.PropertyTypeCache.Value.Retrieve(targetType, PropertyTypeCache.GetAllPropertiesFunc(targetType));

        /// <summary>
        /// Retrieves FieldInfo[] (public) for the given type
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>Value of a field supported by a given object</returns>
        private static FieldInfo[] RetrieveFields(Type targetType)
            => Runtime.FieldTypeCache.Value.Retrieve(targetType, FieldTypeCache.GetAllFieldsFunc(targetType));
    }
}
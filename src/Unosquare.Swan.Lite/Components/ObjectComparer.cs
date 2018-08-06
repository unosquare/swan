namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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

            var properties = Runtime.PropertyTypeCache.RetrieveAllProperties(targetType).ToArray();

            foreach (var propertyTarget in properties)
            {
                var targetPropertyGetMethod = propertyTarget.GetCacheGetMethod();
                
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

            var fields = new List<MemberInfo>(Runtime.FieldTypeCache.RetrieveAllFields(targetType))
                .Union(Runtime.PropertyTypeCache.RetrieveAllProperties(targetType));

            foreach (var targetMember in fields)
            {
                switch (targetMember)
                {
                    case FieldInfo field:
                        if (Equals(field.GetValue(left), field.GetValue(right)) == false)
                            return false;
                        break;
                    case PropertyInfo property:
                        var targetPropertyGetMethod = property.GetCacheGetMethod();

                        if (targetPropertyGetMethod != null &&
                            Equals(targetPropertyGetMethod.Invoke(left, null), targetPropertyGetMethod.Invoke(right, null)) == false)
                            return false;
                        break;
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
    }
}
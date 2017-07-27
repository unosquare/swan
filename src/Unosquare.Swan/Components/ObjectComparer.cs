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
        private static readonly PropertyTypeCache PropertyTypeCache = new PropertyTypeCache();
        private static readonly FieldTypeCache FieldTypeCache = new FieldTypeCache();

        #region Private API

        /// <summary>
        /// Retrieves PropertyInfo[] (both public and non-public) for the given type
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        private static PropertyInfo[] RetrieveProperties(Type targetType)
        {
            return PropertyTypeCache.Retrieve(targetType, () =>
            {
                return targetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.CanRead || p.CanWrite).ToArray();
            });
        }

        /// <summary>
        /// Retrieves FieldInfo[] (public) for the given type
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        private static FieldInfo[] RetrieveFields(Type targetType)
        {
            return FieldTypeCache.Retrieve(targetType,
                () => targetType.GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray());
        }

        private static bool AreObjectsEqual(object left, object right, Type targetType)
        {
            var properties = RetrieveProperties(targetType).ToArray();

            foreach (var propertyTarget in properties)
            {
                var targetPropertyGetMethod = propertyTarget.GetGetMethod();

                if (propertyTarget.PropertyType.IsArray)
                {
                    var leftObj = targetPropertyGetMethod.Invoke(left, null) as IEnumerable;
                    var rightObj = targetPropertyGetMethod.Invoke(right, null) as IEnumerable;
                    
                    if (AreEnumsEqual(leftObj, rightObj) == false)
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

        private static bool AreStructsEqual(object left, object right, Type targetType)
        {
            var fields = new List<MemberInfo>();
            fields.AddRange(RetrieveFields(targetType));
            fields.AddRange(RetrieveProperties(targetType));

            foreach (var targetMember in fields)
            {
                var targetField = targetMember as FieldInfo;

                if (targetField != null)
                {
                    if (Equals(targetField.GetValue(left), targetField.GetValue(right)) == false)
                        return false;
                }
                else
                {
                    var targetPropertyGetMethod = (targetMember as PropertyInfo).GetGetMethod();

                    if (Equals(targetPropertyGetMethod.Invoke(left, null), targetPropertyGetMethod.Invoke(right, null)) == false)
                        return false;
                }
            }

            return true;
        }

        private static bool AreEqual(object left, object right, Type targetType)
        {
            if (Definitions.BasicTypesInfo.ContainsKey(targetType))
                return Equals(left, right);

            if (targetType.IsValueType() || targetType.IsArray)
                return AreStructsEqual(left, right, targetType);

            return AreObjectsEqual(left, right, targetType);
        }

        #endregion

        /// <summary>
        /// Compare if two variables of the same type are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool AreEqual<T>(T left, T right)
        {
            return AreEqual(left, right, typeof(T));
        }
        
        /// <summary>
        /// Compare if two objects of the same type are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool AreObjectsEqual<T>(T left, T right) 
            where T : class
        {
            return AreObjectsEqual(left, right, typeof(T));
        }

        /// <summary>
        /// Compare if two structures of the same type are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool AreStructsEqual<T>(T left, T right) 
            where T : struct
        {
            return AreStructsEqual(left, right, typeof(T));
        }

        /// <summary>
        /// Compare if two enumerables are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool AreEnumsEqual<T>(T left, T right) 
            where T : IEnumerable
        {
            var leftEnumerable = left.Cast<object>().ToArray();
            var rightEnumerable = right.Cast<object>().ToArray();

            if (leftEnumerable.Count() != rightEnumerable.Count())
                return false;

            for (var i = 0; i < leftEnumerable.Count(); i++)
            {
                var leftEl = leftEnumerable[i];
                var rightEl = rightEnumerable[i];
                var targetType = leftEl.GetType();

                if (AreEqual(leftEl, rightEl, targetType) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
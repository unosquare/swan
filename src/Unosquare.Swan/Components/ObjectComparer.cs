using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unosquare.Swan.Reflection;

namespace Unosquare.Swan.Components
{
    /// <summary>
    /// Represents a quick object comparer using the public properties of an object
    /// or the public members in a struct
    /// </summary>
    public static class ObjectComparer
    {
        private static readonly PropertyTypeCache PropertyTypeCache = new PropertyTypeCache();
        private static readonly FieldTypeCache FieldTypeCache = new FieldTypeCache();

        #region Private API

        /// <summary>
        /// Retrieves PropertyInfo[] (both public and non-public) for the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static PropertyInfo[] RetrieveProperties<T>()
        {
            return PropertyTypeCache.Retrieve(typeof(T), () =>
            {
                return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.CanRead || p.CanWrite).ToArray();
            });
        }

        /// <summary>
        /// Retrieves FieldInfo[] (public) for the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static FieldInfo[] RetrieveFields<T>()
        {
            return FieldTypeCache.Retrieve(typeof(T),
                () => typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray());
        }

        #endregion

        /// <summary>
        /// Compare if two object of the same type are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool AreEqual<T>(T left, T right) where T : class
        {
            var properties = RetrieveProperties<T>().ToArray();

            foreach (var propertyTarget in properties)
            {
                var targetPropertyGetMethod = propertyTarget.GetGetMethod();
                
                if (object.Equals(targetPropertyGetMethod.Invoke(left, null), targetPropertyGetMethod.Invoke(right, null)) == false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Compare if two structs of the same type are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool AreStructsEqual<T>(T left, T right) where T : struct
        {
            var fields = new List<MemberInfo>();

            if (typeof(T).IsValueType())
            {
                fields.AddRange(RetrieveFields<T>());
            }

            fields.AddRange(RetrieveProperties<T>().ToArray());

            foreach (var targetMember in fields)
            {
                var targetField = (targetMember as FieldInfo);

                if (targetField != null)
                {
                    if (targetField.GetValue(left).Equals(targetField.GetValue(right)) == false)
                        return false;
                }
                else
                {
                    var targetPropertyGetMethod = (targetMember as PropertyInfo).GetGetMethod();

                    if (object.Equals(targetPropertyGetMethod.Invoke(left, null), targetPropertyGetMethod.Invoke(right, null)) == false)
                        return false;
                }
            }

            return true;
        }
    }
}
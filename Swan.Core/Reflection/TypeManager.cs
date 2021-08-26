using System;
using System.Collections.Concurrent;

namespace Swan.Reflection
{
    public static class TypeManager
    {
        private static readonly ConcurrentDictionary<Type, ExtendedTypeInfo> TypeCache = new();

        public static bool IsBasicType(this Type t) =>
            t is not null ? t.TypeInfo().IsBasicType : throw new ArgumentNullException(nameof(t));

        public static ExtendedTypeInfo TypeInfo(this Type t)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));

            if (TypeCache.TryGetValue(t, out var typeInfo))
                return typeInfo;

            typeInfo = new ExtendedTypeInfo(t);
            TypeCache.TryAdd(t, typeInfo);

            return typeInfo;
        }
    }
}

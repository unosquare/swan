namespace Unosquare.Swan.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Components;

    public class AttributeCache : CacheRepository<MemberInfo, object>
    {
        public PropertyTypeCache PropertyTypeCache { get; }
        public AttributeCache(PropertyTypeCache propertyCache = null){
            PropertyTypeCache = propertyCache ?? Runtime.PropertyTypeCache.Value;
            }

        public object[] Retrieve<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return Retrieve(member, () => member.GetCustomAttributes<T>(inherit));
        }

        public Dictionary<PropertyInfo, object[]> Retrieve<T>(Type type, bool inherit = false)
            where T : Attribute
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return PropertyTypeCache.Retrieve(type, PropertyTypeCache.GetAllPublicPropertiesFunc(type))
                .ToDictionary(x => x as PropertyInfo, x => Retrieve<T>(x, inherit));
        }
    }
}
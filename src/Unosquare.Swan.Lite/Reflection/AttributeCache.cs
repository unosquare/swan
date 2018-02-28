using System;
using System.Linq;
using System.Reflection;

namespace Unosquare.Swan.Reflection
{
    using Components;

    public class AttributeCache : CacheRepository<MemberInfo, object>
    {
        public object[] Retrieve<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return Retrieve(member, () => member.GetCustomAttributes<T>(inherit));
        }
    }
}
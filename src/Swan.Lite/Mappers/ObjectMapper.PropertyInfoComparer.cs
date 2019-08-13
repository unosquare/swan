using System.Collections.Generic;
using System.Reflection;

namespace Swan.Mappers
{
    /// <summary>
    /// Represents an AutoMapper-like object to map from one object type
    /// to another using defined properties map or using the default behaviour
    /// to copy same named properties from one object to another.
    /// 
    /// The extension methods like CopyPropertiesTo use the default behaviour.
    /// </summary>
    public partial class ObjectMapper
    {
        internal class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
                => x != null && y != null && x.Name == y.Name && x.PropertyType == y.PropertyType;

            public int GetHashCode(PropertyInfo obj)
                => obj.Name.GetHashCode() + obj.PropertyType.Name.GetHashCode();
        }
    }
}
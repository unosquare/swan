using Swan.Reflection;
using System.Collections.Generic;

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
        internal class PropertyInfoComparer : IEqualityComparer<IPropertyProxy>
        {
            public bool Equals(IPropertyProxy? x, IPropertyProxy? y)
                => x != null && y != null && x.PropertyName == y.PropertyName && x.PropertyType == y.PropertyType;

            public int GetHashCode(IPropertyProxy obj)
                => obj.PropertyName.GetHashCode(System.StringComparison.InvariantCulture) +
                   obj.PropertyType.Name.GetHashCode(System.StringComparison.InvariantCulture);
        }
    }
}
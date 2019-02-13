namespace Unosquare.Swan.Components
{
    using System.Collections.Generic;
    using System.Reflection;

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
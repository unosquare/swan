using Swan.Reflection;
using System.Collections.Generic;

namespace Swan.Mapping
{
    internal class PropertyComparer : IEqualityComparer<IPropertyProxy>
    {
        public bool Equals(IPropertyProxy? x, IPropertyProxy? y) => x is not null && y is not null &&
            x.PropertyName == y.PropertyName &&
            x.PropertyType == y.PropertyType;

        public int GetHashCode(IPropertyProxy obj)
        {
            unchecked
            {
                return
                    obj.PropertyName.GetHashCode(System.StringComparison.InvariantCulture) +
                    obj.PropertyType.Name.GetHashCode(System.StringComparison.InvariantCulture);
            }
        }
    }
}

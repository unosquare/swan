namespace Swan.Data.Records;

/// <summary>
/// Provides column-to-property maps for efficient object parsing.
/// </summary>
internal static class PropertyColumnMap
{
    private static readonly object SyncLock = new();
    private static readonly Dictionary<ITypeInfo, Dictionary<string, IPropertyProxy>> PropertyColumnMaps = new(128);

    /// <summary>
    /// Gets a dictionary in which keys are the names of the columns and the values represent the property proxies.
    /// </summary>
    /// <param name="typeInfo">The type info to get the column map for.</param>
    /// <returns>The column map as a dictionary.</returns>
    public static IReadOnlyDictionary<string, IPropertyProxy> GetColumnMap(this ITypeInfo typeInfo)
    {
        lock (SyncLock)
        {
            if (PropertyColumnMaps.TryGetValue(typeInfo, out var map))
                return map;

            map = new(StringComparer.OrdinalIgnoreCase);
            foreach (var (propertyName, property) in typeInfo.Properties)
            {
                var fieldName = property.Attribute<ColumnAttribute>() is ColumnAttribute columnAttribute
                    && !string.IsNullOrWhiteSpace(columnAttribute.Name)
                        ? columnAttribute.Name
                        : propertyName.Contains('.', StringComparison.Ordinal)
                        ? propertyName.Split('.', StringSplitOptions.RemoveEmptyEntries)[^1]
                        : property.PropertyName;

                fieldName = fieldName.Trim();

                if (string.IsNullOrWhiteSpace(fieldName))
                    continue;

                if (map.ContainsKey(fieldName))
                    continue;

                map[fieldName] = property;
            }

            PropertyColumnMaps.Add(typeInfo, map);
            return map;
        }
    }
}

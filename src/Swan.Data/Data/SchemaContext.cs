namespace Swan.Data;

public class SchemaContext
{
    private readonly Dictionary<string, ColumnMetadata> _columns;

    public SchemaContext WithColumns<T>() => WithColumns(typeof(T));

    public SchemaContext WithColumns(Type t)
    {
        var properties = t.Properties();
        foreach (var p in properties)
        {
            if (!p.PropertyType.IsBasicType || !p.HasPublicGetter || p.PropertyName.Contains('.', StringComparison.Ordinal))
                continue;

            _columns[p.PropertyName] = new ColumnMetadata
            {
                AllowDBNull = p.PropertyType.IsNullable,
                ColumnName = p.PropertyName,
                DataType = p.PropertyType.BackingType.NativeType,
            };
        }

        return this;
    }

    public SchemaContext WithKey(string columnName)
    {
        _columns[columnName].IsKey = true;
        return this;
    }

    public SchemaContext WithAutoIncrement(string columnName)
    {
        _columns[columnName].IsAutoIncrement = true;
        return this;
    }
}


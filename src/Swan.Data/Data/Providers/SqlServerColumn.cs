#pragma warning disable CA1812
namespace Swan.Data.Providers;

internal record SqlServerColumn : IDbColumnSchema
{
    public bool? AllowDBNull { get; set; }

    public string? BaseCatalogName { get; set; }

    public string? BaseColumnName { get; set; }

    public string? BaseSchemaName { get; set; }

    public string? BaseServerName { get; set; }

    public string? BaseTableName { get; set; }

    public string? ColumnName { get; set; }

    public int? ColumnOrdinal { get; set; }

    public int? ColumnSize { get; set; }

    public Type? DataType { get; set; }

    public string? DataTypeName { get; set; }

    public int? IsAliased { get; set; }

    public bool? IsAutoIncrement { get; set; }

    public bool? IsColumnSet { get; set; }

    public bool? IsExpression { get; set; }

    public bool? IsHidden { get; set; }

    public bool? IsIdentity { get; set; }

    public bool? IsKey { get; set; }

    public bool? IsLong { get; set; }

    public bool? IsReadOnly { get; set; }

    public bool? IsRowVersion { get; set; }

    public bool? IsUnique { get; set; }

    public SqlDbType NonVersionedProviderType { get; set; }

    public byte? NumericPrecision { get; set; }

    public byte? NumericScale { get; set; }

    public string? ProviderSpecificDataType { get; set; }

    public SqlDbType? ProviderType { get; set; }

    public string? UdtAssemblyQualifiedName { get; set; }

    byte IDbColumnSchema.Precision => NumericPrecision.GetValueOrDefault() == byte.MaxValue ?
        byte.MinValue : NumericPrecision.GetValueOrDefault();

    byte IDbColumnSchema.Scale => NumericScale.GetValueOrDefault() == byte.MaxValue ?
        byte.MinValue : NumericScale.GetValueOrDefault();

    int IDbColumnSchema.MaxLength => ColumnSize.GetValueOrDefault();

    string IDbColumnSchema.Name => ColumnName ?? string.Empty;

    int IDbColumnSchema.Ordinal => ColumnOrdinal.GetValueOrDefault(-1);

    Type IDbColumnSchema.DataType => DataType ?? typeof(string);

    string IDbColumnSchema.ProviderDataType => DataTypeName ?? string.Empty;

    bool IDbColumnSchema.AllowsDBNull => AllowDBNull.GetValueOrDefault();

    bool IDbColumnSchema.IsKey => IsKey.GetValueOrDefault();

    bool IDbColumnSchema.IsAutoIncrement => IsAutoIncrement.GetValueOrDefault();

    bool IDbColumnSchema.IsReadOnly => IsReadOnly.GetValueOrDefault();

    string? IDbColumnSchema.IndexName => default;
}

#pragma warning restore CA1812

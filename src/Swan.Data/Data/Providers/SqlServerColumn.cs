#pragma warning disable CA1812
namespace Swan.Data.Providers;

[ExcludeFromCodeCoverage]
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

    public int? NumericPrecision { get; set; }

    public int? NumericScale { get; set; }

    public string? ProviderSpecificDataType { get; set; }

    public SqlDbType? ProviderType { get; set; }

    public string? UdtAssemblyQualifiedName { get; set; }

    int IDbColumnSchema.NumericPrecision => NumericPrecision.GetValueOrDefault(byte.MaxValue) == byte.MaxValue ? -1 : NumericPrecision ?? -1;

    int IDbColumnSchema.NumericScale => NumericScale.GetValueOrDefault(byte.MaxValue) == byte.MaxValue ? -1 : NumericScale ?? -1;

    int IDbColumnSchema.ColumnSize =>  ColumnSize ?? -1;

    string IDbColumnSchema.ColumnName => ColumnName ?? string.Empty;

    int IDbColumnSchema.ColumnOrdinal => ColumnOrdinal.GetValueOrDefault(-1);

    Type IDbColumnSchema.DataType => DataType ?? typeof(string);

    string IDbColumnSchema.DataTypeName => DataTypeName ?? string.Empty;

    string IDbColumnSchema.ProviderType => ProviderType.GetValueOrDefault(SqlDbType.NVarChar).ToStringInvariant();

    bool IDbColumnSchema.AllowDBNull => AllowDBNull.GetValueOrDefault();

    bool IDbColumnSchema.IsKey => IsKey.GetValueOrDefault();

    bool IDbColumnSchema.IsAutoIncrement => IsAutoIncrement.GetValueOrDefault();

    bool IDbColumnSchema.IsReadOnly => IsReadOnly.GetValueOrDefault();

    bool IDbColumnSchema.IsLong => IsLong.GetValueOrDefault();

    bool IDbColumnSchema.IsUnique => IsUnique.GetValueOrDefault();

    object ICloneable.Clone() => this with { };
}

#pragma warning restore CA1812

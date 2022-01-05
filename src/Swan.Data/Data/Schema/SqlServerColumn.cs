#pragma warning disable CA1812
namespace Swan.Data.Schema;

internal record SqlServerColumn : IDbColumn
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

    byte IDbColumn.Precision => NumericPrecision.GetValueOrDefault() == byte.MaxValue ?
        byte.MinValue : NumericPrecision.GetValueOrDefault();

    byte IDbColumn.Scale => NumericScale.GetValueOrDefault() == byte.MaxValue ?
        byte.MinValue : NumericScale.GetValueOrDefault();

    int IDbColumn.MaxLength => ColumnSize.GetValueOrDefault();

    string IDbColumn.Name => ColumnName ?? string.Empty;

    int IDbColumn.Ordinal => ColumnOrdinal.GetValueOrDefault(-1);

    Type IDbColumn.DataType => DataType ?? typeof(string);

    string IDbColumn.ProviderDataType => DataTypeName ?? string.Empty;

    bool IDbColumn.AllowsDBNull => AllowDBNull.GetValueOrDefault();

    bool IDbColumn.IsKey => IsKey.GetValueOrDefault();

    bool IDbColumn.IsAutoIncrement => IsAutoIncrement.GetValueOrDefault();

    bool IDbColumn.IsReadOnly => IsReadOnly.GetValueOrDefault();
}

#pragma warning restore CA1812

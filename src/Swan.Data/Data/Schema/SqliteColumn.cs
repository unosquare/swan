#pragma warning disable CA1812
namespace Swan.Data.Schema;

internal record SqliteColumn : IDbColumnSchema
{
    public bool? AllowDBNull { get; set; }

    public string? BaseCatalogName { get; set; }

    public string? BaseColumnName { get; set; }

    public string? BaseSchemaName { get; set; }

    public string? BaseTableName { get; set; }

    public string? ColumnName { get; set; }

    public int? ColumnOrdinal { get; set; }

    public int? ColumnSize { get; set; }

    public Type? DataType { get; set; }

    public string? DataTypeName { get; set; }

    public bool? IsAliased { get; set; }

    public bool? IsAutoIncrement { get; set; }

    public bool? IsExpression { get; set; }

    public bool? IsKey { get; set; }

    public bool? IsUnique { get; set; }

    public short? NumericPrecision { get; set; }

    public short? NumericScale { get; set; }

    string IDbColumnSchema.Name => ColumnName ?? string.Empty;

    int IDbColumnSchema.Ordinal => ColumnOrdinal.GetValueOrDefault(-1);

    Type IDbColumnSchema.DataType => DataType ?? typeof(string);

    string IDbColumnSchema.ProviderDataType => DataTypeName ?? string.Empty;

    bool IDbColumnSchema.AllowsDBNull => AllowDBNull.GetValueOrDefault();

    bool IDbColumnSchema.IsKey => IsKey.GetValueOrDefault();

    bool IDbColumnSchema.IsAutoIncrement => IsAutoIncrement.GetValueOrDefault();

    bool IDbColumnSchema.IsReadOnly => IsAutoIncrement.GetValueOrDefault();

    byte IDbColumnSchema.Precision => Convert.ToByte(NumericPrecision.GetValueOrDefault().ClampMin(0));

    byte IDbColumnSchema.Scale => Convert.ToByte(NumericScale.GetValueOrDefault().ClampMin(0));

    int IDbColumnSchema.MaxLength => ColumnSize.GetValueOrDefault();
}

#pragma warning restore CA1812

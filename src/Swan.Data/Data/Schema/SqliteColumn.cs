#pragma warning disable CA1812
namespace Swan.Data.Schema;

internal record SqliteColumn : IDbColumn
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

    string IDbColumn.Name => ColumnName ?? string.Empty;

    int IDbColumn.Ordinal => ColumnOrdinal.GetValueOrDefault(-1);

    Type IDbColumn.DataType => DataType ?? typeof(string);

    string IDbColumn.ProviderDataType => DataTypeName ?? string.Empty;

    bool IDbColumn.AllowsDBNull => AllowDBNull.GetValueOrDefault();

    bool IDbColumn.IsKey => IsKey.GetValueOrDefault();

    bool IDbColumn.IsAutoIncrement => IsAutoIncrement.GetValueOrDefault();

    bool IDbColumn.IsReadOnly => IsAutoIncrement.GetValueOrDefault();

    byte IDbColumn.Precision => Convert.ToByte(NumericPrecision.GetValueOrDefault().ClampMin(0));

    byte IDbColumn.Scale => Convert.ToByte(NumericScale.GetValueOrDefault().ClampMin(0));

    int IDbColumn.MaxLength => ColumnSize.GetValueOrDefault();
}

#pragma warning restore CA1812

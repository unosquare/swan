#pragma warning disable CA1812
namespace Swan.Data.Providers;

[ExcludeFromCodeCoverage]
internal record SqliteColumn : IDbColumnSchema
{
    public SqliteColumn()
    {
        // placeholder
    }

    public bool? AllowDBNull { get; set; }

    public string? BaseCatalogName { get; set; }

    public string? BaseColumnName { get; set; }

    public string? BaseSchemaName { get; set; }

    public string? BaseTableName { get; set; }

    public string? ColumnName { get; set; }

    public int? ColumnOrdinal { get; set; }

    public int? ColumnSize { get; set; }

    public Type? DataType { get; set; }

    public string? ProviderType { get; set; }

    public bool? IsAliased { get; set; }

    public bool? IsAutoIncrement { get; set; }

    public bool? IsExpression { get; set; }

    public bool? IsKey { get; set; }

    public bool? IsUnique { get; set; }

    public bool? IsLong { get; set; }

    public short? NumericPrecision { get; set; }

    public short? NumericScale { get; set; }

    string IDbColumnSchema.ColumnName => ColumnName ?? string.Empty;

    int IDbColumnSchema.ColumnOrdinal => ColumnOrdinal.GetValueOrDefault();

    Type IDbColumnSchema.DataType => DataType ?? typeof(string);

    string IDbColumnSchema.ProviderType => ProviderType ?? string.Empty;

    bool IDbColumnSchema.AllowDBNull => AllowDBNull.GetValueOrDefault();

    bool IDbColumnSchema.IsKey => IsKey.GetValueOrDefault();

    bool IDbColumnSchema.IsAutoIncrement => IsAutoIncrement.GetValueOrDefault() ||
        (IsKey.GetValueOrDefault() && (ProviderType ?? string.Empty).ToUpperInvariant().Equals("INTEGER", StringComparison.Ordinal));

    bool IDbColumnSchema.IsReadOnly => IsAutoIncrement.GetValueOrDefault();

    int IDbColumnSchema.ColumnSize => ColumnSize.GetValueOrDefault();

    int IDbColumnSchema.NumericPrecision => Convert.ToInt32(NumericPrecision.GetValueOrDefault().ClampMin(0));

    int IDbColumnSchema.NumericScale => Convert.ToInt32(NumericScale.GetValueOrDefault().ClampMin(0));

    bool IDbColumnSchema.IsLong => IsLong.GetValueOrDefault();

    bool IDbColumnSchema.IsUnique => IsUnique.GetValueOrDefault();

    object ICloneable.Clone() => this with { };
}

#pragma warning restore CA1812

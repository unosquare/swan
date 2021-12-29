namespace Swan.Data;

/// <summary>
/// Provides column schema information.
/// </summary>
public record ColumnMetadata
{
    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string ColumnName { get; internal set; }
    
    /// <summary>
    /// Gets the column ordinal (column position within the table).
    /// </summary>
    public int ColumnOrdinal { get; internal set; }
    
    /// <summary>
    /// Gets the byte size of the column.
    /// </summary>
    public int ColumnSize { get; internal set; }
    
    /// <summary>
    /// For floating point columns, gets the column precision.
    /// </summary>
    public int NumericPrecision { get; internal set; }
    
    /// <summary>
    /// For floating point columns, gets the column scale.
    /// </summary>
    public int NumericScale { get; internal set; }
    
    public bool IsUnique { get; internal set; }
    
    public bool IsKey { get; internal set; }
    
    public Type DataType { get; internal set; }
    
    public bool AllowDBNull { get; internal set; }
    
    public int ProviderType { get; internal set; }
    
    public bool IsAliased { get; internal set; }
    
    public bool IsExpression { get; internal set; }
    
    public bool IsIdentity { get; internal set; }
    
    public bool IsAutoIncrement { get; internal set; }
    
    public bool IsRowVersion { get; internal set; }
    
    public bool IsHidden { get; internal set; }
    
    public bool IsLong { get; internal set; }
    
    public bool IsReadOnly { get; internal set; }
    
    public Type ProviderSpecificDataType { get; internal set; }
    
    public string DataTypeName { get; internal set; }
    
    public bool IsColumnSet { get; internal set; }

    public int NonVersionedProviderType { get; internal set; }
}

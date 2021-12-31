﻿namespace Swan.Data;

/// <summary>
/// Provides column schema information.
/// </summary>
public record DbColumn
{
    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string? ColumnName { get; set; }
    
    /// <summary>
    /// Gets the column ordinal (column position within the table).
    /// </summary>
    public int ColumnOrdinal { get; set; }
    
    /// <summary>
    /// Gets the name of the provider-specific data type.
    /// </summary>
    public string? ProviderDataType { get; internal set; }
    
    /// <summary>
    /// Gets whether column values accept null values.
    /// </summary>
    public bool AllowsDBNull { get; set; }

    /// <summary>
    /// Gets whether this column is part of the primary key.
    /// </summary>
    public bool IsKey { get; internal set; }

    /// <summary>
    /// Gets whether the column is automatically incremented.
    /// </summary>
    public bool IsAutoIncrement { get; internal set; }
   
    
    /// <summary>
    /// Gets whether the column is an expression, autoincremental, or
    /// automatically generated by the database.
    /// </summary>
    public bool IsReadOnly { get; internal set; }
}

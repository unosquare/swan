namespace Swan.Data.Schema;

internal sealed record DbColumnSchema : IDbColumnSchema
{
    public string Name { get; set; }

    public int Ordinal { get; set; }

    public Type DataType { get; set; }

    public string ProviderDataType { get; set; }

    public bool AllowsDBNull { get; set; }

    public bool IsKey { get; set; }

    public bool IsAutoIncrement { get; set; }

    public bool IsReadOnly { get; set; }

    public byte Precision { get; set; }

    public byte Scale {get;set;}

    public int MaxLength { get; set; }

    public string? IndexName { get; set; }
}

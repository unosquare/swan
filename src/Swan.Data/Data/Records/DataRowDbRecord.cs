namespace Swan.Data.Records;

using Swan.Data;

/// <summary>
/// Provides a <see cref="IDataRecord"/> wrapper for a <see cref="DataRow"/>.
/// </summary>
internal sealed class DataRowDbRecord : IDataRecord
{
    // TODO: Complete method implementation
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private readonly DataRow Row;

    /// <summary>
    /// Creates a new instance of the <see cref="DataRowDbRecord"/>.
    /// </summary>
    /// <param name="row">The row to wrap.</param>
    public DataRowDbRecord(DataRow row)
    {
        if (row is null)
            throw new ArgumentNullException(nameof(row));

        Row = row;
    }

    /// <inheritdoc />
    public object this[int i] => Row[i];

    /// <inheritdoc />
    public object this[string name] => Row[name];

    /// <inheritdoc />
    public int FieldCount => Row.Table.Columns.Count;

    /// <inheritdoc />
    public IDataReader GetData(int i) => throw new NotSupportedException();

    /// <inheritdoc />
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) =>
        this.GetBytesIntoArray(i, fieldOffset, buffer, bufferoffset, length);

    /// <inheritdoc />
    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) =>
        this.GetCharsIntoArray(i, fieldoffset, buffer, bufferoffset, length);

    /// <inheritdoc />
    public string GetDataTypeName(int i) => Row.Table.Columns[i].DataType.Name;

    /// <inheritdoc />
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public Type GetFieldType(int i) => Row.Table.Columns[i].DataType;

    /// <inheritdoc />
    public string GetName(int i) => Row.Table.Columns[i].ColumnName;

    /// <inheritdoc />
    public int GetOrdinal(string name) => Row.Table.Columns[name]?.Ordinal ?? throw new ArgumentOutOfRangeException(nameof(name));

    /// <inheritdoc />
    public bool IsDBNull(int i) => Row[i] is null || Convert.IsDBNull(Row[i]);

    /// <inheritdoc />
    public bool GetBoolean(int i) => Row[i] is bool value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToBoolean(Row[i], Invariant);

    /// <inheritdoc />
    public byte GetByte(int i) => Row[i] is byte value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToByte(Row[i], Invariant);

    public char GetChar(int i) => Row[i] is char value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToChar(Row[i], Invariant);

    public DateTime GetDateTime(int i) => Row[i] is DateTime value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToDateTime(Row[i], Invariant);

    public decimal GetDecimal(int i) => Row[i] is decimal value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToDecimal(Row[i], Invariant);

    public double GetDouble(int i) => Row[i] is double value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToDouble(Row[i], Invariant);

    public float GetFloat(int i) => Row[i] is float value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToSingle(Row[i], Invariant);

    public Guid GetGuid(int i) => Row[i] is Guid value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Guid.Empty;

    public short GetInt16(int i) => Row[i] is short value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToInt16(Row[i], Invariant);

    public int GetInt32(int i) => Row[i] is int value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToInt32(Row[i], Invariant);

    public long GetInt64(int i) => Row[i] is long value || TypeManager.TryChangeType(Row[i], out value)
        ? value : Convert.ToInt64(Row[i], Invariant);

    public string GetString(int i) => Row[i] is string value
        ? value : Convert.ToString(Row[i], Invariant) ?? default!;

    public object GetValue(int i) => Row[i];

    public int GetValues(object[] values) => this.GetValuesIntoArray(values);
}

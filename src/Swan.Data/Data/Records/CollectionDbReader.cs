namespace Swan.Data.Records;

using Data;

/// <summary>
/// Represents a DataReader that was derived from a collection.
/// </summary>
internal interface ICollectionDbReader : IDataReader
{
    /// <summary>
    /// Gets the object currently being pointed at by the data reader.
    /// </summary>
    object? Current { get; }
}

/// <summary>
/// Uses an <see cref="IEnumerable"/> set, and an <see cref="IDbTableSchema"/>
/// to be used as an <see cref="IDataReader"/>.
/// </summary>
internal sealed class CollectionDbReader : ICollectionDbReader
{
    private const string ReaderNotReadyMessage = "The reader is either closed, past the end of the last record, or has not read any records.";
    private readonly IDbTableSchema Schema;
    private readonly IEnumerator Enumerator;
    private bool ReadState;
    private ITypeInfo? ItemTypeInfo;

    /// <summary>
    /// Creates a new instance of the <see cref="CollectionDbReader"/> class.
    /// </summary>
    /// <param name="enumerator">The enumerator.</param>
    /// <param name="schema">The schema for the data reader.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CollectionDbReader(IEnumerator enumerator, IDbTableSchema schema)
    {
        Enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CollectionDbReader"/> class.
    /// </summary>
    /// <param name="enumerator">The enumerator.</param>
    /// <param name="itemType">The item type which produces a basic schema for the data reader.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CollectionDbReader(IEnumerator enumerator, Type itemType)
    {
        if (itemType is null)
            throw new ArgumentNullException(nameof(itemType));

        Enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        Schema = itemType.ToTableSchema();

        // TODO: Map Column Attribute to schema.
        ItemTypeInfo = itemType.TypeInfo();
    }


    #region Indexer Properties

    /// <inheritdoc />
    public object this[int i] => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty(Current!, Schema[i]!.ColumnName, out var value)
            ? value ?? DBNull.Value
            : DBNull.Value;

    /// <inheritdoc />
    public object this[string name] => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty(Current!, name, out var value)
            ? value ?? DBNull.Value
            : DBNull.Value;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int FieldCount => Schema.ColumnCount;

    /// <inheritdoc />
    public int Depth => 0;

    /// <inheritdoc />
    public bool IsClosed { get; private set; }

    /// <inheritdoc />
    public int RecordsAffected => -1;

    /// <inheritdoc />
    public object? Current { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a record is current and ready to be read.
    /// </summary>
    private bool HasCurrentRecord => !IsClosed && ReadState && Current is not null;

    #endregion

    #region Table Metadata

    /// <inheritdoc />
    public string GetDataTypeName(int i) => Schema[i]!.ProviderType;

    /// <inheritdoc />
    public string GetName(int i) => Schema[i]!.ColumnName;

    /// <inheritdoc />
    public int GetOrdinal(string name) => Schema.GetColumnOrdinal(name);

    /// <inheritdoc />
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public Type GetFieldType(int i) => Schema[i]!.DataType!;

    /// <inheritdoc />
    public DataTable? GetSchemaTable() => Schema.ToSchemaTable();

    #endregion

    #region Special Datum Getters

    /// <inheritdoc />
    public bool IsDBNull(int i) => GetValue(i) is null || Convert.IsDBNull(GetValue(i));

    /// <inheritdoc />
    public int GetValues(object[] values) => this.GetValuesIntoArray(values);

    /// <inheritdoc />
    public long GetChars(int i, long fieldOffset, char[]? buffer, int bufferoffset, int length) =>
        this.GetCharsIntoArray(i, fieldOffset, buffer, bufferoffset, length);

    /// <inheritdoc />
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) =>
        this.GetBytesIntoArray(i, fieldOffset, buffer, bufferoffset, length);

    /// <inheritdoc />
    public IDataReader GetData(int i) => throw new NotSupportedException();

    #endregion

    #region Standard Datum Getters

    /// <inheritdoc />
    public object GetValue(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty(Current!, Schema[i]!.ColumnName, out var value) ? value ?? DBNull.Value : DBNull.Value;

    /// <inheritdoc />
    public bool GetBoolean(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<bool>(Current!, Schema[i]!.ColumnName, out var value) && value;

    /// <inheritdoc />
    public byte GetByte(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<byte>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public char GetChar(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<char>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public DateTime GetDateTime(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<DateTime>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public decimal GetDecimal(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<decimal>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public double GetDouble(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<double>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public float GetFloat(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<float>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public Guid GetGuid(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<Guid>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public short GetInt16(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<short>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public int GetInt32(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<int>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public long GetInt64(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<long>(Current!, Schema[i]!.ColumnName, out var value) ? value : default;

    /// <inheritdoc />
    public string GetString(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<string>(Current!, Schema[i]!.ColumnName, out var value) ? value! : null!;

    #endregion

    /// <inheritdoc />
    public bool NextResult() => false;

    /// <inheritdoc />
    public bool Read()
    {
        if (IsClosed)
            return false;

        if (!Enumerator.MoveNext())
        {
            Close();
            return false;
        }

        Current = Enumerator.Current;
        if (Current is null)
            return Read();

        ItemTypeInfo ??= Current.GetType().TypeInfo();
        return ReadState = true;
    }

    /// <inheritdoc />
    public void Close()
    {
        ReadState = false;
        IsClosed = true;
        Current = null;
        if (Enumerator is IDisposable disposable)
            disposable.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks to release resources.
    /// When called with also managed, it will close the reader.
    /// </summary>
    /// <param name="alsoManaged">When set to true, closes this reader.</param>
    private void Dispose(bool alsoManaged)
    {
        if (IsClosed)
            return;

        if (alsoManaged)
            Close();
    }
}

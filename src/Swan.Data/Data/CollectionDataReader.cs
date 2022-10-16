namespace Swan.Data;

using Swan.Data.Extensions;
using System.Collections;

/// <summary>
/// Represents a DataReader that was derived from a collection.
/// </summary>
public interface ICollectionDataReader : IDataReader
{
    /// <summary>
    /// Gets the object currently being pointed at by the data reader.
    /// </summary>
    object? CurrentRecord { get; }
}

/// <summary>
/// Uses an <see cref="IEnumerable"/> set, and an <see cref="IDbTableSchema"/>
/// to be used as an <see cref="IDataReader"/>.
/// </summary>
internal class CollectionDataReader : ICollectionDataReader
{
    private const string ReaderNotReadyMessage = "The reader is either closed, past the end of the last record, or has not read any records.";
    private readonly IDbTableSchema Schema;
    private readonly IEnumerator Enumerator;
    private bool ReadState;
    private ITypeInfo? ItemTypeInfo;
    
    /// <summary>
    /// Creates a new instance of the <see cref="CollectionDataReader"/> class.
    /// </summary>
    /// <param name="enumerator">The enumerator.</param>
    /// <param name="schema">The schema for the data reader.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CollectionDataReader(IEnumerator enumerator, IDbTableSchema schema)
    {
        if (enumerator is null)
            throw new ArgumentNullException(nameof(enumerator));

        if (schema is null)
            throw new ArgumentNullException(nameof(schema));

        Enumerator = enumerator;
        Schema = schema;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CollectionDataReader"/> class.
    /// </summary>
    /// <param name="enumerator">The enumerator.</param>
    /// <param name="itemType">The item type which produces a basic schema for the data reader.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CollectionDataReader(IEnumerator enumerator, Type itemType)
    {
        if (enumerator is null)
            throw new ArgumentNullException(nameof(enumerator));

        if (itemType is null)
            throw new ArgumentNullException(nameof(itemType));

        Enumerator = enumerator;
        Schema = itemType.ToTableSchema();
        ItemTypeInfo = itemType.TypeInfo();
    }


    #region Indexer Properties

    /// <inheritdoc />
    public object this[int i] => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty(CurrentRecord!, Schema[i]!.Name, out var value)
            ? value ?? DBNull.Value
            : DBNull.Value;

    /// <inheritdoc />
    public object this[string name] => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty(CurrentRecord!, name, out var value)
            ? value ?? DBNull.Value
            : DBNull.Value;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int FieldCount => Schema.ColumnCount;

    /// <inheritdoc />
    public int Depth => 0;

    /// <inheritdoc />
    public bool IsClosed { get; protected set; }

    /// <inheritdoc />
    public int RecordsAffected => -1;

    /// <inheritdoc />
    public virtual object? CurrentRecord { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether a record is current and ready to be read.
    /// </summary>
    protected virtual bool HasCurrentRecord => !IsClosed && ReadState && CurrentRecord is not null;

    #endregion

    #region Table Metadata

    /// <inheritdoc />
    public string GetDataTypeName(int i) => Schema[i]!.ProviderDataType;

    /// <inheritdoc />
    public string GetName(int i) => Schema[i]!.Name;

    /// <inheritdoc />
    public int GetOrdinal(string name) => Schema.GetColumnIndex(name);

    /// <inheritdoc />
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public Type GetFieldType(int i) => Schema[i]!.DataType!;

    /// <inheritdoc />
    public DataTable? GetSchemaTable() => Schema.ToSchemaTable();
    
    #endregion

    #region Special Datum Getters

    /// <inheritdoc />
    public bool IsDBNull(int i) => GetValue(i) == DBNull.Value;

    /// <inheritdoc />
    public int GetValues(object[] values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        var count = 0;

        for (var i = 0; i < Math.Min(values.Length, Schema.ColumnCount); i++)
        {
            values[i] = GetValue(i);
            count++;
        }

        return count;
    }

    /// <inheritdoc />
    public long GetChars(int i, long fieldOffset, char[]? buffer, int bufferoffset, int length)
    {
        if (IsDBNull(i))
            return 0;

        if (GetFieldType(i) != typeof(string))
            throw new InvalidOperationException("Cannot get chars because field is not of string type.");

        var sourceSpan = GetString(i).AsSpan();
        var charsRead = 0L;
        var targetOffset = bufferoffset;
        for (var sourceOffset = (int)fieldOffset; sourceOffset < sourceSpan.Length; sourceOffset++)
        {
            if (buffer is not null)
                buffer[bufferoffset] = sourceSpan[sourceOffset];

            targetOffset++;
            charsRead++;
        }

        return charsRead;
    }

    /// <inheritdoc />
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
    {
        if (IsDBNull(i))
            return 0;

        if (GetFieldType(i) != typeof(byte[]))
            throw new InvalidOperationException("Cannot get bytes because field is not a byte array.");

        var sourceSpan = (GetValue(i) as byte[]).AsSpan();
        var bytesRead = 0L;
        var targetOffset = bufferoffset;
        for (var sourceOffset = (int)fieldOffset; sourceOffset < sourceSpan.Length; sourceOffset++)
        {
            if (buffer is not null)
                buffer[bufferoffset] = sourceSpan[sourceOffset];

            targetOffset++;
            bytesRead++;
        }

        return bytesRead;
    }

    /// <inheritdoc />
    public IDataReader GetData(int i) => throw new NotSupportedException();

    #endregion

    #region Standard Datum Getters

    /// <inheritdoc />
    public object GetValue(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty(CurrentRecord!, Schema[i]!.Name, out var value) ? value ?? DBNull.Value : DBNull.Value;

    /// <inheritdoc />
    public bool GetBoolean(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<bool>(CurrentRecord!, Schema[i]!.Name, out var value) && value;

    /// <inheritdoc />
    public byte GetByte(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<byte>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public char GetChar(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<char>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public DateTime GetDateTime(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<DateTime>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public decimal GetDecimal(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<decimal>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public double GetDouble(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<double>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public float GetFloat(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<float>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public Guid GetGuid(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<Guid>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public short GetInt16(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<short>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public int GetInt32(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<int>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public long GetInt64(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<long>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

    /// <inheritdoc />
    public string GetString(int i) => !HasCurrentRecord
        ? throw new InvalidOperationException(ReaderNotReadyMessage)
        : ItemTypeInfo!.TryReadProperty<string>(CurrentRecord!, Schema[i]!.Name, out var value) ? value! : null!;

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

        CurrentRecord = Enumerator.Current;
        if (CurrentRecord is null)
            return Read();

        ItemTypeInfo ??= CurrentRecord.GetType().TypeInfo();
        return ReadState = true;
    }

    /// <inheritdoc />
    public void Close()
    {
        ReadState = false;
        IsClosed = true;
        CurrentRecord = null;
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
    protected virtual void Dispose(bool alsoManaged)
    {
        if (IsClosed)
            return;

        if (alsoManaged)
            Close();
    }
}

/// <summary>
/// Represents a DataReader that was derived from a collection.
/// </summary>
public interface ICollectionDataReader<T> : ICollectionDataReader
{
    /// <summary>
    /// Gets the object currently being pointed at by the data reader.
    /// </summary>
    T? CurrenRecord { get; }
}

/// <summary>
/// Represents a strongly-typed instance of a <see cref="CollectionDataReader"/> class.
/// </summary>
/// <typeparam name="T">The type of the collection's items.</typeparam>
internal class CollectionDataReader<T> : CollectionDataReader, ICollectionDataReader<T>
{
    public CollectionDataReader(IEnumerator<T> enumerator, IDbTableSchema schema) :
        base(enumerator, schema)
    {
        // placeholder
    }

    public CollectionDataReader(IEnumerator<T> enumerator) :
        base(enumerator, typeof(T))
    {
        // placeholder
    }

    /// <inheritdoc />
    public T? CurrenRecord => base.CurrentRecord is T value ? value : default;
}

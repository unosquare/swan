namespace Swan.Data.Extensions;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides default API methods to parse rows or records into objects of different types.
/// </summary>
public static class DataParserExtensions
{
    #region IDataRecord Parsers

    /// <summary>
    /// Reads an object from the underlying record at the current position.
    /// </summary>
    /// <param name="record">The reader to read from.</param>
    /// <param name="t">The type of object this method returns.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static object ParseObject(this IDataRecord record, Type t, Func<object>? typeFactory = default)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        if (t is null)
            throw new ArgumentNullException(nameof(t));

        var typeInfo = t.TypeInfo();
        typeFactory ??= () => typeInfo.CreateInstance();
        var result = typeFactory.Invoke();

        var fieldNames = new Dictionary<string, int>(record.FieldCount, StringComparer.Ordinal);
        for (var i = 0; i < record.FieldCount; i++)
            fieldNames[record.GetName(i)] = i;

        foreach (var (fieldName, columnIndex) in fieldNames)
        {
            if (!typeInfo.TryFindProperty(fieldName, out var property) ||
                property is null || !property.CanWrite || !property.HasPublicSetter)
                continue;


            if (record.IsDBNull(columnIndex))
            {
                property.Write(result, property.PropertyType.DefaultValue);
                continue;
            }

            var fieldValue = record.GetValue(columnIndex);
            if (!property.TryWrite(result, fieldValue))
                throw new InvalidCastException(
                $"Unable to convert value for field '{fieldName}' of type '{fieldValue.GetType().Name}' to '{property.PropertyType}'");
        }

        return result;
    }

    /// <summary>
    /// Reads an object from the underlying record at the current position.
    /// </summary>
    /// <typeparam name="T">The type of object this method returns.</typeparam>
    /// <param name="record">The reader to read from.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static T ParseObject<T>(this IDataRecord record, Func<T>? typeFactory = default) =>
        (T)record.ParseObject(typeof(T), typeFactory is null ? default : () => typeFactory.Invoke()!);

    /// <summary>
    /// Reads a dynamically typed object from the underlying record at the current position.
    /// </summary>
    /// <param name="record">The data record to read from</param>
    /// <returns>A dynamic <see cref="ExpandoObject"/></returns>
    public static ExpandoObject ParseExpando(this IDataRecord record)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        var result = new ExpandoObject();

        for (var i = 0; i < record.FieldCount; i++)
        {
            var fieldName = record.GetName(i);
            var fieldValue = record.IsDBNull(i) ? null : record.GetValue(i);
            var propertyName = fieldName.ToExpandoPropertyName(i);

            result.TryAdd(propertyName, fieldValue);
        }

        return result;
    }

    #endregion

    #region DataRow Parsers

    /// <summary>
    /// Reads an object from the underlying row.
    /// </summary>
    /// <param name="row">The data row to convert.</param>
    /// <param name="t">The type of object this method returns.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static object ParseObject(this DataRow row, Type t, Func<object>? typeFactory = default)
    {
        if (row is null)
            throw new ArgumentNullException(nameof(row));

        if (t is null)
            throw new ArgumentNullException(nameof(t));

        var typeInfo = t.TypeInfo();
        typeFactory ??= () => typeInfo.CreateInstance();
        var result = typeFactory.Invoke();

        var fieldNames = new Dictionary<string, int>(row.Table.Columns.Count, StringComparer.Ordinal);
        for (var i = 0; i < row.Table.Columns.Count; i++)
            fieldNames[row.Table.Columns[i].ColumnName] = i;

        foreach (var (fieldName, columnIndex) in fieldNames)
        {
            if (!typeInfo.TryFindProperty(fieldName, out var property) ||
                property is null || !property.CanWrite || !property.HasPublicSetter)
                continue;

            if (row[columnIndex] == DBNull.Value)
            {
                property.Write(result, property.PropertyType.DefaultValue);
                continue;
            }

            var fieldValue = row[columnIndex];
            if (!property.TryWrite(result, fieldValue))
                throw new InvalidCastException(
                $"Unable to convert value for field '{fieldName}' of type '{fieldValue.GetType().Name}' to '{property.PropertyType}'");

        }

        return result;
    }

    /// <summary>
    /// Reads an object from the underlying row.
    /// </summary>
    /// <typeparam name="T">The type of object this method returns.</typeparam>
    /// <param name="row">The row to read from.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the row.</returns>
    public static T ParseObject<T>(this DataRow row, Func<T>? typeFactory = default) =>
        (T)row.ParseObject(typeof(T), typeFactory is null ? default : () => typeFactory.Invoke()!);

    /// <summary>
    /// Reads a dynamically typed object from the underlying row.
    /// </summary>
    /// <param name="row">The data row to read from.</param>
    /// <returns>A dynamic <see cref="ExpandoObject"/></returns>
    public static ExpandoObject ParseExpando(this DataRow row)
    {
        if (row is null)
            throw new ArgumentNullException(nameof(row));

        var result = new ExpandoObject();

        for (var i = 0; i < row.Table.Columns.Count; i++)
        {
            var fieldName = row.Table.Columns[i].ColumnName;
            var fieldValue = row[i] == DBNull.Value ? null : row[i];
            var propertyName = fieldName.ToExpandoPropertyName(i);

            result.TryAdd(propertyName, fieldValue);
        }

        return result;
    }

    #endregion

    #region IDataRecord Converters

    private class CollectionDataReader : DbDataReader
    {
        private const string ReaderNotReadyMessage = "The reader is either closed, past the end of the last record, or has not read any records.";
        private readonly IEnumerable Collection;
        private readonly IDbTableSchema Schema;

        private readonly IEnumerator Enumerator;

        private bool isClosed;
        private bool readState;
        private ITypeInfo? ItemTypeInfo;
        private object? CurrentRecord;


        public CollectionDataReader(IEnumerable collection, IDbTableSchema schema)
        {
            Collection = collection;
            Enumerator = collection.GetEnumerator();
            Schema = schema;
        }

        #region Indexer Properties

        /// <inheritdoc />
        public override object this[int i] => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty(CurrentRecord!, Schema[i]!.Name, out var value)
                ? value ?? DBNull.Value
                : DBNull.Value;

        /// <inheritdoc />
        public override object this[string name] => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty(CurrentRecord!, name, out var value)
                ? value ?? DBNull.Value
                : DBNull.Value;

        #endregion

        #region Properties

        /// <inheritdoc />
        public override int FieldCount => Schema.ColumnCount;

        /// <inheritdoc />
        public override int Depth => 0;

        /// <inheritdoc />
        public override bool HasRows => throw new NotImplementedException();

        /// <inheritdoc />
        public override bool IsClosed => isClosed;

        /// <inheritdoc />
        public override int RecordsAffected => -1;

        /// <summary>
        /// Gets a value indicating whether a record is current and ready to be read.
        /// </summary>
        private bool HasCurrentRecord => !isClosed && readState && CurrentRecord is not null;

        #endregion

        #region Table Metadata

        /// <inheritdoc />
        public override string GetDataTypeName(int i) => Schema[i]!.ProviderDataType;

        /// <inheritdoc />
        public override string GetName(int i) => Schema[i]!.Name;

        /// <inheritdoc />
        public override int GetOrdinal(string name) => Schema.GetColumnIndex(name);

        /// <inheritdoc />
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public override Type GetFieldType(int i) => Schema[i]!.DataType!;

        #endregion

        #region Special Datum Getters

        /// <inheritdoc />
        public override bool IsDBNull(int i) => GetValue(i) == DBNull.Value;

        /// <inheritdoc />
        public override int GetValues(object[] values)
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
        public override long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();

        /// <inheritdoc />
        public override long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();

        #endregion

        #region Standard Datum Getters

        /// <inheritdoc />
        public override object GetValue(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty(CurrentRecord!, Schema[i]!.Name, out var value) ? value ?? DBNull.Value : DBNull.Value;

        /// <inheritdoc />
        public override bool GetBoolean(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<bool>(CurrentRecord!, Schema[i]!.Name, out var value) && value;

        /// <inheritdoc />
        public override byte GetByte(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<byte>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override char GetChar(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<char>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override DateTime GetDateTime(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<DateTime>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override decimal GetDecimal(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<decimal>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override double GetDouble(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<double>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override float GetFloat(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<float>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override Guid GetGuid(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<Guid>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override short GetInt16(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<short>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override int GetInt32(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<int>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override long GetInt64(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<long>(CurrentRecord!, Schema[i]!.Name, out var value) ? value : default;

        /// <inheritdoc />
        public override string GetString(int i) => !HasCurrentRecord
            ? throw new InvalidOperationException(ReaderNotReadyMessage)
            : ItemTypeInfo!.TryReadProperty<string>(CurrentRecord!, Schema[i]!.Name, out var value) ? value! : null!;

        #endregion

        /// <inheritdoc />
        public override IEnumerator GetEnumerator() => Collection.GetEnumerator();

        /// <inheritdoc />
        public override bool NextResult() => false;

        /// <inheritdoc />
        public override bool Read()
        {
            if (isClosed)
                return false;

            if (!Enumerator.MoveNext())
            {
                readState = false;
                isClosed = true;
            }

            CurrentRecord = Enumerator.Current;
            if (CurrentRecord is null)
                return Read();

            ItemTypeInfo ??= CurrentRecord.GetType().TypeInfo();

            return readState = true;
        }

        /// <inheritdoc />
        public override void Close()
        {
            isClosed = true;
            base.Close();
        }

        override 
    }

    #endregion
}


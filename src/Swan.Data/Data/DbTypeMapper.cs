namespace Swan.Data;

public class DbTypeMapper
{
    private static readonly IReadOnlyDictionary<Type, DbType> DefaultTypeMap = new Dictionary<Type, DbType>(64)
    {
        [typeof(byte)] = DbType.Byte,
        [typeof(sbyte)] = DbType.SByte,
        [typeof(short)] = DbType.Int16,
        [typeof(ushort)] = DbType.UInt16,
        [typeof(int)] = DbType.Int32,
        [typeof(uint)] = DbType.UInt32,
        [typeof(long)] = DbType.Int64,
        [typeof(ulong)] = DbType.UInt64,
        [typeof(float)] = DbType.Single,
        [typeof(double)] = DbType.Double,
        [typeof(decimal)] = DbType.Decimal,
        [typeof(bool)] = DbType.Boolean,
        [typeof(string)] = DbType.String,
        [typeof(char)] = DbType.StringFixedLength,
        [typeof(Guid)] = DbType.Guid,
        [typeof(DateTime)] = DbType.DateTime2,
        [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
        [typeof(byte[])] = DbType.Binary,
        [typeof(byte?)] = DbType.Byte,
        [typeof(sbyte?)] = DbType.SByte,
        [typeof(short?)] = DbType.Int16,
        [typeof(ushort?)] = DbType.UInt16,
        [typeof(int?)] = DbType.Int32,
        [typeof(uint?)] = DbType.UInt32,
        [typeof(long?)] = DbType.Int64,
        [typeof(ulong?)] = DbType.UInt64,
        [typeof(float?)] = DbType.Single,
        [typeof(double?)] = DbType.Double,
        [typeof(decimal?)] = DbType.Decimal,
        [typeof(bool?)] = DbType.Boolean,
        [typeof(char?)] = DbType.StringFixedLength,
        [typeof(Guid?)] = DbType.Guid,
        [typeof(DateTime?)] = DbType.DateTime2,
        [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
        [typeof(object)] = DbType.Object
    };

    private readonly Dictionary<Type, DbType> TypeMap;

    public DbTypeMapper()
    {
        TypeMap = new(DefaultTypeMap);
    }

    public static DbTypeMapper Default { get; } = new();

    public bool TryGetDbTypeFor(Type type, [MaybeNullWhen(false)] out DbType? dbType)
    {
        dbType = default;

        if (type != null && TypeMap.TryGetValue(type, out var mappedType))
        {
            dbType = mappedType;
            return true;
        }

        return false;
    }

    public bool TryGetDbTypeFor<T>(T? value, out DbType? dbType) =>
        TryGetDbTypeFor(typeof(T), out dbType);

}

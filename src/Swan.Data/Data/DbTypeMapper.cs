namespace Swan.Data;

/// <summary>
/// Represents a mapper that is able to resolve types between
/// a database provider and CLR types.
/// </summary>
internal abstract class DbTypeMapper : IDbTypeMapper
{
    protected readonly Dictionary<Type, DbType> DbTypeMap = new(64)
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
    };

    protected readonly Dictionary<Type, string> ProviderTypeMap = new(64)
    {
        [typeof(byte)] = "TINYINT",
        [typeof(sbyte)] = "SMALLINT",
        [typeof(short)] = "SMALLINT",
        [typeof(ushort)] = "INT",
        [typeof(int)] = "INT",
        [typeof(uint)] = "BIGINT",
        [typeof(long)] = "BIGINT",
        [typeof(ulong)] = "DECIMAL(20)",
        [typeof(float)] = "REAL",
        [typeof(double)] = "FLOAT",
        [typeof(decimal)] = "DECIMAL(29,4)",
        [typeof(bool)] = "BIT",
        [typeof(string)] = "NVARCHAR(512)",
        [typeof(char)] = "NCHAR(1)",
        [typeof(Guid)] = "UNIQUEIDENTIFIER",
        [typeof(DateTime)] = "DATETIME2",
        [typeof(DateTimeOffset)] = "DATETIMEOFFSET",
        [typeof(byte[])] = "VARBINARY(MAX)",
    };

    /// <summary>
    /// Creates a new instance of the <see cref="DbTypeMapper"/> class.
    /// </summary>
    protected DbTypeMapper()
    {
        // placeholder
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<Type> SupportedTypes => DbTypeMap.Keys.ToArray();

    /// <summary>
    /// Gets a default type mapper for the given provider type.
    /// </summary>
    /// <param name="kind">The provider kind.</param>
    /// <returns>An suitable type mapper for the given proivider type.</returns>
    public static IDbTypeMapper GetDefault(ProviderKind kind)
    {
        return kind switch
        {
            ProviderKind.MySql => new MySqlTypeMapper(),
            ProviderKind.SqlServer => new SqlServerTypeMapper(),
            ProviderKind.Sqlite => new SqliteTypeMapper(),
            _ => throw new NotSupportedException(),
        };
    }

    /// <inheritdoc />
    public virtual bool TryGetDbTypeFor(Type type, [MaybeNullWhen(false)] out DbType? dbType)
    {
        dbType = default;
        type = type.TypeInfo().BackingType.NativeType;

        if (type is not null && DbTypeMap.TryGetValue(type, out var mappedType))
        {
            dbType = mappedType;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public virtual bool TryGetProviderTypeFor(Type type, [MaybeNullWhen(false)] out string? providerType)
    {
        providerType = default;
        type = type.TypeInfo().BackingType.NativeType;

        if (type is not null && ProviderTypeMap.TryGetValue(type, out var mappedType))
        {
            providerType = mappedType;
            return true;
        }

        return false;
    }
}

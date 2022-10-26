namespace Swan.Data;

/// <summary>
/// Represents a mapper that is able to resolve types between
/// a database provider and CLR types.
/// </summary>
internal class DbTypeMapper : IDbTypeMapper
{
    private static readonly Lazy<DbTypeMapper> _defaultMapperLazy = new(true);

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
        [typeof(DateOnly)] = DbType.Date,
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
        [typeof(decimal)] = "DECIMAL(19,4)",
        [typeof(bool)] = "BIT",
        [typeof(string)] = "NVARCHAR(512)",
        [typeof(char)] = "NCHAR(1)",
        [typeof(Guid)] = "UNIQUEIDENTIFIER",
        [typeof(DateTime)] = "DATETIME2",
        [typeof(DateOnly)] = "DATE",
        [typeof(DateTimeOffset)] = "DATETIMEOFFSET",
        [typeof(byte[])] = "VARBINARY(4000)",
    };

    /// <summary>
    /// Creates a new instance of the <see cref="DbTypeMapper"/> class.
    /// </summary>
    public DbTypeMapper()
    {
        // placeholder
    }

    /// <summary>
    /// Gets an instance of the default type mapper.
    /// </summary>
    public static IDbTypeMapper Default => _defaultMapperLazy.Value;

    /// <inheritdoc />
    public virtual IReadOnlyList<Type> SupportedTypes => DbTypeMap.Keys.ToArray();

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
    public virtual bool TryGetProviderTypeFor(Type type, [MaybeNullWhen(false)] out string providerType)
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

    /// <inheritdoc />
    public bool TryGetProviderTypeFor(IDbColumnSchema column, [MaybeNullWhen(false)] out string providerType)
    {
        if (column is null)
            throw new ArgumentNullException(nameof(column));

        if (!TryGetProviderTypeFor(column.DataType, out var providerMappedType) ||
            string.IsNullOrWhiteSpace(column.ProviderType))
        {
            providerType = default;
            return false;
        }

        var dataType = new ProviderType(providerMappedType);
        var columnType = new ProviderType(column.ProviderType);
        var hasLength = column.ColumnSize > 0 || column.IsLong;
        var hasPrecision = column.NumericPrecision > 0;
        var hasScale = column.NumericScale > 0;
        var needsArguments = dataType.HasArguments && (hasLength || hasPrecision || hasScale);
        providerType = columnType.BasicType;

        if (needsArguments)
        {
            if (hasPrecision && hasScale && !column.IsLong)
                providerType = $"{columnType.BasicType}({column.NumericPrecision}, {column.NumericScale})";
            else if (hasPrecision && !hasScale && !column.IsLong)
                providerType = $"{columnType.BasicType}({column.NumericPrecision})";
            else if (hasLength)
                providerType = $"{columnType.BasicType}({(column.ColumnSize == int.MaxValue || column.IsLong ? "MAX" : column.ColumnSize)})";
        }

        return true;
    }

    private record ProviderType
    {
        public ProviderType(ReadOnlySpan<char> providerType)
        {
            const char OpenArgs = '(';
            const char CloseArgs = ')';
            const char Separator = ',';

            var startIndex = providerType.IndexOf(OpenArgs);
            var endIndex = providerType.LastIndexOf(CloseArgs);

            var basicType = startIndex >= 0
                ? providerType.Slice(0, startIndex)
                : providerType;

            var builder = new StringBuilder(basicType.Length);
            foreach (var c in basicType)
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToUpperInvariant(c));
            }

            BasicType = builder.ToString();
            var argumentsTextLength = (endIndex - startIndex) - 1;

            if (argumentsTextLength <= 0)
            {
                Arguments = Array.Empty<string>();
                return;
            }

            startIndex++;
            var argumentsText = providerType.Slice(startIndex, argumentsTextLength);
            var argBuilder = new StringBuilder(argumentsText.Length + 1);
            var parsedArgs = new List<string>(4);

            foreach (var c in argumentsText)
            {
                if (c == Separator)
                {
                    parsedArgs.Add(argBuilder.ToString());
                    argBuilder.Clear();
                    continue;
                }

                argBuilder.Append(c);
            }

            if (argBuilder.Length > 0)
                parsedArgs.Add(argBuilder.ToString());

            Arguments = parsedArgs.ToArray();
        }

        public string BasicType { get; }

        public IReadOnlyList<string> Arguments { get; }

        public bool HasArguments => Arguments.Count > 0;
    }
}

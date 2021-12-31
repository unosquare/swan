namespace Swan.Data;

public class DbTable
{
    Dictionary<string, DbColumn> _columns = new(128);
    private const string CommandText = @"SELECT
	 [Column].TABLE_CATALOG AS [Database]
	,[Column].TABLE_SCHEMA AS [Schema]
	,[Column].TABLE_NAME AS [Table]
	,[Column].COLUMN_NAME AS [ColumnName]
	,[Column].ORDINAL_POSITION AS [ColumnOrdinal]
	,[Column].DATA_TYPE AS [ProviderDataType]
	,[Column].CHARACTER_MAXIMUM_LENGTH AS [MasLength]
	,[Column].NUMERIC_PRECISION AS [Precision]
	,[Column].NUMERIC_SCALE AS [Scale]
	,CASE WHEN [Column].IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS [AllowsDBNull]
	,CASE WHEN [Constraint].CONSTRAINT_TYPE = 'PRIMARY KEY' THEN 1 ELSE 0 END AS [IsKey]
	,CASE WHEN COLUMNPROPERTY(object_id([Column].TABLE_NAME), [Column].COLUMN_NAME, 'IsIdentity') = 1
		THEN 1
		ELSE 0
	 END AS [IsAutoIncrement]
	,CASE WHEN COLUMNPROPERTY(object_id([Column].TABLE_NAME), [Column].COLUMN_NAME, 'IsComputed') = 1
		THEN 1
		ELSE 0
	 END AS [IsComputed]
FROM INFORMATION_SCHEMA.COLUMNS AS [Column]
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE [Usage] ON
	[Column].TABLE_CATALOG = [Usage].TABLE_CATALOG AND
	[Column].TABLE_NAME = [Usage].TABLE_NAME AND
	[Column].TABLE_SCHEMA = [Usage].TABLE_SCHEMA AND
	[Column].COLUMN_NAME = [Usage].COLUMN_NAME
LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS [Constraint] ON
	[Constraint].TABLE_CATALOG = [Usage].TABLE_CATALOG AND
	[Constraint].TABLE_NAME = [Usage].TABLE_NAME AND
	[Constraint].TABLE_SCHEMA = [Usage].TABLE_SCHEMA AND
	[Constraint].CONSTRAINT_NAME = [Usage].CONSTRAINT_NAME AND
	[Constraint].CONSTRAINT_TYPE <> 'FOREIGN KEY'
ORDER BY
	[Database], [Schema], [Table], [ColumnOrdinal]";

    public DbTable(IDbConnection connection, string tableName, string? schema)
    {
        Provider = connection.Provider();
        Database = connection.Database;
        TableName = tableName;
        Schema = schema ?? Provider.DefaultSchemaName;

        foreach (var item in connection.Query(CommandText))
        {
            if (item.Schema != Schema || item.Table != TableName)
                continue;

            _columns[item.ColumnName] = new DbColumn
            {
                AllowsDBNull = item.AllowsDBNull != 0,
                ColumnName = item.ColumnName,
                ColumnOrdinal = item.ColumnOrdinal,
                IsAutoIncrement = item.IsAutoIncrement != 0,
                IsReadOnly = item.IsAutoIncrement != 0 || item.IsComputed != 0,
                IsKey = item.IsKey != 0,
                ProviderDataType = item.ProviderDataType,
            };
        }

    }

    public DbProvider Provider { get; }

    public string Database { get; }

    public string Schema { get; }

    public string TableName { get; }

    public IReadOnlyList<DbColumn> Columns => _columns.Values.ToArray();
}


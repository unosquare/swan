namespace Swan.Data.SqlBulkOps;

using Reflection;
using Schema;
using System.Globalization;
using System.Text;

internal static class HelperExtensions
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static bool TryGetRowsCopied(this SqlBulkCopy bulkOperation, out int rowsCopied)
    {
        const string RowsCopiedFieldName = "_rowsCopied";

        rowsCopied = 0;
        var rowsCopiedField = typeof(SqlBulkCopy).TypeInfo().Fields
            .FirstOrDefault(c => !c.IsPublic && c.FieldType == typeof(int) && c.Name.Equals(RowsCopiedFieldName, StringComparison.Ordinal));

        if (rowsCopiedField?.GetValue(bulkOperation) is int actualRowsCopied)
        {
            rowsCopied = actualRowsCopied;
            return true;
        }

        return false;
    }

    public static string BuildBulkUpdateCommandText(IDbTableSchema tempTable, IDbTableSchema targetTable, DbProvider provider)
    {
        var updateCommandText = new StringBuilder(4096);

        // Get intersection of updateable columns
        var columnNames = targetTable.UpdateableColumns.Select(c => c.ColumnName)
            .Intersect(tempTable.UpdateableColumns.Select(c => c.ColumnName), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var keyColumnNames = targetTable.KeyColumns.Select(c => c.ColumnName).ToArray();

        if (columnNames.Length <= 0)
            throw new InvalidOperationException("No columns suitable for an update command were found.");

        if (keyColumnNames.Length <= 0)
            throw new InvalidOperationException("No key columns suitable for an update command were found.");

        var sourceTableName = provider.QuoteTable(tempTable);
        var targetTableName = provider.QuoteTable(targetTable);
        var sourceAlias = provider.QuoteTable("s");
        var targetAlias = provider.QuoteTable("t");

        updateCommandText.AppendLine(Invariant, $"UPDATE {targetAlias} SET");
        for (var i = 0; i < columnNames.Length; i++)
        {
            var colName = provider.QuoteField(columnNames[i]);
            var isLast = i >= columnNames.Length - 1;
            updateCommandText.AppendLine(Invariant,
                $"    {targetAlias}.{colName} = {sourceAlias}.{colName}{(isLast ? string.Empty : ",")}");
        }

        updateCommandText.AppendLine(Invariant, 
            $"FROM {targetTableName} AS {targetAlias} INNER JOIN {sourceTableName} AS {sourceAlias} ON");
        
        for (var i = 0; i < keyColumnNames.Length; i++)
        {
            var colName = provider.QuoteField(keyColumnNames[i]);
            var isLast = i >= keyColumnNames.Length - 1;
            updateCommandText.AppendLine(Invariant,
                $"    {targetAlias}.{colName} = {sourceAlias}.{colName}{(isLast ? string.Empty : " AND ")}");
        }

        return updateCommandText.ToString();
    }

    public static string BuildBulkDeleteCommandText(IDbTableSchema tempTable, IDbTableSchema targetTable, DbProvider provider)
    {
        var deleteCommandText = new StringBuilder(4096);
        var keyColumnNames = targetTable.KeyColumns.Select(c => c.ColumnName).ToArray();
        var sourceTableName = provider.QuoteTable(tempTable);
        var targetTableName = provider.QuoteTable(targetTable);
        var sourceAlias = provider.QuoteTable("s");
        var targetAlias = provider.QuoteTable("t");

        deleteCommandText.AppendLine(Invariant, $"DELETE {targetAlias}");
        deleteCommandText.AppendLine(Invariant,
            $"FROM {targetTableName} AS {targetAlias} INNER JOIN {sourceTableName} AS {sourceAlias} ON");

        for (var i = 0; i < keyColumnNames.Length; i++)
        {
            var colName = provider.QuoteField(keyColumnNames[i]);
            var isLast = i >= keyColumnNames.Length - 1;
            deleteCommandText.AppendLine(Invariant,
                $"    {targetAlias}.{colName} = {sourceAlias}.{colName}{(isLast ? string.Empty : " AND ")}");
        }

        return deleteCommandText.ToString();
    }

    public static async Task<bool> IsMemoryOptimized(this ITableContext table, SqlTransaction? transaction, CancellationToken ct = default)
    {
        await table.Connection.EnsureConnectedAsync(ct);

        var scalarValue = await table.Connection
            .BeginCommandText("SELECT OBJECTPROPERTY(OBJECT_ID(@TableName),'TableIsMemoryOptimized')")
            .EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(new { TableName = table.QuoteTable() })
            .ExecuteScalarAsync(ct)
            .ConfigureAwait(false);

        try
        {
            return Convert.ToInt32(scalarValue, CultureInfo.InvariantCulture) == 1;
        }
        catch
        {
            // ignore
        }

        return false;
    }
}

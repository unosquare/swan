namespace Swan.Data.SqlBulkOps;

using Swan.Data.Schema;
using Swan.Reflection;
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
        var columnNames = targetTable.UpdateableColumns.Select(c => c.Name)
            .Intersect(tempTable.UpdateableColumns.Select(c => c.Name), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var keyColumnNames = targetTable.KeyColumns.Select(c => c.Name).ToArray();
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
}

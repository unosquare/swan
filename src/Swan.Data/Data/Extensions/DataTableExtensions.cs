namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DataTable"/> objects.
/// </summary>
public static partial class DataTableExtensions
{
    /// <summary>
    /// Converts a <see cref="DataTable"/> object into an enumerable set
    /// of <see cref="ExpandoObject"/> with property names corresponding to columns.
    /// Property names are normalized by removing whitespace, special
    /// characters or leading digits.
    /// </summary>
    /// <param name="table">The data table to extract rows from.</param>
    /// <returns>An enumerable set of dynamically typed Expando objects.</returns>
    public static IEnumerable<dynamic> AsEnumerable(this DataTable table)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        foreach (DataRow row in table.Rows)
            yield return row.ParseExpando();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table">The data table to extract rows from.</param>
    /// <returns></returns>
    public static IEnumerable<T> AsEnumerable<T>(this DataTable table)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        foreach (DataRow row in table.Rows)
            yield return row.ParseObject<T>();
    }
}


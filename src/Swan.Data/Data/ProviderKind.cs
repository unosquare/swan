namespace Swan.Data;

/// <summary>
/// Enumerates the database provider kinds this library supports.
/// </summary>
public enum ProviderKind
{
    /// <summary>
    /// Unsupported or unknown provider.
    /// </summary>
    Unknown,

    /// <summary>
    /// Microsoft SQL Server.
    /// </summary>
    SqlServer,

    /// <summary>
    /// Oracle MySql or MariaDB
    /// </summary>
    MySql,

    /// <summary>
    /// Sqlite 3 or higher.
    /// </summary>
    Sqlite
}

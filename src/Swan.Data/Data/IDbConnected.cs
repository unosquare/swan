namespace Swan.Data;

/// <summary>
/// Implemented in classes that keep connection references.
/// </summary>
public interface IDbConnected
{
    /// <summary>
    /// Gets the live connection associated with this object.
    /// </summary>
    DbConnection Connection { get; }
}

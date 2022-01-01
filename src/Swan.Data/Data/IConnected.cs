namespace Swan.Data;

/// <summary>
/// Implemented in classes that keep connection references.
/// </summary>
public interface IConnected
{
    /// <summary>
    /// Gets the live connection associated with this object.
    /// </summary>
    IDbConnection Connection { get; }
}

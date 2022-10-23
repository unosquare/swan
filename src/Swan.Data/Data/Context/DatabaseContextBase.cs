namespace Swan.Data.Context;

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

/// <summary>
/// Provides a base class for implementing a database context.
/// This encapsulates a connection, its associated tables and typical database
/// functionality. Table schemas are utomatically populated,
/// mapped to corresponding DTOs and cached for efficiency.
/// </summary>
public abstract class DatabaseContextBase : IDbConnected, IDisposable
{
    /// <summary>
    /// Holds method information for the
    /// <see cref="ConnectionExtensions.Table{T}(DbConnection, string, string?, DbTransaction?)"/> method.
    /// </summary>
    private static readonly MethodInfo TableMethod = typeof(ConnectionExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(c => c.IsGenericMethod && c.Name.Equals(nameof(ConnectionExtensions.Table), StringComparison.Ordinal));

    /// <summary>
    /// Holds a dictionary of record types ans their associated property proxies.
    /// </summary>
    private readonly Dictionary<Type, IPropertyProxy> TableContextProxies;
    private readonly SemaphoreSlim Semaphore = new(1, 1);
    private bool isDisposed;

    /// <summary>
    /// Creates a new instance of the <see cref="DatabaseContextBase"/> class.
    /// </summary>
    protected DatabaseContextBase(DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        Connection = connection;
        Connection.ConfigureAwait(false);
        Connection.EnsureConnected();

        Provider = connection.Provider();
        Database = connection.Database;

        var contextProperties = GetType().TypeInfo().Properties;
        TableContextProxies = new Dictionary<Type, IPropertyProxy>();
        foreach ((_, var property) in contextProperties)
        {
            if (property.PropertyType.GenericTypeArguments.Count != 1 ||
                property.PropertyType.NativeType.GetGenericTypeDefinition() != typeof(ITableContext<>) |
                !property.CanRead || !property.CanWrite)
                continue;

            var recordType = property.PropertyType.GenericTypeArguments[0];
            TableContextProxies.Add(recordType.NativeType, property);
        }

        InitializeTableProperties();
    }

    /// <inheritdoc />
    public virtual DbConnection Connection { get; }

    /// <inheritdoc />
    public virtual DbProvider Provider { get; }

    /// <summary>
    /// Gets the name of the database for the associated connection.
    /// </summary>
    public string Database { get; }

    private void InitializeTableProperties()
    {
        // Populate the ITableContext<> Properties
        foreach ((var recordType, var tableProxy) in TableContextProxies)
        {
            // Write the result to the private setter of the DataContext we created.
            tableProxy.Write(this, InitializeTableProperty(recordType, tableProxy));
        }
    }

    /// <summary>
    /// Tries to acquire a registered table context for the given record type.
    /// </summary>
    /// <param name="recordType">The record type.</param>
    /// <param name="tableContext">The associated table context.</param>
    /// <returns>True when the operation succeeds. False otherwise.</returns>
    public virtual bool TryGetTableContext(Type recordType, [MaybeNullWhen(false)] out ITableContext tableContext)
    {
        tableContext = null;
        if (!TableContextProxies.TryGetValue(recordType, out var propertyProxy))
            return false;

        tableContext = propertyProxy.Read(this) as ITableContext;
        return tableContext is not null;
    }

    /// <summary>
    /// Tries to acquire a registered table context for the given record type.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="tableContext">The associated table context.</param>
    /// <returns>True when the operation succeeds. False otherwise.</returns>
    public virtual bool TryGetTableContext<T>([MaybeNullWhen(false)] out ITableContext<T> tableContext)
        where T : class
    {
        tableContext = null;
        if (!TableContextProxies.TryGetValue(typeof(T), out var propertyProxy))
            return false;

        tableContext = propertyProxy.Read(this) as ITableContext<T>;
        return tableContext is not null;
    }

    /// <summary>
    /// Signals a new data operation will be executed.
    /// Asynchronously waits for any operation in progress.
    /// Always call the <see cref="CompleteDataOperation"/> method at the end of the operation.
    /// </summary>
    /// <param name="ct">The opetional cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    public async Task BeginDataOperationAsync(CancellationToken ct = default) =>
        await Semaphore.WaitAsync(ct).ConfigureAwait(false);

    /// <summary>
    /// Signals the completion of a data operation so that a new one can take place.
    /// </summary>
    private void CompleteDataOperation() => Semaphore.Release();

    /// <summary>
    /// Initializes a table property in the current <see cref="DatabaseContextBase"/>.
    /// </summary>
    /// <param name="recordType">The type argument of the table context.</param>
    /// <param name="tableProperty">The property metadata that holds the table context.</param>
    /// <returns>The table context that will be writtent to the table property.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    protected virtual ITableContext InitializeTableProperty(Type recordType, IPropertyProxy tableProperty)
    {
        if (recordType is null)
            throw new ArgumentNullException(nameof(recordType));

        if (tableProperty is null)
            throw new ArgumentNullException(nameof(tableProperty));

        // We will use this array to pass as arguments to the
        // Table<> method.
        var tableCallArgs = new object?[]
        {
            Connection,
            default(string), // table name
            default(string), // schema name
            default(DbTransaction),
        };

        // Produce a typed version of the generic method.
        var tableMethod = TableMethod.MakeGenericMethod(recordType);

        // Try to get a table attribute
        var tableAttribute = tableProperty.Attribute<TableAttribute>();

        if (tableAttribute is not null)
        {
            if (!string.IsNullOrWhiteSpace(tableAttribute.Name))
                tableCallArgs[1] = tableAttribute.Name;

            if (!string.IsNullOrWhiteSpace(tableAttribute.Schema))
                tableCallArgs[2] = tableAttribute.Schema;
        }

        // Update the argument list to contain the target table name.
        // The target table name must equal the property name.
        tableCallArgs[1] ??= tableProperty.PropertyName;

        // Invoke the typed version of the TableAsync<> method.
        if (tableMethod.Invoke(null, tableCallArgs) is not ITableContext tableContext)
            throw new NotSupportedException($"The method call did not return an object of type '{nameof(ITableContext)}'.");

        return tableContext;
    }

    /// <summary>
    /// Disposes unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="alsoManaged">Whether managed resources will also be freed.</param>
    protected virtual void Dispose(bool alsoManaged)
    {
        if (isDisposed)
            return;

        isDisposed = true;

        if (alsoManaged)
        {
            Connection.ConfigureAwait(false);
            Connection.Dispose();
            Semaphore.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(alsoManaged: true);
        GC.SuppressFinalize(this);
    }
}

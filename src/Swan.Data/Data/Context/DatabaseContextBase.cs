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
    // TODO: Implement schema caching

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
    private readonly List<IPropertyProxy> TableContextProxies;
    private readonly SemaphoreSlim Semaphore = new(1, 1);
    private bool isDisposed;

    /// <summary>
    /// Creates a new instance of the <see cref="DatabaseContextBase"/> class.
    /// </summary>
    protected DatabaseContextBase(DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        // Setup connection, provider and database.
        Connection = connection;
        Connection.ConfigureAwait(false);
        Connection.EnsureConnected();

        Provider = connection.Provider();
        Database = connection.Database;

        var tableProperties = GetType().TypeInfo().Properties.Values.ToArray();
        TableContextProxies = new(tableProperties.Length);
        foreach (var property in tableProperties)
        {
            if (property.PropertyType.GenericTypeArguments.Count != 1 ||
                property.PropertyType.NativeType.GetGenericTypeDefinition() != typeof(ITableContext<>) |
                !property.CanRead || !property.CanWrite)
                continue;

            TableContextProxies.Add(property);
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
        foreach (var tableProperty in TableContextProxies)
        {
            // Write the result to the private setter of the DataContext we created.
            tableProperty.Write(this, InitializeTableProperty(tableProperty));
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
        if (recordType is null)
            throw new ArgumentNullException(nameof(recordType));

        tableContext = null;
        var tableProperty = TableContextProxies
            .FirstOrDefault(c => c.PropertyType.GenericTypeArguments[0].NativeType == recordType);

        if (tableProperty is null)
            return false;

        tableContext = tableProperty.Read(this) as ITableContext;
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
        if (TryGetTableContext(typeof(T), out var table))
            tableContext = table as ITableContext<T>;

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
    public void CompleteDataOperation() => Semaphore.Release();

    /// <summary>
    /// Initializes a table property in the current <see cref="DatabaseContextBase"/>.
    /// </summary>
    /// <param name="tableProperty">The property metadata that holds the table context.</param>
    /// <returns>The table context that will be writtent to the table property.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    protected virtual ITableContext InitializeTableProperty(IPropertyProxy tableProperty)
    {
        if (tableProperty is null)
            throw new ArgumentNullException(nameof(tableProperty));

        var recordType = tableProperty.PropertyType.GenericTypeArguments[0];

        // We will use this array to pass as arguments to the
        // Table<> method.
        var tableCallArgs = new object?[]
        {
            Connection,
            default(string), // table name
            default(string), // schema name
            default(DbTransaction),
        };

        // validate call parameter count and types
        var p = TableMethod.GetParameters();
        if (p.Length != tableCallArgs.Length)
            throw new InvalidOperationException("Table method call argument count does not match provided argument count.");

        for (var i = 0; i < p.Length; i++)
        {
            if (!p[i].ParameterType.IsAssignableFrom(tableCallArgs[i]?.GetType() ?? p[i].ParameterType))
                throw new InvalidOperationException($"Table method argument {p[i].Name} has an incompatible value.");
        }

        // Produce a typed version of the generic method.
        var tableMethod = TableMethod.MakeGenericMethod(recordType.NativeType);

        // Try to get a table attribute from type definition.
        var tableAttribute = recordType.NativeType.Attribute<TableAttribute>();
        if (tableAttribute is not null)
        {
            if (!string.IsNullOrWhiteSpace(tableAttribute.Name))
                tableCallArgs[1] = tableAttribute.Name;

            if (!string.IsNullOrWhiteSpace(tableAttribute.Schema))
                tableCallArgs[2] = tableAttribute.Schema;
        }

        // If not yet set, the target table name must equal the property name.
        tableCallArgs[1] ??= tableProperty.PropertyName;

        // Invoke the typed version of the TableAsync<> method.
        return tableMethod.Invoke(null, tableCallArgs) is not ITableContext tableContext
            ? throw new NotSupportedException($"The method call did not return an object of type '{nameof(ITableContext)}'.")
            : tableContext;
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

namespace Swan.Data.Extensions;

public static partial class QueryExtensions
{
    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async IAsyncEnumerable<T> QueryAsync<T>(this DbCommand command, CommandBehavior behavior = CommandBehavior.Default,
        Func<IDataRecord, T>? deserialize = default, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        deserialize ??= (r) => r.ParseObject<T>();
        var reader = await command.ExecuteOptimizedReaderAsync(behavior, ct).ConfigureAwait(false);

        try
        {
            if (reader.FieldCount <= 0)
                yield break;

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
                yield return deserialize(reader);

            // Skip the following result sets.
            while (await reader.NextResultAsync(ct).ConfigureAwait(false)) { }

            // Gracefully dispose the reader.
            await reader.DisposeAsync().ConfigureAwait(false);
            reader = null;
        }
        finally
        {
            if (reader is not null)
            {
                if (!reader.IsClosed)
                {
                    try { command.Cancel(); }
                    catch { /* ignore */ }
                }

                await reader.DisposeAsync().ConfigureAwait(false);
            }

            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IAsyncEnumerable<dynamic> QueryAsync(this DbCommand command, CommandBehavior behavior = CommandBehavior.Default,
        CancellationToken ct = default) =>
        command.QueryAsync(behavior, (reader) => reader.ParseExpando(), ct);

}


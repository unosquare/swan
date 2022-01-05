namespace Swan.Data;

using System.Data;

/// <summary>
/// A table context that maps between a given type and a data store.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ObjectContext<T> : TableContext, ITableContext<T>
    where T : class
{
    public ObjectContext(IDbConnection connection, string tableName, string? schema = null)
        : base(connection, tableName, schema)
    {

    }

    public T CreateOne(T item, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public int CreateMany(T item, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public T UpdateOne(T item, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public T UpdateMany(IEnumerable<T> items, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> RetrieveAll(IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> RetrievePaged(int skip, int take, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public bool TryFind(object key, out T item, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public T DeleteOne(T item, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public T DeleteMany(IEnumerable<T> items, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }
}


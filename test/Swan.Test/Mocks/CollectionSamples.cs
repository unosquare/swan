namespace Swan.Test.Mocks;

using System.Collections;
using System.Collections.ObjectModel;

public static class CollectionSamples
{
    private static readonly Dictionary<string, int> BaseCollection = new()
    {
        ["item 1"] = 1,
        ["item 2"] = 2,
        ["item 3"] = 3,
        ["item 4"] = 4,
        ["item 5"] = 5,
        ["item 6"] = 6,
        ["item 7"] = 7,
        ["item 8"] = 8,
    };

    public static IEnumerable ReadOnlyDictionary => new ReadOnlyDictionary<string, int>(BaseCollection);

    public static IEnumerable GenericDictionary => BaseCollection.ToDictionary(c => c.Key, c => c.Value);

    public static IEnumerable Dictionary => new Hashtable(BaseCollection);

    public static IEnumerable ReadOnlyList => new ArraySegment<int>(BaseCollection.Values.ToArray());

    public static IEnumerable GenericList => BaseCollection.Values.Select(c => c).ToList();

    public static IEnumerable List => new ArrayList(BaseCollection.Values.ToArray());

    public static IEnumerable ReadOnlyCollection => new ReadOnlyCollection<int>(BaseCollection.Values.ToList());

    public static IEnumerable GenericCollection => new HashSet<int>(BaseCollection.Values.ToArray());

    public static IEnumerable Collection
    {
        get
        {
            var q = new Queue();
            foreach (var item in BaseCollection.Values)
                q.Enqueue(item);

            return q;
        }
    }

    public static IEnumerable GenericEnumerable => BaseCollection.Values.Select(c => c);

    public static IEnumerable Enumerable => new SimpleEnumerable();

    public static IEnumerable Array => "Hello World!".ToArray();

    private class SimpleEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator() => BaseCollection.Values.GetEnumerator();
    }
}

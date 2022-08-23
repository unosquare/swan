namespace Swan.Test;

using NUnit.Framework;
using Collections;
using Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

[TestFixture]
public class CollectionProxyTest : TestFixtureBase
{
    [Test]
    public void ProxyIsCreatedWithValidKind()
    {
        var testCases = new Dictionary<CollectionKind, IEnumerable>()
        {
            [CollectionKind.GenericDictionary] = CollectionSamples.GenericDictionary,
            [CollectionKind.Dictionary] = CollectionSamples.Dictionary,
            [CollectionKind.GenericList] = CollectionSamples.GenericList,
            [CollectionKind.List] = CollectionSamples.List,
            [CollectionKind.GenericCollection] = CollectionSamples.GenericCollection,
            [CollectionKind.Collection] = CollectionSamples.Collection,
            [CollectionKind.GenericEnumerable] = CollectionSamples.GenericEnumerable,
            [CollectionKind.Enumerable] = CollectionSamples.Enumerable,
            [CollectionKind.List] = CollectionSamples.Array,
        };

        foreach ((CollectionKind kind, var collection) in testCases)
        {
            var proxy = collection.AsProxy();
            Assert.AreEqual(kind, proxy.CollectionKind);
        }
    }

    [TestCase(typeof(int))]
    [TestCase(typeof(object))]
    [TestCase(typeof(double))]
    [TestCase(typeof(CollectionProxyTest))]
    public void ProxyFailsWithInvalidTypes(Type t)
    {
        var instance = t.TypeInfo().CreateInstance();
        Assert.IsFalse(CollectionProxy.TryCreate(instance, out _));
    }

    [Test]
    public void IsReadOnlyWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>()
        {
            [CollectionSamples.ReadOnlyDictionary] = true,
            [CollectionSamples.GenericDictionary] = false,
            [CollectionSamples.Dictionary] = false,
            [CollectionSamples.ReadOnlyList] = true,
            [CollectionSamples.GenericList] = false,
            [CollectionSamples.List] = false,
            [CollectionSamples.ReadOnlyCollection] = true,
            [CollectionSamples.GenericCollection] = false,
            [CollectionSamples.Collection] = false,
            [CollectionSamples.GenericEnumerable] = false,
            [CollectionSamples.Enumerable] = false,
            [CollectionSamples.Array] = false,
        };

        var index = 0;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            var proxy = collection.AsProxy();
            var result = proxy.IsReadOnly;
            Assert.AreEqual(result, expected);
            index++;
        }
    }

    [Test]
    public void IsFixedSizeWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>()
        {
            [CollectionSamples.ReadOnlyDictionary] = true,
            [CollectionSamples.GenericDictionary] = false,
            [CollectionSamples.Dictionary] = false,
            [CollectionSamples.ReadOnlyList] = true,
            [CollectionSamples.GenericList] = false,
            [CollectionSamples.List] = false,
            [CollectionSamples.ReadOnlyCollection] = true,
            [CollectionSamples.GenericCollection] = false,
            [CollectionSamples.Collection] = false,
            [CollectionSamples.GenericEnumerable] = true,
            [CollectionSamples.Enumerable] = true,
            [CollectionSamples.Array] = true,
        };

        var index = 0;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            var proxy = collection.AsProxy();
            var result = proxy.IsFixedSize;
            Assert.AreEqual(result, expected);
            index++;
        }
    }

    [Test]
    public void CountWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>()
        {
            [CollectionSamples.ReadOnlyDictionary] = true,
            [CollectionSamples.GenericDictionary] = false,
            [CollectionSamples.Dictionary] = false,
            [CollectionSamples.ReadOnlyList] = true,
            [CollectionSamples.GenericList] = false,
            [CollectionSamples.List] = false,
            [CollectionSamples.ReadOnlyCollection] = true,
            [CollectionSamples.GenericCollection] = false,
            [CollectionSamples.Collection] = true,
            [CollectionSamples.GenericEnumerable] = true,
            [CollectionSamples.Enumerable] = true,
            [CollectionSamples.Array] = true,
        };

        var index = 0;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            var proxy = collection.AsProxy();
            var result = proxy.Count;
            Assert.IsTrue(result > 0);
            index++;
        }
    }

    [Test]
    public void IsSynchronizedWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>()
        {
            [CollectionSamples.ReadOnlyDictionary] = false,
            [CollectionSamples.GenericDictionary] = false,
            [CollectionSamples.Dictionary] = false,
            [CollectionSamples.ReadOnlyList] = false,
            [CollectionSamples.GenericList] = false,
            [CollectionSamples.List] = false,
            [CollectionSamples.ReadOnlyCollection] = false,
            [CollectionSamples.GenericCollection] = false,
            [CollectionSamples.Collection] = false,
            [CollectionSamples.GenericEnumerable] = false,
            [CollectionSamples.Enumerable] = false,
            [CollectionSamples.Array] = false,
        };

        var index = 0;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            var proxy = collection.AsProxy();
            var result = proxy.IsSynchronized;
            Assert.AreEqual(result, expected);
            index++;
        }
    }

    [Test]
    public void ClearWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>()
        {
            [CollectionSamples.ReadOnlyDictionary] = true,
            [CollectionSamples.GenericDictionary] = false,
            [CollectionSamples.Dictionary] = false,
            [CollectionSamples.ReadOnlyList] = true,
            [CollectionSamples.GenericList] = false,
            [CollectionSamples.List] = false,
            [CollectionSamples.ReadOnlyCollection] = true,
            [CollectionSamples.GenericCollection] = false,
            [CollectionSamples.Collection] = false,
            [CollectionSamples.GenericEnumerable] = true,
            [CollectionSamples.Enumerable] = true,
            [CollectionSamples.Array] = true,
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();

            if (expected)
            {
                Assert.Catch(() => proxy.Clear());
                continue;
            }

            proxy.Clear();
            Assert.IsTrue(proxy.Count == 0);
        }
    }

    [Test]
    public void IndexerWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>()
        {
            [CollectionSamples.ReadOnlyDictionary] = true,
            [CollectionSamples.GenericDictionary] = false,
            [CollectionSamples.Dictionary] = false,
            [CollectionSamples.ReadOnlyList] = true,
            [CollectionSamples.GenericList] = false,
            [CollectionSamples.List] = false,
            [CollectionSamples.ReadOnlyCollection] = true,
            [CollectionSamples.GenericCollection] = false,
            [CollectionSamples.Collection] = true,
            [CollectionSamples.GenericEnumerable] = true,
            [CollectionSamples.Enumerable] = true,
            [CollectionSamples.Array] = true,
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            Assert.IsTrue(proxy[7] is int or char);

            if (!proxy.IsReadOnly)
            {
                if (proxy.IsDictionary)
                {
                    proxy["item 1"] = 32;
                    Assert.IsTrue(Equals(proxy["item 1"], 32));
                }
                else
                {
                    if (proxy.IsArray)
                    {
                        proxy["1"] = "A";
                        proxy[2] = "B";
                        Assert.IsTrue(Equals(proxy[1], 'A'));
                        Assert.IsTrue(Equals(proxy[2], 'B'));
                    }
                    else if (proxy.CollectionKind is CollectionKind.List or CollectionKind.GenericList)
                    {
                        proxy["1"] = "32";
                        Assert.IsTrue(Equals(proxy[1], 32) || Equals(proxy[1], "32"));
                    }
                    else
                    {
                        Assert.Catch(() => proxy["1"] = "32");
                    }
                }
            }


            if (proxy.IsDictionary is false)
                Assert.IsTrue(proxy["7"] is int or char);
            else
                Assert.IsTrue(proxy["item 1"] is int);

            if (proxy.IsReadOnly is false)
            {
                // if (proxy.)
            }
        }
    }

    [Test]
    public void CollectionTypeAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, Type>()
        {
            [CollectionSamples.ReadOnlyDictionary] = typeof(IDictionary<string, int>),
            [CollectionSamples.GenericDictionary] = typeof(IDictionary<string, int>),
            [CollectionSamples.Dictionary] = typeof(IDictionary),
            [CollectionSamples.ReadOnlyList] = typeof(IList<int>),
            [CollectionSamples.GenericList] = typeof(IList<int>),
            [CollectionSamples.List] = typeof(IList),
            [CollectionSamples.ReadOnlyCollection] = typeof(IList<int>),
            [CollectionSamples.GenericCollection] = typeof(ICollection<int>),
            [CollectionSamples.Collection] = typeof(ICollection),
            [CollectionSamples.GenericEnumerable] = typeof(IEnumerable<int>),
            [CollectionSamples.Enumerable] = typeof(IEnumerable),
            [CollectionSamples.Array] = typeof(IList),
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            Assert.IsTrue(proxy.CollectionType.NativeType == expected);
        }
    }

    [Test]
    public void FirstWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, object>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 1,
            [CollectionSamples.GenericDictionary] = 1,
            [CollectionSamples.Dictionary] = 1,
            [CollectionSamples.ReadOnlyList] = 1,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 1,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 1,
            [CollectionSamples.GenericEnumerable] = 1,
            [CollectionSamples.Enumerable] = 1,
            [CollectionSamples.Array] = 'H',
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            if (collection is Hashtable ht)
                Assert.IsTrue(proxy.First() is int);
            else
                Assert.IsTrue(Equals(proxy.First(), expected));
        }
    }

    [Test]
    public void LastWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, object>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 8,
            [CollectionSamples.GenericDictionary] = 8,
            [CollectionSamples.Dictionary] = 8,
            [CollectionSamples.ReadOnlyList] = 8,
            [CollectionSamples.GenericList] = 8,
            [CollectionSamples.List] = 8,
            [CollectionSamples.ReadOnlyCollection] = 8,
            [CollectionSamples.GenericCollection] = 8,
            [CollectionSamples.Collection] = 8,
            [CollectionSamples.GenericEnumerable] = 8,
            [CollectionSamples.Enumerable] = 8,
            [CollectionSamples.Array] = '!',
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            if (collection is Hashtable ht)
                Assert.IsTrue(proxy.Last() is int);
            else
                Assert.IsTrue(Equals(proxy.Last(), expected));
        }
    }

    [Test]
    public void CopyToWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 8,
            [CollectionSamples.GenericDictionary] = 8,
            [CollectionSamples.Dictionary] = 8,
            [CollectionSamples.ReadOnlyList] = 8,
            [CollectionSamples.GenericList] = 8,
            [CollectionSamples.List] = 8,
            [CollectionSamples.ReadOnlyCollection] = 8,
            [CollectionSamples.GenericCollection] = 8,
            [CollectionSamples.Collection] = 8,
            [CollectionSamples.GenericEnumerable] = 8,
            [CollectionSamples.Enumerable] = 8,
            [CollectionSamples.Array] = '!',
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            var target = new object[proxy.Count];
            proxy.CopyTo(target, 0);

            Assert.IsTrue(target.Length == proxy.Count);
            foreach (var item in target)
                Assert.IsTrue(item is int or char);
        }
    }

    [Test]
    public void ConversionWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 8,
            [CollectionSamples.GenericDictionary] = 8,
            [CollectionSamples.Dictionary] = 8,
            [CollectionSamples.ReadOnlyList] = 8,
            [CollectionSamples.GenericList] = 8,
            [CollectionSamples.List] = 8,
            [CollectionSamples.ReadOnlyCollection] = 8,
            [CollectionSamples.GenericCollection] = 8,
            [CollectionSamples.Collection] = 8,
            [CollectionSamples.GenericEnumerable] = 8,
            [CollectionSamples.Enumerable] = 8,
            [CollectionSamples.Array] = '!',
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            var list = proxy.ToList<string>();
            var arr = proxy.ToArray<int>();
            Assert.IsTrue(list.Count == proxy.Count);
            Assert.IsTrue(arr.Length == proxy.Count);
            foreach (var item in list)
                Assert.IsTrue(item is string);

            var array = proxy.ToArray();
            Assert.IsTrue(array.Length > 0);

            Assert.IsTrue(proxy.SyncRoot is not null);
        }
    }

    [Test]
    public void AddWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            [CollectionSamples.GenericDictionary] = 2,
            [CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            [CollectionSamples.GenericEnumerable] = 0,
            [CollectionSamples.Enumerable] = 0,
            [CollectionSamples.Array] = 0,
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            if (expected == 0)
            {
                Assert.Catch(() => proxy.Add("hello"));
                Assert.Catch(() => proxy.AddRange(new[] { 2, 3, 4, 5 }));
                continue;
            }

            if (expected == 1)
            {
                Assert.Catch(() => proxy.Add("item 8", 30));
                proxy.Add("9");
                proxy.AddRange(new[] { 2, 3, 4, 5 });
                var lastItem = proxy.Last();
                Assert.IsTrue(lastItem is int or string);
            }

            if (expected == 2)
            {
                proxy.Add("item 9", 9);

                var lastItem = proxy.Last();
                Assert.IsTrue(lastItem is int or string);
            }
        }
    }

    [Test]
    public void IndexOfWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            [CollectionSamples.GenericDictionary] = 2,
            [CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            [CollectionSamples.GenericEnumerable] = 0,
            [CollectionSamples.Enumerable] = 0,
            [CollectionSamples.Array] = 0,
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();

            if (proxy.CollectionKind is CollectionKind.GenericDictionary)
            {
                Assert.IsTrue(proxy.IndexOf("1") >= 0);
                Assert.IsTrue(proxy.IndexOf("nonexistent") == -1);
                continue;
            }

            if (proxy.IsDictionary)
            {
                Assert.IsTrue(proxy.IndexOf(1) >= 0);
                Assert.IsTrue(proxy.IndexOf("nonexistent") == -1);
                continue;
            }

            if (proxy.IsArray)
            {
                Assert.IsTrue(proxy.IndexOf('H') >= 0);
                Assert.IsTrue(proxy.IndexOf('z') == -1);
                continue;
            }

            Assert.IsTrue(proxy.IndexOf(2) >= 0);
            Assert.IsTrue(proxy.IndexOf("444") < 0);
        }
    }

    [Test]
    public void RemoveAtWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            [CollectionSamples.GenericDictionary] = 2,
            [CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            [CollectionSamples.GenericEnumerable] = 0,
            [CollectionSamples.Enumerable] = 0,
            [CollectionSamples.Array] = 0,
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            var originalItem = proxy.LastOrDefault();

            if (proxy.IsFixedSize || proxy.IsReadOnly || proxy.SourceType.NativeType == typeof(Queue))
            {
                Assert.Catch(() => proxy.RemoveAt(proxy.Count - 1));
                continue;
            }

            proxy.RemoveAt(proxy.Count - 1);
            Assert.IsFalse(Equals(originalItem, proxy.LastOrDefault()));
            Assert.IsTrue(Equals(Array.Empty<int>().AsProxy().LastOrDefault(), 0));
            Assert.IsTrue(Equals(Array.Empty<string>().AsProxy().FirstOrDefault(), null));

            Assert.Catch(() => proxy.RemoveAt(394995));
        }
    }

    [Test]
    public void RemoveWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            [CollectionSamples.GenericDictionary] = 2,
            [CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            [CollectionSamples.GenericEnumerable] = 0,
            [CollectionSamples.Enumerable] = 0,
            [CollectionSamples.Array] = 0,
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();
            var originalCount = proxy.Count;

            if (!proxy.IsDictionary && proxy.ValuesType.NativeType != typeof(object))
                Assert.Catch(() => proxy.Remove("XXXXX"));

            if (proxy.IsFixedSize || proxy.IsReadOnly)
            {
                Assert.Catch(() => proxy.Remove("item 1"));
                continue;
            }

            if (proxy.IsDictionary)
            {
                Assert.IsTrue(proxy.ContainsKey("item 6"));
                proxy.Remove("item 6");
                Assert.IsFalse(proxy.ContainsKey("item 6"));
                continue;
            }

            if (proxy.Collection is Queue)
                continue;

            Assert.IsTrue(proxy.ContainsValue(6));
            proxy.Remove(6);
            Assert.IsFalse(proxy.ContainsValue(6));
        }
    }

    [Test]
    public void InsertWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            [CollectionSamples.GenericDictionary] = 2,
            [CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            [CollectionSamples.GenericEnumerable] = 0,
            [CollectionSamples.Enumerable] = 0,
            [CollectionSamples.Array] = 0,
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();

            if (proxy.IsFixedSize || proxy.IsReadOnly || proxy.IsDictionary || (
                    proxy.CollectionKind is not (CollectionKind.List or CollectionKind.GenericList)))
            {
                Assert.Catch(() => proxy.Insert(0, 99));
                continue;
            }

            var originalItem = proxy.FirstOrDefault();
            proxy.Insert(0, 99);
            Assert.IsFalse(Equals(originalItem, proxy.FirstOrDefault()));
        }
    }

    [Test]
    public void KeysWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            [CollectionSamples.GenericDictionary] = 2,
            [CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            [CollectionSamples.GenericEnumerable] = 0,
            [CollectionSamples.Enumerable] = 0,
            [CollectionSamples.Array] = 0,
        };

        var index = -1;
        foreach ((IEnumerable collection, var expected) in testCases)
        {
            index++;

            var proxy = collection.AsProxy();

            if (proxy.IsDictionary)
            {
                foreach (var key in proxy.Keys)
                    Assert.IsTrue(key is string);

                continue;
            }

            foreach (var key in proxy.Keys)
                Assert.IsTrue(key is int);
        }
    }

    [Test]
    public void ContainsKeyWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            [CollectionSamples.GenericDictionary] = 2,
            [CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            [CollectionSamples.GenericEnumerable] = 0,
            [CollectionSamples.Enumerable] = 0,
            [CollectionSamples.Array] = 0,
        };

        foreach ((IEnumerable collection, _) in testCases)
        {
            var proxy = collection.AsProxy();

            if (proxy.IsDictionary)
            {
                Assert.IsTrue(proxy.ContainsKey("item 2"));
                Assert.IsFalse(proxy.ContainsKey(1));

                continue;
            }

            Assert.IsTrue(proxy.ContainsKey(proxy.Count - 1));
            Assert.IsFalse(proxy.ContainsKey("-1"));
            Assert.IsFalse(proxy.ContainsKey("xYx"));
        }
    }

    [Test]
    public void EnumeratorsWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>()
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            [CollectionSamples.GenericDictionary] = 2,
            [CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            [CollectionSamples.GenericList] = 1,
            [CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            [CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            [CollectionSamples.GenericEnumerable] = 0,
            [CollectionSamples.Enumerable] = 0,
            [CollectionSamples.Array] = 0,
        };

        foreach ((IEnumerable collection, _) in testCases)
        {
            var proxy = collection.AsProxy();

            proxy.ForEach((kvp) =>
            {
                Assert.IsTrue(kvp.Key is string or int);
                Assert.IsTrue(kvp.Value is int or char);
            });
        }
    }
}

namespace Swan.Test;

using Collections;
using Mocks;
using Reflection;
using System.Collections;

[TestFixture]
public class CollectionProxyTest : TestFixtureBase
{
    [Test]
    public void ProxyIsCreatedWithValidKind()
    {
        var testCases = new Dictionary<CollectionKind, IEnumerable>
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

        foreach (var (kind, collection) in testCases)
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
        var testCases = new Dictionary<IEnumerable, bool>
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

        foreach (var (collection, expected) in testCases)
        {
            var proxy = collection.AsProxy();
            var result = proxy.IsReadOnly;
            Assert.AreEqual(result, expected);
        }
    }

    [Test]
    public void IsFixedSizeWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>
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

        foreach (var (collection, expected) in testCases)
        {
            var proxy = collection.AsProxy();
            var result = proxy.IsFixedSize;
            Assert.AreEqual(result, expected);
        }
    }

    [Test]
    public void CountWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>
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
            //[CollectionSamples.GenericEnumerable] = true,
            //[CollectionSamples.Enumerable] = true,
            //[CollectionSamples.Array] = true,
        };

        foreach (var (collection, _) in testCases)
        {
            var proxy = collection.AsProxy();
            var result = proxy.Count;
            Assert.IsTrue(result > 0);
        }
    }

    [Test]
    public void IsSynchronizedWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>
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

        foreach (var (collection, expected) in testCases)
        {
            var proxy = collection.AsProxy();
            var result = proxy.IsSynchronized;
            Assert.AreEqual(result, expected);
        }
    }

    [Test]
    public void ClearWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, bool>
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

        foreach (var (collection, expected) in testCases)
        {
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
        var testCases = new Dictionary<IEnumerable, bool>
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

        foreach (var (collection, _) in testCases)
        {
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
        }
    }

    [Test]
    public void CollectionTypeAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, Type>
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

        foreach (var (collection, expected) in testCases)
        {
            var proxy = collection.AsProxy();
            Assert.IsTrue(proxy.CollectionType.NativeType == expected);
        }
    }

    [Test]
    public void FirstWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, object>
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

        foreach (var (collection, expected) in testCases)
        {
            var proxy = collection.AsProxy();
            if (collection is Hashtable)
                Assert.IsTrue(proxy.First() is int);
            else
                Assert.IsTrue(Equals(proxy.First(), expected));
        }
    }

    [Test]
    public void LastWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, object>
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

        };

        foreach (var (collection, expected) in testCases)
        {
            var proxy = collection.AsProxy();
            if (collection is Hashtable)
                Assert.IsTrue(proxy.Last() is int);
            else
                Assert.IsTrue(Equals(proxy.Last(), expected));
        }
    }

    [Test]
    public void CopyToWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>
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
            //[CollectionSamples.GenericEnumerable] = 8,
            //[CollectionSamples.Enumerable] = 8,
            //[CollectionSamples.Array] = '!',
        };

        foreach (var (collection, _) in testCases)
        {
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
        var testCases = new Dictionary<IEnumerable, int>
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
        };

        foreach (var (collection, _) in testCases)
        {
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
        var testCases = new Dictionary<IEnumerable, int>
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

        foreach (var (collection, expected) in testCases)
        {
            var proxy = collection.AsProxy();

            switch (expected)
            {
                case 0:
                    Assert.Catch(() => proxy.Add("hello"));
                    Assert.Catch(() => proxy.AddRange(new[] {2, 3, 4, 5}));
                    continue;
                case 1:
                    {
                        Assert.Catch(() => proxy.Add("item 8", 30));
                        proxy.Add("9");
                        proxy.AddRange(new[] {2, 3, 4, 5});
                        var lastItem = proxy.Last();
                        Assert.IsTrue(lastItem is int or string);
                        break;
                    }
                case 2:
                    {
                        proxy.Add("item 9", 9);

                        var lastItem = proxy.Last();
                        Assert.IsTrue(lastItem is int or string);
                        break;
                    }
            }
        }
    }

    [Test]
    public void IndexOfWorksAsExpected()
    {
        var testCases = new Dictionary<IEnumerable, int>
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
            //[CollectionSamples.GenericEnumerable] = 0,
            //[CollectionSamples.Enumerable] = 0,
            //[CollectionSamples.Array] = 0,
        };

        foreach (var (collection, _) in testCases)
        {
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
        var testCases = new Dictionary<IEnumerable, int>
        {
            [CollectionSamples.ReadOnlyDictionary] = 0,
            //[CollectionSamples.GenericDictionary] = 2,
            //[CollectionSamples.Dictionary] = 2,
            [CollectionSamples.ReadOnlyList] = 0,
            //[CollectionSamples.GenericList] = 1,
            //[CollectionSamples.List] = 1,
            [CollectionSamples.ReadOnlyCollection] = 0,
            //[CollectionSamples.GenericCollection] = 1,
            [CollectionSamples.Collection] = 0,
            //[CollectionSamples.GenericEnumerable] = 0,
            //[CollectionSamples.Enumerable] = 0,
            //[CollectionSamples.Array] = 0,
        };

        foreach (var (collection, expected) in testCases)
        {
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
        var testCases = new Dictionary<IEnumerable, int>
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

        foreach (var (collection, _) in testCases)
        {
            var proxy = collection.AsProxy();

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
        var testCases = new Dictionary<IEnumerable, int>
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

        foreach (var (collection, _) in testCases)
        {
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
        var testCases = new Dictionary<IEnumerable, int>
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
            //[CollectionSamples.GenericEnumerable] = 0,
            //[CollectionSamples.Enumerable] = 0,
            //[CollectionSamples.Array] = 0,
        };

        foreach (var (collection, _) in testCases)
        {
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
        var testCases = new Dictionary<IEnumerable, int>
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
            //[CollectionSamples.GenericEnumerable] = 0,
            //[CollectionSamples.Enumerable] = 0,
            //[CollectionSamples.Array] = 0,
        };

        foreach (var (collection, _) in testCases)
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
        var testCases = new Dictionary<IEnumerable, int>
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
            //[CollectionSamples.GenericEnumerable] = 0,
            //[CollectionSamples.Enumerable] = 0,
            //[CollectionSamples.Array] = 0,
        };

        foreach (var (collection, _) in testCases)
        {
            var proxy = collection.AsProxy();

            proxy.ForEach(kvp =>
            {
                Assert.IsTrue(kvp.Key is string or int);
                Assert.IsTrue(kvp.Value is int or char);
            });
        }
    }

    [Test]
    public void WithIndexOutOfRange_ThrowsException()
    {
        Dictionary<int, string> dict = new() {{0, "Zero"}, {1, "One"}, {2, "Two"}};

        var proxy = dict.AsProxy();

        Assert.Throws<ArgumentOutOfRangeException>(() => proxy.RemoveAt(5));
    }

    [Test]
    public void WithDictionariAsProxy_RemovesGivenKey()
    {
        Dictionary<int, string> dict = new() {{0, "Zero"}, {1, "One"}, {2, "Two"}};

        var proxy = dict.AsProxy();

        proxy.RemoveAt(1);

        foreach (var key in proxy.Keys)
        {
            Assert.AreNotEqual(1, key);
        }
    }

    [Test]
    public void WithNullArray_ThrowsException()
    {
        var array = new int[] {1, 2};
        int[] array2 = null;

        var proxy = array.AsProxy();

        Assert.Throws<ArgumentNullException>(() => proxy.CopyTo(array2, 0));
    }

    [Test]
    public void WithOutOfRangeIndex_ThrowsException()
    {
        var array = new int[] {1, 2};
        var array2 = new int[] {0, 0};

        var proxy = array.AsProxy();

        Assert.Throws<ArgumentOutOfRangeException>(() => proxy.CopyTo(array2, 4));
    }

    [Test]
    public void WithNullCollectionProxy_ReturnsFalse()
    {
        var array = new int[] {1, 2};
        CollectionProxy proxy2 = null;

        var proxy = array.AsProxy();

        Assert.IsFalse(proxy.SequenceEquals(proxy2));
    }

    [Test]
    public void WithDifferentCollections_ReturnsFalse()
    {
        var list1 = new List<int> {1, 2, 3, 4};
        var list2 = new List<int> {1, 2, 3, 5};

        var proxy = list1.AsProxy();
        var proxy2 = list2.AsProxy();

        Assert.IsFalse(proxy.SequenceEquals(proxy2));
    }

    [Test]
    public void WithTargetCollectionIsNull_ReturnsFalse()
    {
        var list1 = new List<int> {1, 2, 3, 4};

        var proxy = list1.AsProxy();
        CollectionProxy proxy2 = null;

        Assert.IsFalse(proxy.TryCopyTo(proxy2));
    }

    [Test]
    public void WithNoRepeatingKeysDictionaries_ReturnsTrue()
    {
        Dictionary<int, string> dict = new() {{0, "Zero"}, {1, "One"}, {2, "Two"}};

        Dictionary<int, string> dict2 = new() {{3, "Three"}, {4, "Four"}, {5, "Five"}};

        var proxy = dict.AsProxy();
        var proxy2 = dict2.AsProxy();

        Assert.IsTrue(proxy.TryCopyTo(proxy2));
    }

    [Test]
    public void WithNoRepeatingNumbersLists_ReturnsTrue()
    {
        var list1 = new List<int> {1, 2, 3, 4};
        var list2 = new List<int> {5, 6, 7, 8};

        var proxy = list1.AsProxy();
        var proxy2 = list2.AsProxy();

        Assert.IsTrue(proxy.TryCopyTo(proxy2));
    }
}

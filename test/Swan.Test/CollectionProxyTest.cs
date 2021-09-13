using NUnit.Framework;
using Swan.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Swan.Test
{
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
            var testCases = new Dictionary<CollectionKind, object>()
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
                if (!CollectionProxy.TryCreate(collection, out var proxy))
                    throw new InvalidOperationException("Cannot create collection proxy");

                Assert.AreEqual(kind, proxy.Kind);
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
                if (!CollectionProxy.TryCreate(collection, out var proxy))
                    throw new InvalidOperationException("Cannot create collection proxy");

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
                [CollectionSamples.Collection] = true,
                [CollectionSamples.GenericEnumerable] = true,
                [CollectionSamples.Enumerable] = true,
                [CollectionSamples.Array] = true,
            };

            var index = 0;
            foreach ((IEnumerable collection, var expected) in testCases)
            {
                if (!CollectionProxy.TryCreate(collection, out var proxy))
                    throw new InvalidOperationException("Cannot create collection proxy");

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
                if (!CollectionProxy.TryCreate(collection, out var proxy))
                    throw new InvalidOperationException("Cannot create collection proxy");

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
                if (!CollectionProxy.TryCreate(collection, out var proxy))
                    throw new InvalidOperationException("Cannot create collection proxy");

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
                [CollectionSamples.Collection] = true,
                [CollectionSamples.GenericEnumerable] = true,
                [CollectionSamples.Enumerable] = true,
                [CollectionSamples.Array] = true,
            };

            var index = -1;
            foreach ((IEnumerable collection, var expected) in testCases)
            {
                index++;

                if (!CollectionProxy.TryCreate(collection, out var proxy))
                    throw new InvalidOperationException("Cannot create collection proxy");

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

                if (!CollectionProxy.TryCreate(collection, out var proxy))
                    throw new InvalidOperationException("Cannot create collection proxy");

                Assert.IsTrue(proxy[7] is int or char);

                if (!proxy.Info.IsDictionary)
                    Assert.IsTrue(proxy["7"] is int or char);
            }
        }
    }
}

namespace Unosquare.Swan.Test.ExtensionsFunctionalTest
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ExtensionsFunctionalTest : TestFixtureBase
    {
        protected const string AdditionalName = "Sauron";

        protected static IEnumerable<string> Enumerable = new List<string> {"Aragorn", "Gimli", "Legolas", "Gandalf"};

        protected static List<object> Expected = new List<object>
        {
            "Aragorn",
            "Gimli",
            "Legolas",
            "Gandalf",
            AdditionalName,
        };

        protected IQueryable<string> Queryable => Enumerable.AsQueryable();

        protected List<string> List => Enumerable.ToList();

        protected IEnumerable<string> AddName(IEnumerable<string> input) => input.Concat(new[] {AdditionalName});

        protected IQueryable<string> AddName(IQueryable<string> input) => input.Concat(new[] {AdditionalName});

        protected string AddName() => AdditionalName;

        protected IEnumerable<string> AddRange() => new[] {AdditionalName};
    }

    [TestFixture]
    public class When : ExtensionsFunctionalTest
    {
        [Test]
        public void WithIEnumerableAndConditionEqualsTrue_ReturnsIEnumerable()
        {
            var whenResult = Enumerable.When(() => true, AddName);

            Assert.AreEqual(Expected, whenResult);
        }

        [Test]
        public void WithIEnumerableAndConditionEqualsFalse_ReturnsIEnumerable()
        {
            var whenResult = Enumerable.When(() => false, AddName);

            Assert.AreEqual(Enumerable, whenResult);
        }

        [Test]
        public void WithNullIEnumerable_ThrowsArgumentNullException()
        {
            IEnumerable<string> enumerable = null;

            Assert.Throws<ArgumentNullException>(() =>
                enumerable.When(() => false, AddName));
        }

        [Test]
        public void WithIEnumerableAndNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Enumerable.When(null, AddName));
        }

        [Test]
        public void WithIEnumerableAndNullFunction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Enumerable.When(() => false, null));
        }

        [Test]
        public void WithIQueryableAndConditionEqualsTrue_ReturnsIQueryable()
        {
            var whenResult = Queryable.When(() => true, AddName);

            Assert.AreEqual(Expected, whenResult);
        }

        [Test]
        public void WithIQueryableAndConditionEqualsFalse_ReturnsIQueryable()
        {
            var whenResult = Queryable.When(() => false, AddName);

            Assert.AreEqual(Queryable, whenResult);
        }

        [Test]
        public void WithNullIQueryable_ThrowsArgumentNullException()
        {
            IQueryable<string> queryable = null;

            Assert.Throws<ArgumentNullException>(() =>
                queryable.When(() => false, AddName));
        }

        [Test]
        public void WithIQueryableAndNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Queryable.When(null, AddName));
        }

        [Test]
        public void WithIQueryableAndNullFunction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Queryable.When(() => false, null));
        }
    }

    [TestFixture]
    public class AddWhen : ExtensionsFunctionalTest
    {
        [Test]
        public void WithMethodCallAndConditionEqualsTrue_ReturnsListWithAddedItem()
        {
            var whenResult = List.AddWhen(() => true, AddName);

            Assert.AreEqual(Expected, whenResult);
        }

        [Test]
        public void WithMethodCallAndConditionEqualsFalse_ReturnsSameList()
        {
            var whenResult = List.AddWhen(() => false, AddName);

            Assert.AreEqual(Enumerable, whenResult);
        }

        [Test]
        public void WithMethodCallAndNullIList_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullStringList.AddWhen(() => true, AddName));
        }

        [Test]
        public void WithMethodCallAndNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => List.AddWhen(null, AddName));
        }

        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => List.AddWhen(() => true, null));
        }

        [Test]
        public void WithConditionEqualsTrue_ReturnsListWithAddedItem()
        {
            var whenResult = List.AddWhen(true, AdditionalName);

            Assert.AreEqual(Expected, whenResult);
        }

        [Test]
        public void WithConditionEqualsFalse_ReturnsListWithAddedItem()
        {
            var whenResult = List.AddWhen(false, AdditionalName);

            Assert.AreEqual(Enumerable, whenResult);
        }

        [Test]
        public void WithNullIList_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullStringList.AddWhen(true, AdditionalName));
        }
    }

    [TestFixture]
    public class AddRangeWhen : ExtensionsFunctionalTest
    {
        [Test]
        public void WithConditionEqualsTrue_ReturnsListWithAddedRange()
        {
            var whenResult = List.AddRangeWhen(() => true, AddRange);

            Assert.AreEqual(Expected, whenResult);
        }

        [Test]
        public void WithConditionEqualsFalse_ReturnsSameList()
        {
            var whenResult = List.AddRangeWhen(() => false, AddRange);

            Assert.AreEqual(Enumerable, whenResult);
        }

        [Test]
        public void WithNullList_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullStringList.AddRangeWhen(() => true, AddRange));
        }

        [Test]
        public void WithNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                List.AddRangeWhen(null, AddRange));
        }

        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                List.AddRangeWhen(() => true, null));
        }
    }
}
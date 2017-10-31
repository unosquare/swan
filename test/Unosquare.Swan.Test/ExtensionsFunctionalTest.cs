using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Unosquare.Swan.Test.ExtensionsFunctionalTest
{
    public abstract class ExtensionsFunctionalTest
    {
        protected static List<string> Names = new List<string> {"Aragorn", "Gimli", "Legolas", "Gandalf"};

        protected readonly IEnumerable<string> Enumerable = Names.AsEnumerable();
        protected readonly IQueryable<string> Queryable = Names.AsQueryable();

        protected IEnumerable<string> AddName(IEnumerable<string> input) =>
            input.AsEnumerable().Concat(new[] {"Sauron"});

        protected IQueryable<string> AddName(IQueryable<string> input) => input.AsQueryable().Concat(new[] {"Sauron"});

        protected string AddName() => "Sauron";

        protected IEnumerable<string> AddRange() => new[] {"Frodo", "Sam"};
    }

    [TestFixture]
    public class When : ExtensionsFunctionalTest
    {
        public static List<object> expected = new List<object> {"Aragorn", "Gimli", "Legolas", "Gandalf", "Sauron"};

        [Test]
        public void WithIEnumerableAndConditionEqualsTrue_ReturnsIEnumerable()
        {
            var whenResult = Enumerable.When(() => true, AddName);

            Assert.AreEqual(expected, whenResult);
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
                enumerable.When(() => false, AddName)
            );
        }

        [Test]
        public void WithIEnumerableAndNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Enumerable.When(null, AddName)
            );
        }

        [Test]
        public void WithIEnumerableAndNullFunction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Enumerable.When(() => false, null)
            );
        }

        [Test]
        public void WithIQueryableAndConditionEqualsTrue_ReturnsIQueryable()
        {
            var whenResult = Queryable.When(() => true, AddName);

            Assert.AreNotEqual(expected, whenResult);
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
                queryable.When(() => false, AddName)
            );
        }

        [Test]
        public void WithIQueryableAndNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Queryable.When(null, AddName)
            );
        }

        [Test]
        public void WithIQueryableAndNullFunction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Queryable.When(() => false, null)
            );
        }
    }

    [TestFixture]
    public class AddWhen : ExtensionsFunctionalTest
    {
        List<object> expected = new List<object> {"Aragorn", "Gimli", "Legolas", "Gandalf", "Frodo", "Sam", "Arwen"};

        [Test]
        public void WithMethodCallAndConditionEqualsTrue_ReturnsListWithAddedItem()
        {
            var expected =
                new List<object> {"Aragorn", "Gimli", "Legolas", "Gandalf", "Frodo", "Sam", "Arwen", "Sauron"};

            var whenResult = Names.AddWhen(() => true, AddName);

            Assert.AreEqual(expected, whenResult);
        }

        [Test]
        public void WithMethodCallAndConditionEqualsFalse_ReturnsSameList()
        {
            var whenResult = Names.AddWhen(() => false, AddName);

            Assert.AreEqual(expected, whenResult);
        }

        [Test]
        public void WithMethodCallAndNullIList_ThrowsArgumentNullException()
        {
            IList<object> list = null;

            Assert.Throws<ArgumentNullException>(() =>
                list.AddWhen(() => true, AddName)
            );
        }

        [Test]
        public void WithMethodCallAndNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Names.AddWhen(null, AddName)
            );
        }

        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Names.AddWhen(() => true, null)
            );
        }

        [Test]
        public void WithConditionEqualsTrue_ReturnsListWithAddedItem()
        {
            var whenResult = Names.AddWhen(true, "Arwen");

            Assert.AreEqual(expected, whenResult);
        }

        [Test]
        public void WithConditionEqualsFalse_ReturnsListWithAddedItem()
        {
            var whenResult = Names.AddWhen(false, "Arwen");

            Assert.AreEqual(Names, whenResult);
        }

        [Test]
        public void WithNullIList_ThrowsArgumentNullException()
        {
            IList<object> list = null;

            Assert.Throws<ArgumentNullException>(() =>
                list.AddWhen(true, "Arwen")
            );
        }

    }

    [TestFixture]
    public class AddRangeWhen : ExtensionsFunctionalTest
    {
        [Test]
        public void WithConditionEqualsTrue_ReturnsListWithAddedRange()
        {
            var expected = new List<object> {"Aragorn", "Gimli", "Legolas", "Gandalf", "Frodo", "Sam"};

            var whenResult = Names.AddRangeWhen(() => true, AddRange);

            Assert.AreEqual(expected, Names);
        }

        [Test]
        public void WithConditionEqualsFalse_ReturnsSameList()
        {
            var whenResult = Names.AddRangeWhen(() => false, AddRange);

            Assert.AreEqual(Names, Names);
        }

        [Test]
        public void WithNullList_ThrowsArgumentNullException()
        {
            List<object> list = null;

            Assert.Throws<ArgumentNullException>(() =>
                list.AddRangeWhen(() => true, AddRange)
            );
        }

        [Test]
        public void WithNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Names.AddRangeWhen(null, AddRange)
            );
        }

        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Names.AddRangeWhen(() => true, null)
            );
        }
    }
}
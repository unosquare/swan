using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Unosquare.Swan.Test.ExtensionsFunctionalTest
{
    public abstract class ExtensionsFunctionalTest
    {
        protected static IList<object> _names = new List<object> { "Aragorn", "Gimli", "Legolas", "Gandalf"};
        protected static List<object> _hobbits = new List<object> { "Frodo", "Sam", "Pippin" };

        protected readonly IEnumerable<object> _enumerable = _names.AsEnumerable();
        protected readonly IQueryable<object> _queryable = _names.AsQueryable();
        
        protected IEnumerable<object> AddName(IEnumerable<object> input)
        {
            var names = input.AsEnumerable().Concat(new[] { "Galadriel", "Elrond" });

            return names;
        }

        protected IQueryable<object> AddName(IQueryable<object> input)
        {
            var names = input.AsQueryable().Concat(new[] { "Galadriel", "Elrond" });
            
            return names;
        }

        protected string AddName()
        {
            var name = "Sauron";
            
            return name;
        }

        protected IEnumerable<object> AddRange()
        {
            var names = new[] { "Merry", "Bilbo" };

            return names;
        }
    }

    [TestFixture]
    public class When : ExtensionsFunctionalTest
    {
        [Test]
        public void WithIEnumerableAndConditionEqualsTrue_ReturnsIEnumerable()
        {
            var whenResult = _enumerable.When(() => true, AddName);
            
            Assert.AreNotEqual(whenResult, _enumerable);
        }

        [Test]
        public void WithIEnumerableAndConditionEqualsFalse_ReturnsIEnumerable()
        {
            var whenResult = _enumerable.When(() => false, AddName);

            Assert.AreEqual(whenResult, _enumerable);
        }

        [Test]
        public void WithNullIEnumerable_ThrowsArgumentNullException()
        {
            IEnumerable<object> enumerable = null;

            Assert.Throws<ArgumentNullException>(() =>
                enumerable.When(() => false, AddName)
            );
        }

        [Test]
        public void WithIEnumerableAndNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _enumerable.When(null, AddName)
            );
        }

        [Test]
        public void WithIEnumerableAndNullFunction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _enumerable.When(() => false, null)
            );
        }

        [Test]
        public void WithIQueryableAndConditionEqualsTrue_ReturnsIQueryable()
        {
            var whenResult = _queryable.When(() => true, AddName);

            Assert.AreNotEqual(whenResult, _queryable);
        }

        [Test]
        public void WithIQueryableAndConditionEqualsFalse_ReturnsIQueryable()
        {
            var whenResult = _queryable.When(() => false, AddName);

            Assert.AreEqual(whenResult, _queryable);
        }

        [Test]
        public void WithNullIQueryable_ThrowsArgumentNullException()
        {
            IQueryable<object> queryable = null;

            Assert.Throws<ArgumentNullException>(() =>
                queryable.When(() => false, AddName)
            );
        }

        [Test]
        public void WithIQueryableAndNullCondition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _queryable.When(null, AddName)
            );
        }

        [Test]
        public void WithIQueryableAndNullFunction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _queryable.When(() => false, null)
            );
        }
    }

    [TestFixture]
    public class AddWhen : ExtensionsFunctionalTest
    {
        List<object> expected = new List<object> { "Aragorn", "Gimli", "Legolas", "Gandalf", "Arwen" };

        [Test]
        public void WithMethodCallAndConditionEqualsTrue_ReturnsListWithAddedItem()
        {
            var expected = new List<object> { "Aragorn", "Gimli", "Legolas", "Gandalf", "Arwen", "Sauron" };
            
            var whenResult = _names.AddWhen(() => true, AddName);
            
            Assert.AreEqual(expected, whenResult);
        }

        [Test]
        public void WithMethodCallAndConditionEqualsFalse_ReturnsSameList()
        {
            var whenResult = _names.AddWhen(() => false, AddName);

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
            Func<string> methodCall = AddName;

            Assert.Throws<ArgumentNullException>(() =>
                _names.AddWhen(null, methodCall)
            );
        }

        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _names.AddWhen(() => true, null)
            );
        }

        [Test]
        public void WithConditionEqualsTrue_ReturnsListWithAddedItem()
        {
            var whenResult = _names.AddWhen(true, "Arwen");

            Assert.AreEqual(expected, whenResult);
        }

        [Test]
        public void WithConditionEqualsFalse_ReturnsListWithAddedItem()
        {
            var whenResult = _names.AddWhen(false, "Arwen");

            Assert.AreEqual(_names, whenResult);
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
            var expected = new List<object> { "Frodo", "Sam", "Pippin", "Merry", "Bilbo" };
            
            var whenResult = _hobbits.AddRangeWhen(() => true, AddRange);
            
            Assert.AreEqual(expected, _hobbits);
        }

        [Test]
        public void WithConditionEqualsFalse_ReturnsSameList()
        {
            var whenResult = _hobbits.AddRangeWhen(() => false, AddRange);

            Assert.AreEqual(_hobbits, _hobbits);
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
            Func<IEnumerable<object>> methodCall = AddRange;

            Assert.Throws<ArgumentNullException>(() =>
                 _hobbits.AddRangeWhen(null, methodCall)
            );
        }

        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                 _hobbits.AddRangeWhen(() => true, null)
            );
        }

    }
}

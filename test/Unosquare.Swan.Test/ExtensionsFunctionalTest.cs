using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.ExtensionsFunctionalTest
{
    public abstract class ExtensionsFunctionalTest
    {
        protected static IList<object> _names = new List<object> { "Aragorn", "Gimli", "Legolas", "Gandalf"};
        protected static IList<object> _cities = new List<object> { "Edoras", "Minas Tirith", "Moria" };
        protected static List<object> _hobbits = new List<object> { "Frodo", "Sam", "Pippin" };

        protected readonly IEnumerable<object> _enumerable = _names.AsEnumerable();
        protected readonly IQueryable<object> _queryable = _names.AsQueryable();
        
        protected IEnumerable<object> Function(IEnumerable<object> input)
        {
            var names = input.AsEnumerable().Concat(new[] { "Galadriel", "Elrond", "Sauron" });

            return names;
        }

        protected IQueryable<object> Function(IQueryable<object> input)
        {
            var names = input.AsQueryable().Concat(new[] { "Galadriel", "Elrond", "Sauron" });
            
            return names;
        }

        protected string Function()
        {
            var name = "Rivendell";
            
            return name;
        }

        protected IEnumerable<object> FunctionAddRange()
        {
            var names = new[] { "Merry", "Bilbo" };

            return names;
        }
    }

    [TestFixture]
    public class When : ExtensionsFunctionalTest
    {
        [Test]
        public void WithIEnumerableAndMethodCallEqualsTrue_IEnumerable()
        {
            var whenResult = _enumerable.When(() => true, Function);
            
            Assert.AreNotEqual(whenResult, _enumerable);
        }

        [Test]
        public void WithIEnumerableAndMethodCallEqualsFalse_IEnumerable()
        {
            var whenResult = _enumerable.When(() => false, Function);

            Assert.AreEqual(whenResult, _enumerable);
        }
        
        [Test]
        public void WithIQueryableAndMethodCallEqualsTrue_IEnumerable()
        {
            var whenResult = _queryable.When(() => true, Function);

            Assert.AreNotEqual(whenResult, _queryable);
        }

        [Test]
        public void WithIQueryableAndMethodCallEqualsFalse_IEnumerable()
        {
            var whenResult = _queryable.When(() => false, Function);

            Assert.AreEqual(whenResult, _queryable);
        }
    }

    [TestFixture]
    public class AddWhen : ExtensionsFunctionalTest
    {
        [Test]
        public void WithMethodCallAndConditionEqualsTrue_ReturnsObjectWithAddedItem()
        {
            IList<object> expected = new List<object> { "Edoras", "Minas Tirith", "Moria", "Minas Morgul", "Rivendell" };
            
            var whenResult = _cities.AddWhen(() => true, Function);
            
            Assert.AreEqual(expected, whenResult);
        }
        
        [Test]
        public void WithMethodCallAndNullIList_ThrowsArgumentNullException()
        {
            IList<object> list = null;

            Assert.Throws<ArgumentNullException>(() =>
                list.AddWhen(() => true, Function)
            );
        }

        [Test]
        public void WithMethodCallAndNullCondition_ThrowsArgumentNullException()
        {
            Func<string> methodCall = Function;

            Assert.Throws<ArgumentNullException>(() =>
                _cities.AddWhen(null, methodCall)
            );
        }

        [Test]
        public void WithNullValueAndFuncBool_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _cities.AddWhen(() => true, null)
            );
        }

        [Test]
        public void WithConditionEqualsTrue_ReturnsObjectWithAddedItem()
        {
            IList<object> expected = new List<object> { "Edoras", "Minas Tirith", "Moria", "Minas Morgul" };
            
            var whenResult = _cities.AddWhen(true, "Minas Morgul");

            Assert.AreEqual(expected, whenResult);
        }

        [Test]
        public void WithNullIList_ThrowsArgumentNullException()
        {
            IList<object> list = null;

            Assert.Throws<ArgumentNullException>(() =>
                list.AddWhen(true, "Minas Morgul")
            );
        }
        
    }

    [TestFixture]
    public class AddRangeWhen : ExtensionsFunctionalTest
    {
        [Test]
        public void WithIEnumerableAndMethodCallEqualsTrue_IEnumerable()
        {
            List<object> expected = new List<object> { "Frodo", "Sam", "Pippin", "Merry", "Bilbo" };
            
            var whenResult = _hobbits.AddRangeWhen(() => true, FunctionAddRange);
            
            Assert.AreEqual(expected, _hobbits);
        }

        [Test]
        public void WithNullList_ThrowsArgumentNullException()
        {
            List<object> list = null;

            Assert.Throws<ArgumentNullException>(() =>
                 list.AddRangeWhen(() => true, FunctionAddRange)
            );
        }

        [Test]
        public void WithNullCondition_ThrowsArgumentNullException()
        {
            Func<IEnumerable<object>> methodCall = FunctionAddRange;

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

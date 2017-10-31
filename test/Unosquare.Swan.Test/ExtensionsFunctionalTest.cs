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
        protected static IList<object> _cities = new List<object> { "Edoras", "Minas Tirith", "Moria" };
        protected static IList<object> _names = new List<object> { "Aragorn", "Gimli", "Legolas", "Gandalf"};
        protected readonly IEnumerable<object> _enumerable = _names.AsEnumerable();
        protected readonly IQueryable<object> _queryable = _names.AsQueryable();
        
        protected IEnumerable<object> Function(IEnumerable<object> input)
        {
            var names = input.AsEnumerable().Concat(new[] { "Frodo", "Sam", "Pippin", "Merry" });

            return names;
        }

        protected IQueryable<object> Function(IQueryable<object> input)
        {
            var names = input.AsQueryable().Concat(new[] { "Frodo", "Sam", "Pippin", "Merry" });
            
            return names;
        }

        protected string Function()
        {
            var name = "Rivendell";
            
            return name;
        }
    }

    [TestFixture]
    public class When : ExtensionsFunctionalTest
    {
        [Test]
        public void WithIEnumerableAndMethodCallEqualsTrue_IEnumerable()
        {
            Func<bool> condition = () => true;
            Func<IEnumerable<object>, IEnumerable<object>> methodCall = Function;
            
            var whenResult = _enumerable.When(condition, methodCall);
            
            Assert.AreNotEqual(whenResult, _enumerable);
        }

        [Test]
        public void WithIEnumerableAndMethodCallEqualsFalse_IEnumerable()
        {
            Func<bool> condition = () => false;
            Func<IEnumerable<object>, IEnumerable<object>> methodCall = Function;

            var whenResult = _enumerable.When(condition, methodCall);
            
            Assert.AreEqual(whenResult, _enumerable);
        }
        
        [Test]
        public void WithIQueryableAndMethodCallEqualsTrue_IEnumerable()
        {
            Func<bool> condition = () => true;
            Func<IQueryable<object>, IQueryable<object>> methodCall = Function;

            var whenResult = _queryable.When(condition, methodCall);
            
            Assert.AreNotEqual(whenResult, _queryable);
        }

        [Test]
        public void WithIQueryableAndMethodCallEqualsFalse_IEnumerable()
        {
            Func<bool> condition = () => false;
            Func<IQueryable<object>, IQueryable<object>> methodCall = Function;

            var whenResult = _queryable.When(condition, methodCall);

            Assert.AreEqual(whenResult, _queryable);
        }
    }

    [TestFixture]
    public class AddWhen : ExtensionsFunctionalTest
    {
        IList<object> expected = new List<object> { "Edoras", "Minas Tirith", "Moria", "Rivendell" };

        [Test]
        public void WithConditionEqualsTrue_ReturnsObjectWithAddedItem()
        {
            Func<bool> condition = () => true;
            Func<string> methodCall = Function;

            var whenResult = _cities.AddWhen(condition, methodCall);
            
            Assert.AreEqual(expected, whenResult);
        }

        [Test]
        public void WithNullIList_ThrowsArgumentNullException()
        {
            IList<object> list = null;
            Func<bool> condition = () => true;
            Func<string> methodCall = Function;

            Assert.Throws<ArgumentNullException>(() =>
                list.AddWhen(condition, methodCall)
            );
        }

        [Test]
        public void WithNullCondition_ThrowsArgumentNullException()
        {
            Func<string> methodCall = Function;

            Assert.Throws<ArgumentNullException>(() =>
                _cities.AddWhen(null, methodCall)
            );
        }

        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Func<bool> condition = () => true;

            Assert.Throws<ArgumentNullException>(() =>
                _cities.AddWhen(condition, null)
            );
        }
    }
    
}

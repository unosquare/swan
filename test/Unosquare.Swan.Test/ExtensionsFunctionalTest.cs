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
        protected readonly IEnumerable<string> _enumerable = _names.AsQueryable().Cast<string>();
        protected readonly IQueryable<string> _queryable = _names.AsQueryable().Cast<string>();

        protected bool ReturnTrue()
        {
            return true;
        }

        protected bool ReturnFalse()
        {
            return false;
        }

        protected IEnumerable<string> Function(IEnumerable<string> input)
        {
            IEnumerable<string> names = input.AsEnumerable().Concat(new[] { "Frodo", "Sam", "Pippin", "Merry" });

            return names;
        }

        protected IQueryable<string> Function(IQueryable<string> input)
        {
            IQueryable<string> names = input.AsQueryable().Concat(new[] { "Frodo", "Sam", "Pippin", "Merry" });
            
            return names;
        }

        protected string Function()
        {
            string name = "Arwen";
            
            return name;
        }
    }

    [TestFixture]
    public class When : ExtensionsFunctionalTest
    {
        [Test]
        public void WithIEnumerableAndMethodCallEqualsTrue_IEnumerable()
        {
            Func<bool> condition = ReturnTrue;
            Func<IEnumerable<string>, IEnumerable<string>> methodCall = Function;
            
            var whenResult = _enumerable.When(condition, methodCall);

            foreach(var item in whenResult)
            {
                Console.WriteLine(item);
            }

            Assert.AreNotEqual(whenResult, _enumerable);
        }

        [Test]
        public void WithIEnumerableAndMethodCallEqualsFalse_IEnumerable()
        {
            Func<bool> condition = ReturnFalse;
            Func<IEnumerable<string>, IEnumerable<string>> methodCall = Function;

            var whenResult = _enumerable.When(condition, methodCall);

            foreach(var item in whenResult)
            {
                Console.WriteLine(item);
            }

            Assert.AreEqual(whenResult, _enumerable);
        }


        [Test]
        public void WithIQueryableAndMethodCallEqualsFalse_IEnumerable()
        {
            Func<bool> condition = ReturnFalse;
            Func<IQueryable<string>, IQueryable<string>> methodCall = Function;

            var whenResult = _queryable.When(condition, methodCall);
            
            Assert.AreEqual(whenResult, _queryable);
        }

        [Test]
        public void WithIQueryableAndMethodCallEqualsTrue_IEnumerable()
        {
            Func<bool> condition = ReturnTrue;
            Func<IQueryable<string>, IQueryable<string>> methodCall = Function;

            var whenResult = _queryable.When(condition, methodCall);
            
            Assert.AreNotEqual(whenResult, _queryable);
        }
    }

    [TestFixture]
    public class AddWhen : ExtensionsFunctionalTest
    {
        [Test]
        public void WithValidParams_ThrowsArgumentNullException()
        {
            Func<bool> condition = ReturnTrue;
            Func<string> methodCall = Function;

            var whenResult = _names.AddWhen(condition, methodCall);
           
            Assert.IsNotNull(null);
        }

        [Test]
        public void WithNullIList_ThrowsArgumentNullException()
        {
            IList<object> list = null;
            Func<bool> condition = ReturnTrue;
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
                _names.AddWhen(null, methodCall)
            );
        }

        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Func<bool> condition = ReturnTrue;

            Assert.Throws<ArgumentNullException>(() =>
                _names.AddWhen(condition, null)
            );
        }
    }


}

namespace Unosquare.Swan.Test.ObjectComparerTests
{
    using NUnit.Framework;
    using System;
    using Components;
    using Mocks;

    [TestFixture]
    public class ObjectValidatorInstance
    {
        [Test]
        public void NullPredicate_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            Runtime.ObjectValidator.Value.AddValidator<SimpleValidationMock>(null, "as"));
        }

        [Test]
        public void NullObject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Runtime.ObjectValidator.Value.Validate(null as SimpleValidationMock));
        }

        [Test]
        public void ValidObject_ReturnsTrue()
        {
            var obj = new ObjectValidator();
            obj.AddValidator<SimpleValidationMock>(x => !string.IsNullOrEmpty(x.Name), "Name must not be empty");
            var res = obj.Validate(new SimpleValidationMock { Name = "" });

            Assert.IsTrue(res.Count > 0);
        }

        [Test]
        public void InvalidObject_ReturnsFalse()
        {
            var obj = new ObjectValidator();
            var res = obj.Validate(new RangeMock { Age = 3 , Kilograms = 0 });
            Assert.That(res.Count, Is.EqualTo(1));
        }
    }
}

//    [TestFixture]
//    public class ObjectValidatorAttributes
//    {
//        [Test]
//        public void NullObject_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => ObjectValidator.IsValid(null as SimpleValidationMock));            
//        }
//    }

//    [TestFixture]
//    public class NotNullAttribute
//    {
//        [Test]
//        public void NullProperty_ReturnsFalse()
//        {
//            var res = ObjectValidator.IsValid(new NotNullMock());
//            Assert.IsFalse(res);
//        }

//        [Test]
//        public void NotNullProperty_ReturnsTrue()
//        {
//            var res = ObjectValidator.IsValid(new NotNullMock { Number = 12});
//            Assert.IsTrue(res);
//        }
//    }

//    [TestFixture]
//    public class RangeAttribute
//    {
//        [Test]
//        public void ValueWithinRange_ReturnsTrue()
//        {
//            var res = ObjectValidator.IsValid(new RangeMock { Age = 5 , Kilograms = 0.5});
//            Assert.IsTrue(res);
//        }

//        [Test]
//        public void ValueOutsideRange_ReturnsFalse()
//        {
//            var res = ObjectValidator.IsValid(new RangeMock { Age = 0 });
//            Assert.IsFalse(res);
//        }

//        [Test]
//        public void InvalidType_ReturnsFalse()
//        {
//            var res = ObjectValidator.IsValid(new InvalidRangeMock { Invalid = "inv" });
//            Assert.IsFalse(res);
//        }
//    }

//    [TestFixture]
//    public class RegexAttribute
//    {
//        [TestCase("hi", true)]
//        [TestCase("Hi", false)]
//        public void StringValidation(string salute,bool valid)
//        {
//            ObjectValidator.IsValid(new RegexMock());
//            var res = ObjectValidator.IsValid(new RegexMock { Salute = salute });
//            Assert.That(valid, Is.EqualTo(res));
//        }

//        [Test]
//        public void NullString_ReturnsFalse()
//        {
//            var res = ObjectValidator.IsValid(new RegexMock { Salute = null });
//            Assert.IsFalse(res);
//        }

//        [Test]
//        public void NotStringType_ThrowsInvalidOperationException()
//        {
//            Assert.Throws<InvalidOperationException>(() => ObjectValidator.IsValid(new InvalidRegexMock { Salute = 1 }));
//        }
//    }
//}

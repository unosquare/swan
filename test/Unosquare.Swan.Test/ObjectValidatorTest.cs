namespace Unosquare.Swan.Test.ObjectComparerTests
{
    using Components;
    using Mocks;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class ObjectValidatorInstance
    {
        [Test]
        public void NullPredicate_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            Runtime.ObjectValidator.AddValidator<SimpleValidationMock>(null, "as"));
        }

        [Test]
        public void NullObject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Runtime.ObjectValidator.Validate(null as SimpleValidationMock));
        }

        [Test]
        public void ValidObject_ReturnsNoErrors()
        {
            var obj = new ObjectValidator();
            obj.AddValidator<SimpleValidationMock>(x => !string.IsNullOrEmpty(x.Name), "Name must not be empty");
            var res = obj.Validate(new SimpleValidationMock { Name = "Name" });

            Assert.IsTrue(res.IsValid);
        }

        [Test]
        public void InvalidObject_ReturnsErrors()
        {
            var obj = new ObjectValidator();
            var res = obj.Validate(new RangeMock { Age = 3, Kilograms = 0 });

            Assert.IsFalse(res.IsValid);
            Assert.That(res.Errors.Count, Is.EqualTo(1));
            Assert.AreEqual(res.Errors.First().PropertyName, nameof(RangeMock.Kilograms));
            Assert.AreEqual(res.Errors.First().ErrorMessage, "Value is not within the specified range");
        }

        [Test]
        public void IsValidObject_ReturnsTrue()
        {
            var obj = new ObjectValidator();
            Assert.IsTrue(obj.IsValid(new RangeMock {Age = 3, Kilograms = 1}));
        }
    }

    [TestFixture]
    public class ObjectValidatorAttributes
    {
        [Test]
        public void NullObject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            Runtime.ObjectValidator.Validate(null as SimpleValidationMock));
        }
    }

    [TestFixture]
    public class NotNullAttribute
    {        
        [Test]
        public void NullProperty_ReturnsErrors()
        {
            var res = Runtime.ObjectValidator.Validate(new NotNullMock());
            Assert.IsFalse(res.IsValid);
            Assert.That(res.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void NotNullProperty_ReturnsNoErrors()
        {
            var res = Runtime.ObjectValidator.Validate(new NotNullMock { Number = 12 });
            Assert.IsTrue(res.IsValid);
            Assert.That(res.Errors.Count, Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class RangeAttribute
    {
        [Test]
        public void ValueWithinRange_ReturnsNoErrors()
        {
            var res = Runtime.ObjectValidator.Validate(new RangeMock { Age = 5, Kilograms = 0.5 });
            Assert.IsTrue(res.IsValid);
        }

        [Test]
        public void ValueOutsideRange_ReturnsErrors()
        {
            var res = Runtime.ObjectValidator.Validate(new RangeMock { Age = 0 });
            Assert.IsFalse(res.IsValid);
            Assert.That(res.Errors.Count, Is.EqualTo(2));
        }

        [Test]
        public void InvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Runtime.ObjectValidator.Validate(new InvalidRangeMock { Invalid = "inv" }));
        }
    }

    [TestFixture]
    public class RegexAttribute
    {
        [TestCase("hi", true, 0)]
        [TestCase("Hi", false, 1)]
        public void StringValidation(string salute, bool valid, int count)
        {
            var res = Runtime.ObjectValidator.Validate(new RegexMock { Salute = salute });
            Assert.That(valid, Is.EqualTo(res.IsValid));
            Assert.That(res.Errors.Count, Is.EqualTo(count));
        }

        [Test]
        public void NullString_ReturnsErrors()
        {
            var res = Runtime.ObjectValidator.Validate(new RegexMock { Salute = null });
            Assert.IsFalse(res.IsValid);
            Assert.AreEqual(res.Errors.First().ErrorMessage, "String does not match the specified regular expression");
        }

        [Test]
        public void NotStringType_ThrowsInvalidOperationException()
        {
            Assert.Throws<ArgumentException>(() => Runtime.ObjectValidator.Validate(new InvalidRegexMock { Salute = 1 }));
        }
    }

    [TestFixture]
    public class EmailAttribute
    {
        [TestCase("test@test.com", true, 0)]
        [TestCase("test", false, 1)]
        public void StringValidation(string to, bool valid, int count)
        {
            var res = Runtime.ObjectValidator.Validate(new EmailMock { To = to });
            Assert.That(valid, Is.EqualTo(res.IsValid));
            Assert.That(res.Errors.Count, Is.EqualTo(count));
        }

        [Test]
        public void NullString_ReturnsErrors()
        {
            var res = Runtime.ObjectValidator.Validate(new EmailMock { To = null });
            Assert.IsFalse(res.IsValid);
            Assert.AreEqual(res.Errors.First().ErrorMessage, "String is not an email");
        }
    }
}
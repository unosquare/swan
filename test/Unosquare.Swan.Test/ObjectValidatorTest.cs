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
        }
    }

    [TestFixture]
    public class ObjectValidatorAttributes
    {
        [Test]
        public void NullObject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            Runtime.ObjectValidator.Value.Validate(null as SimpleValidationMock));
        }
    }

    [TestFixture]
    public class NotNullAttribute
    {        
        public void NullProperty_ReturnsErrors()
        {
            var res = Runtime.ObjectValidator.Value.Validate(new NotNullMock());
            Assert.IsFalse(res.IsValid);
            Assert.That(res.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void NotNullProperty_ReturnsNoErrors()
        {
            var res = Runtime.ObjectValidator.Value.Validate(new NotNullMock { Number = 12 });
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
            var res = Runtime.ObjectValidator.Value.Validate(new RangeMock { Age = 5, Kilograms = 0.5 });
            Assert.IsTrue(res.IsValid);
        }

        [Test]
        public void ValueOutsideRange_ReturnsErrors()
        {
            var res = Runtime.ObjectValidator.Value.Validate(new RangeMock { Age = 0 });
            Assert.IsFalse(res.IsValid);
            Assert.That(res.Errors.Count, Is.EqualTo(2));
        }

        [Test]
        public void InvalidType_ReturnsErrors()
        {
            var res = Runtime.ObjectValidator.Value.Validate(new InvalidRangeMock { Invalid = "inv" });
            Assert.IsFalse(res.IsValid);
            Assert.That(res.Errors.Count, Is.EqualTo(1));
        }
    }

    [TestFixture]
    public class RegexAttribute
    {
        [TestCase("hi", true, 0)]
        [TestCase("Hi", false, 1)]
        public void StringValidation(string salute, bool valid, int count)
        {
            var res = Runtime.ObjectValidator.Value.Validate(new RegexMock { Salute = salute });
            Assert.That(valid, Is.EqualTo(res.IsValid));
            Assert.That(res.Errors.Count, Is.EqualTo(count));
        }

        [Test]
        public void NullString_ReturnsErrors()
        {
            var res = Runtime.ObjectValidator.Value.Validate(new RegexMock { Salute = null });
            Assert.IsFalse(res.IsValid);
        }

        [Test]
        public void NotStringType_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Runtime.ObjectValidator.Value.Validate(new InvalidRegexMock { Salute = 1 }));
        }
    }
}
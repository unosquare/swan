using NUnit.Framework;
using Swan.Reflection;
using Swan.Test.Mocks;
using Swan.Validation;
using System;
using System.Linq;
using System.Reflection;

namespace Swan.Test
{
    [TestFixture]
    public class ConstrainedRetrieve
    {
        [Test]
        public void ValidMember_ReturnsProperties()
        {
            var member = typeof(RegexMock).TypeInfo().Properties[nameof(RegexMock.Salute)];
            var attributes = member.Attribute<MatchAttribute>();

            Assert.NotNull(attributes);
        }

        [Test]
        public void PropertyWithNoMatchingAttributes_ReturnsZeroProperties()
        {
            var member = typeof(NotNullMock).TypeInfo().Properties[nameof(NotNullMock.Number)];
            var attributes = member.Attribute<MatchAttribute>();

            Assert.IsNull(attributes);
        }

        [Test]
        public void RetrievePropertiesWithNullType_ReturnsNull()
        {
            Assert.IsNull(typeof(NotNullMock).Attribute(null));
        }

        [Test]
        public void RetrievePropertiesWithValidType_ReturnsProperties()
        {
            var prop = typeof(NotNullMock).Properties()
                .FirstOrDefault(p => p.Attribute(typeof(IValidator)) != null);

            Assert.IsNotNull(prop);
        }
    }

    [TestFixture]
    public class Retrieve
    {
        [Test]
        public void NullType_ThrowsArgumentNullException()
        {
            var member = typeof(RegexMock).TypeInfo().Properties[nameof(RegexMock.Salute)];
            Assert.Throws<ArgumentNullException>(() =>
                member.Attribute(null));
        }

        [Test]
        public void ValidParams_ReturnsAttributes()
        {
            var member = typeof(RegexMock).TypeInfo().Properties[nameof(RegexMock.Salute)];
            var attributes = member.PropertyAttributes.Where(c => c is IValidator);

            Assert.That(attributes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void PropertyWithNoMatchingAttributes_ReturnsZeroProperties()
        {
            var member = typeof(NotNullMock).TypeInfo().Properties[nameof(NotNullMock.Number)];
            var attributes = member.Attribute(typeof(IReflect));

            Assert.IsNull(attributes);
        }

        [Test]
        public void RetrievePropertiesWithValidType_ReturnsProperties()
        {
            var props = typeof(NotNullMock).Properties().Where(c => c.HasAttribute<NotNullAttribute>());

            Assert.That(props.Count, Is.EqualTo(1));
        }
    }
}
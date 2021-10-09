namespace Swan.Test.ObjectMapperTests
{
    using NUnit.Framework;
    using Swan.Mapping;
    using Swan.Test.Mocks;
    using System;
    using System.Collections.Generic;

    public abstract class ObjectMapperTest : TestFixtureBase
    {
        protected User SourceUser => new()
        {
            Email = "geovanni.perez@unosquare.com",
            Name = "Geo",
            Role = new Role { Name = "Admin" },
            StartDate = new DateTime(2000, 2, 5),
        };

        protected Dictionary<string, object?> SourceDict => new()
        {
            { "Name", "Armando" },
            { "Email", "armando.cifuentes@unosquare.com" },
            { "Role", "Intern tester" },
        };
    }

    [TestFixture]
    public class CreateMap : ObjectMapperTest
    {
        [Test]
        public void SimpleMap_ReturnsTrue()
        {
            var destination = ObjectMapper.Default
                .AddMap<User, UserDto>()
                .Apply(SourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(SourceUser.Name, destination.Name);
            Assert.AreEqual(SourceUser.Email, destination.Email);
            Assert.AreEqual(SourceUser.StartDate, destination.StartDate);
            Assert.IsNull(destination.Role);
        }

        [Test]
        public void MapDuplicated_ThrowsInvalidOperationException()
        {
            var mapper = new ObjectMapper();
            mapper.AddMap<User, UserDto>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                mapper.AddMap<User, UserDto>();
            });
        }

        [Test]
        public void MapWithoutSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectMapper.Default.Apply<UserDto>(null));
        }

        [Test]
        public void WithAutoresolveFalse_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new ObjectMapper().Apply<UserDto>(SourceUser, false));
        }
    }

    [TestFixture]
    public class PropertyMap : ObjectMapperTest
    {
        [Test]
        public void PropertiesAreEquals_ReturnsTrue()
        {
            var mapper = new ObjectMapper();
            var destination = mapper.AddMap<User, UserDto>()
                .Add(t => t.Role, s => s.Role.Name)
                .Apply(SourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(SourceUser.Name, destination.Name);
            Assert.AreEqual(SourceUser.Email, destination.Email);
            Assert.AreEqual(SourceUser.Role.Name, destination.Role);
        }

        [Test]
        public void PropertyDestinationWithInvalidPropertySource_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ObjectMapper().AddMap<User, UserDto>().Add(t => t, s => s.Role.Name));
        }

        [Test]
        public void PropertySourceWithInvalidPropertyDestination_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ObjectMapper().AddMap<User, UserDto>().Add(t => t.Role, s => s));
        }

        [Test]
        public void PropertiesTypeNotMatchInMaps_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => ObjectMapper.Default.AddMap<User, ErrorJson>());
        }
    }

    [TestFixture]
    public class RemoveMap : ObjectMapperTest
    {
        [Test]
        public void RemoveProperty_ReturnsTrue()
        {
            var mapper = new ObjectMapper();
            mapper.AddMap<User, UserDto>().Remove(t => t.Email);

            var destination = mapper.Apply<UserDto>(SourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(SourceUser.Name, destination.Name);
            Assert.IsNull(destination.Email);
            Assert.IsNull(destination.Role);
        }

        [Test]
        public void RemoveInvalidProperty_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ObjectMapper().AddMap<User, UserDto>().Remove(t => t));
        }

        [Test]
        public void PropertyDestinationInfoNull_ReturnsException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ObjectMapper().AddMap<User, UserDto>().Remove(x => x.Name == null));
        }
    }

    [TestFixture]
    public class AutoMap : ObjectMapperTest
    {
        [Test]
        public void AutoMapTest_ReturnsTrue()
        {
            var mapper = new ObjectMapper();
            var destination = mapper.Apply<UserDto>(SourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(SourceUser.Name, destination.Name);
            Assert.AreEqual(SourceUser.Email, destination.Email);
            Assert.IsNotNull(destination.Role);
        }
    }

    [TestFixture]
    public class Copy : ObjectMapperTest
    {
        [Test]
        public void SourceNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectMapper.Copy(NullObj, new UserDto()));
        }

        [Test]
        public void TargetNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectMapper.Copy(new UserDto(), null));
        }

        [Test]
        public void SourceDictionaryNull_ThrowsArgumentNullException()
        {
            var target = new UserDto();

            Assert.Throws<ArgumentNullException>(() => ObjectMapper.Copy(null, target));
        }

        [Test]
        public void TargetDictionaryNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectMapper.Copy(SourceDict, null));
        }

        [Test]
        public void SourceAndTargetNotNull_ReturnsCopy()
        {
            var target = new UserDto();

            var propertiesToCopy = new[] { "Name", "Email" };
            var ignoreProperties = new[] { "Role" };

            ObjectMapper.Copy(SourceDict, target, propertiesToCopy, ignoreProperties);

            Assert.AreEqual(SourceDict["Name"]?.ToString(), target.Name);
            Assert.AreEqual(SourceDict["Email"]?.ToString(), target.Email);
        }
    }
}
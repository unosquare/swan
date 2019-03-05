namespace Unosquare.Swan.Test.ObjectMapperTests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using Components;
    using Mocks;

    public abstract class ObjectMapperTest : TestFixtureBase
    {
        protected User SourceUser => new User
        {
            Email = "geovanni.perez@unosquare.com",
            Name = "Geo",
            Role = new Role {Name = "Admin"},
            StartDate = new DateTime(2000, 10, 13),
        };

        protected Dictionary<string, object> SourceDict => new Dictionary<string, object>
        {
            {"Name", "Armando"},
            {"Email", "armando.cifuentes@unosquare.com"},
            {"Role", "Intern tester"},
        };
    }

    [TestFixture]
    public class CreateMap : ObjectMapperTest
    {
        [Test]
        public void SimpleMap_ReturnsTrue()
        {
            Runtime.ObjectMapper.CreateMap<User, UserDto>();

            var destination = Runtime.ObjectMapper.Map<UserDto>(SourceUser);

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
            mapper.CreateMap<User, UserDto>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                mapper.CreateMap<User, UserDto>();
            });
        }

        [Test]
        public void MapWithoutSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Runtime.ObjectMapper.Map<UserDto>(null));
        }

        [Test]
        public void WithAutoresolveFalse_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new ObjectMapper().Map<UserDto>(SourceUser, false));
        }
    }

    [TestFixture]
    public class PropertyMap : ObjectMapperTest
    {
        [Test]
        public void PropertiesAreEquals_ReturnsTrue()
        {
            var mapper = new ObjectMapper();
            mapper.CreateMap<User, UserDto>().MapProperty(t => t.Role, s => s.Role.Name);

            var destination = mapper.Map<UserDto>(SourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(SourceUser.Name, destination.Name);
            Assert.AreEqual(SourceUser.Email, destination.Email);
            Assert.AreEqual(SourceUser.Role.Name, destination.Role);
        }

        [Test]
        public void PropertyDestinationWithInvalidPropertySource_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ObjectMapper().CreateMap<User, UserDto>().MapProperty(t => t, s => s.Role.Name));
        }

        [Test]
        public void PropertySourceWithInvalidPropertyDestination_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ObjectMapper().CreateMap<User, UserDto>().MapProperty(t => t.Role, s => s));
        }

        [Test]
        public void PropertiesTypeNotMatchInMaps_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Runtime.ObjectMapper.CreateMap<User, ErrorJson>());
        }
    }

    [TestFixture]
    public class RemoveMap : ObjectMapperTest
    {
        [Test]
        public void RemoveProperty_ReturnsTrue()
        {
            var mapper = new ObjectMapper();
            mapper.CreateMap<User, UserDto>().RemoveMapProperty(t => t.Email);

            var destination = mapper.Map<UserDto>(SourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(SourceUser.Name, destination.Name);
            Assert.IsNull(destination.Email);
            Assert.IsNull(destination.Role);
        }

        [Test]
        public void RemoveInvalidProperty_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ObjectMapper().CreateMap<User, UserDto>().RemoveMapProperty(t => t));
        }

        [Test]
        public void PropertyDestinationInfoNull_ReturnsException()
        {
            Assert.Throws<ArgumentException>(() => 
                new ObjectMapper().CreateMap<User, UserDto>().RemoveMapProperty(x => x.Name == null));
        }
    }

    [TestFixture]
    public class AutoMap : ObjectMapperTest
    {
        [Test]
        public void AutoMapTest_ReturnsTrue()
        {
            var mapper = new ObjectMapper();
            var destination = mapper.Map<UserDto>(SourceUser);

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

            var propertiesToCopy = new[] {"Name", "Email"};
            var ignoreProperties = new[] {"Role"};

            ObjectMapper.Copy(SourceDict, target, propertiesToCopy, ignoreProperties);

            Assert.AreEqual(SourceDict["Name"].ToString(), target.Name);
            Assert.AreEqual(SourceDict["Email"].ToString(), target.Email);
        }
    }
}
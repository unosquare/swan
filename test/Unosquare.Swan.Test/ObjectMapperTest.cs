using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ObjectMapperTest
    {
        private readonly User sourceUser = new User
        {
            Email = "geovanni.perez@unosquare.com",
            Name = "Geo",
            Role = new Role { Name = "Admin" }
        };

        [TestFixture]
        public class CreateMap : ObjectMapperTest
        {
            [Test]
            public void SimpleMap_ReturnsTrue()
            {
                Runtime.ObjectMapper.CreateMap<User, UserDto>();

                var destination = Runtime.ObjectMapper.Map<UserDto>(sourceUser);

                Assert.IsNotNull(destination);
                Assert.AreEqual(sourceUser.Name, destination.Name);
                Assert.AreEqual(sourceUser.Email, destination.Email);
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
            public void MapWithoutSouce_ThrowsArgumentNullException()
            {
                var mapper = new ObjectMapper();

                Assert.Throws<ArgumentNullException>(() =>
                {
                    mapper.Map<UserDto>(null);
                });
            }

            [Test]
            public void WithAutoresolveFalse_ThrowsInvalidOperationException()
            {
                var mapper = new ObjectMapper();

                Assert.Throws<InvalidOperationException>(() =>
                {
                    mapper.Map<UserDto>(sourceUser, false);
                });
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

                var destination = mapper.Map<UserDto>(sourceUser);

                Assert.IsNotNull(destination);
                Assert.AreEqual(sourceUser.Name, destination.Name);
                Assert.AreEqual(sourceUser.Email, destination.Email);
                Assert.AreEqual(sourceUser.Role.Name, destination.Role);
            }

            [Test]
            public void PropertyDestinationWithInvalidPropertySource_ThrowsException()
            {
                var mapper = new ObjectMapper();

                Assert.Throws<Exception>(() =>
                {
                    mapper.CreateMap<User, UserDto>().MapProperty(t => t, s => s.Role.Name);
                });
            }

            [Test]
            public void PropertySourceWithInvalidPropertyDestination_ThrowsException()
            {
                var mapper = new ObjectMapper();

                Assert.Throws<Exception>(() =>
                {
                    mapper.CreateMap<User, UserDto>().MapProperty(t => t.Role, s => s);
                });
            }

            [Test]
            public void PropertiesTypeNotMatchInMaps_ThrowsInvalidOperationException()
            {
                var mapper = new ObjectMapper();

                Assert.Throws<InvalidOperationException>(() =>
                {
                    mapper.CreateMap<User, AdminDto>();
                });
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

                var destination = mapper.Map<UserDto>(sourceUser);

                Assert.IsNotNull(destination);
                Assert.AreEqual(sourceUser.Name, destination.Name);
                Assert.IsNull(destination.Email);
                Assert.IsNull(destination.Role);
            }

            [Test]
            public void RemoveInvalidProperty_ThrowsException()
            {
                Assert.Throws<Exception>(() =>
                {
                    var mapper = new ObjectMapper();
                    mapper.CreateMap<User, UserDto>().RemoveMapProperty(t => t);
                });
            }

            [Test]
            public void PropertyDestionationInfoNull_ReturnsException()
            {
                var mapper = new ObjectMapper();
                var destination = mapper.Map<UserDto>(sourceUser);

                Assert.Throws<Exception>(() =>
                {
                    mapper.CreateMap<User, UserDto>().RemoveMapProperty(x => x.Name == null);
                });
            }
        }

        [TestFixture]
        public class AutoMap : ObjectMapperTest
        {
            [Test]
            public void AutoMapTest_ReturnsTrue()
            {
                var mapper = new ObjectMapper();
                var destination = mapper.Map<UserDto>(sourceUser);

                Assert.IsNotNull(destination);
                Assert.AreEqual(sourceUser.Name, destination.Name);
                Assert.AreEqual(sourceUser.Email, destination.Email);
                Assert.IsNotNull(destination.Role);
            }
        }

        [TestFixture]
        public class Copy : ObjectMapperTest
        {
            [Test]
            public void SourceNull_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    ObjectMapper.Copy((object)null, new UserDto(), null, null);
                });
            }

            [Test]
            public void TargetNull_ThrowsArgumentNullException()
            {
                var source = new UserDto();

                Assert.Throws<ArgumentNullException>(() =>
                {
                    ObjectMapper.Copy(source, null, null, null);
                });
            }

            [Test]
            public void SourceDictionaryNull_ThrowsArgumentNullException()
            {
                var target = new UserDto();

                Assert.Throws<ArgumentNullException>(() =>
                {
                    ObjectMapper.Copy(null, target, null, null);
                });
            }

            [Test]
            public void TargetDictionaryNull_ThrowsArgumentNullException()
            {
                var source = new Dictionary<string, object>
                {
                    { "Mario", 1 },
                    { "Arturo", 2 },
                    { "Fernanda", 3 }
                };

                Assert.Throws<ArgumentNullException>(() =>
                {
                    ObjectMapper.Copy(source, null, null, null);
                });
            }

            [Test]
            public void SourceAndTargetNotNull_ReturnsCopy()
            {
                var source = new Dictionary<string, object>
                {
                    { "Name", "Armando" },
                    { "Email", "armando.cifuentes@unosquare.com" },
                    { "Role", "Intern tester" }
                };

                var target = new UserDto();

                var propertiesToCopy = new string[] { "Name", "Email" };
                var ignoreProperties = new string[] { "Role" };

                var result = ObjectMapper.Copy(source, target, propertiesToCopy, ignoreProperties);

                Assert.AreEqual(source["Name"].ToString(), target.Name);
                Assert.AreEqual(source["Email"].ToString(), target.Email);
            }
        }
    }
}
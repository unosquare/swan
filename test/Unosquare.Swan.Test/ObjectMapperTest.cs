using NUnit.Framework;
using System;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ObjectMapperTest
    {
        private readonly User _sourceUser = new User
        {
            Email = "geovanni.perez@unosquare.com",
            Name = "Geo",
            Role = new Role { Name = "Admin" }
        };

        [Test]
        public void SimpleMapTest()
        {
            Runtime.ObjectMapper.CreateMap<User, UserDto>();

            var destination = Runtime.ObjectMapper.Map<UserDto>(_sourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(_sourceUser.Name, destination.Name);
            Assert.AreEqual(_sourceUser.Email, destination.Email);
            Assert.IsNull(destination.Role);
        }

        [Test]
        public void PropertyMapTest()
        {
            var mapper = new ObjectMapper();
            mapper.CreateMap<User, UserDto>().MapProperty(t => t.Role, s => s.Role.Name);

            var destination = mapper.Map<UserDto>(_sourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(_sourceUser.Name, destination.Name);
            Assert.AreEqual(_sourceUser.Email, destination.Email);
            Assert.AreEqual(_sourceUser.Role.Name, destination.Role);
        }

        [Test]
        public void RemoveyMapTest()
        {
            var mapper = new ObjectMapper();
            mapper.CreateMap<User, UserDto>().RemoveMapProperty(t => t.Email);

            var destination = mapper.Map<UserDto>(_sourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(_sourceUser.Name, destination.Name);
            Assert.IsNull(destination.Email);
            Assert.IsNull(destination.Role);
        }

        [Test]
        public void AutoMapTest()
        {
            var mapper = new ObjectMapper();
            var destination = mapper.Map<UserDto>(_sourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(_sourceUser.Name, destination.Name);
            Assert.AreEqual(_sourceUser.Email, destination.Email);
            Assert.IsNotNull(destination.Role);
        }

        [Test]
        public void PropertyMapDestinationExceptionTest()
        {
            Assert.Throws<Exception>(() => {
                var mapper = new ObjectMapper();
                mapper.CreateMap<User, UserDto>().MapProperty(t => t, s => s.Role.Name);
            });
        }

        [Test]
        public void PropertyMapSourceExceptionTest()
        {
            Assert.Throws<Exception>(() => {
                var mapper = new ObjectMapper();
                mapper.CreateMap<User, UserDto>().MapProperty(t => t.Role, s => s);
            });
        }

        [Test]
        public void RemoveyMapDestinationExceptionTest()
        {
            Assert.Throws<Exception>(() => {
                var mapper = new ObjectMapper();
                mapper.CreateMap<User, UserDto>().RemoveMapProperty(t => t);
            });
        }

        [Test]
        public void PropertyMapInvalidOperationExistingMapTest()
        {
            var mapper = new ObjectMapper();
            mapper.CreateMap<User, UserDto>();

            Assert.Throws<InvalidOperationException>(() => {
                mapper.CreateMap<User, UserDto>();
            });
        }

        [Test]
        public void PropertyMapInvalidOperationTypesNotMatchTest()
        {
            var mapper = new ObjectMapper();

            Assert.Throws<InvalidOperationException>(() => {                
                mapper.CreateMap<User, AdminDto>();
            });
        }

        [Test]
        public void MapArgumentExceptionTest()
        {
            var mapper = new ObjectMapper();

            Assert.Throws<ArgumentNullException>(() => {
                mapper.Map<UserDto>(null);
            });
        }

        [Test]
        public void MapInvalidOperationExceptionTest()
        {
            var mapper = new ObjectMapper();

            Assert.Throws<InvalidOperationException>(() => {
                mapper.Map<UserDto>(_sourceUser, false);
            });            
        }
        
        [Test]
        public void RemoveMapProperty_PropertyDestinationInfoNull_ReturnsInvalidDestinationExpression()
        {
            var mapper = new ObjectMapper();
            var destination = mapper.Map<UserDto>(_sourceUser);

            Assert.Throws<Exception>(() =>
            {
                mapper.CreateMap<User, UserDto>().RemoveMapProperty(x => x.Name == null);
            });
        }
    }
}
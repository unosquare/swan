namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Exceptions;
    using Networking.Ldap;

    public abstract class LdapTest
    {
        protected const string LdapServer = "ldap.forumsys.com";

        protected async Task<LdapConnection> GetDefaultConnection()
        {
            var cn = new LdapConnection();
            await cn.Connect(LdapServer, 389);
            await cn.Bind("uid=riemann,dc=example,dc=com", "password");

            return cn;
        }
    }

    [TestFixture]
    public class Bind : LdapTest
    {
        [Test]
        public async Task ValidCredentials_ReturnsTrue()
        {
            var cn = await GetDefaultConnection();
            Assert.IsNotNull(cn.AuthenticationDn);
            cn.Disconnect();
        }

        [Test]
        public void InvalidCredentials_ThrowsLdapException()
        {
            Assert.ThrowsAsync<LdapException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect(LdapServer, 389);
                await cn.Bind("uid=riemann,dc=example", "password");
                cn.Disconnect();
            });
        }

        [Test]
        public async Task NullPassword_ReturnsNullAuthenticationDnProperty()
        {
            var cn = new LdapConnection();
            await cn.Connect(LdapServer, 389);
            await cn.Bind("uid=riemann,dc=example,dc=com", null);
            Assert.IsNull(cn.AuthenticationDn);
            cn.Disconnect();

        }
    }

    [TestFixture]
    public class Connect : LdapTest
    {
        [Test]
        public async Task ValidHost_ReturnsTrue()
        {
            var cn = new LdapConnection();
            await cn.Connect(LdapServer, 389);
            Assert.IsTrue(cn.Connected);
            cn.Disconnect();
        }

        [Test]
        public void InvalidHost_ThrowsSocketException()
        {
            Assert.CatchAsync<SocketException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect("ldap.forumsys", 389);
                await cn.Bind("uid=riemann,dc=example,dc=com", "password");
                cn.Disconnect();
            });
        }

        [Test]
        public void InvalidPort_ThrowsSocketException()
        {
            Assert.CatchAsync<SocketException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect("ldap.forumsys", 388);
                await cn.Bind("uid=riemann,dc=example,dc=com", "password");
                cn.Disconnect();
            });
        }

        [Test]
        public async Task Connected_ResetConnection()
        {
            var cn = await GetDefaultConnection();
            Assert.IsTrue(cn.Connected);
            cn.Disconnect();
            Assert.IsFalse(cn.Connected);
        }
    }

    [TestFixture]
    public class Search : LdapTest
    {
        [Test]
        public async Task MultipleSearchResults()
        {
            var cn = await GetDefaultConnection();
            var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.ScopeSub);

            if (lsc.HasMore())
            {
                var entry = lsc.Next();
                var ldapAttributes = entry.GetAttributeSet();

                Assert.IsNotNull(ldapAttributes.GetAttribute("uniqueMember")?.StringValue);
            }

            Assert.AreNotEqual(lsc.Count, 0);
            Assert.IsTrue(lsc.HasMore());
            cn.Disconnect();
        }

        [Test]
        public async Task SingleSearchResult()
        {
            var cn = await GetDefaultConnection();
            var lsc = await cn.Search(
                "ou=scientists,dc=example,dc=com", 
                LdapConnection.ScopeSub,
                "(uniqueMember=uid=tesla,dc=example,dc=com)");

            if (lsc.HasMore())
            {
                var entry = lsc.Next();
                var ldapAttributes = entry.GetAttributeSet();
                
                Assert.IsNotNull(ldapAttributes.GetAttribute("uniqueMember")?.StringValue);
            }

            Assert.AreNotEqual(lsc.Count, 0);
            cn.Disconnect();
        }

        [Test]
        public void UsingInvalidDN_ThrowsLdapException()
        {
            Assert.ThrowsAsync<LdapException>(async () =>
            {
                var cn = await GetDefaultConnection();
                await cn.Search("ou=scientists,dc=com", LdapConnection.ScopeSub);
                cn.Disconnect();
            });
        }

        [Test]
        public void SearchForMore_ThrowsLdapException()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var cn = await GetDefaultConnection();
                var lsc = await cn.Search(
                    "ou=scientists,dc=example,dc=com", 
                    LdapConnection.ScopeSub,
                    "(uniqueMember=uid=tesla,dc=example,dc=com)");

                while (lsc.HasMore())
                {
                    lsc.Next();
                }

                lsc.Next();

                cn.Disconnect();
            });
        }

        public class ModifyTest : LdapTest
        {
            [Test]
            public void ChangeUserProperty()
            {
                var ex = Assert.ThrowsAsync<LdapException>(async () =>
                {
                    var cn = await GetDefaultConnection();
                    await cn.Modify(
                        "uid=euclid,dc=example,dc=com",
                        new[] { new LdapModification(LdapModificationOp.Replace, "mail", "new@ldap.forumsys.com")});

                    cn.Disconnect();
                });

                Assert.AreEqual(ex.ResultCode, LdapStatusCode.InsufficientAccessRights);
            }
        }

        public class ReadTest : LdapTest
        {
            [Test]
            public async Task ReadUserProperties()
            {
                if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
                    Assert.Inconclusive("Can not test in AppVeyor");

                var cn = new LdapConnection();
                await cn.Connect("127.0.0.1", 1089);
                await cn.Bind("cn=root", "secret");
                var properties = await cn.Read("cn=Simio, o=joyent");
                var mail = properties.GetAttribute("email");
                Assert.AreEqual(mail.StringValue, "gperez@unosquare.com");
                cn.Disconnect();
            }
        }
    }
}
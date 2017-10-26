using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Networking.Ldap;
using System;
using System.Net.Sockets;

namespace Unosquare.Swan.Test
{
    public class LdapTest
    {
        private const string ldapServer = "ldap.forumsys.com";

        [TestFixture]
        public class Bind : LdapTest
        {
            [Test]
            public async Task ValidCredentials_ReturnsTrue()
            {
                var cn = new LdapConnection();
                await cn.Connect(ldapServer, 389);
                await cn.Bind("uid=riemann,dc=example,dc=com", "password");
                Assert.IsNotNull(cn.AuthenticationDn);
                cn.Disconnect();
            }

            [Test]
            public void InvalidCredentials_ThrowsLdapException()
            {
                Assert.ThrowsAsync<LdapException>(async () =>
                {
                    var cn = new LdapConnection();
                    await cn.Connect(ldapServer, 389);
                    await cn.Bind("uid=riemann,dc=example", "password");
                    cn.Disconnect();
                });
            }
        }

        [TestFixture]
        public class Connect : LdapTest
        {
            [Test]
            public async Task ValidHost_ReturnsTrue()
            {
                var cn = new LdapConnection();
                await cn.Connect(ldapServer, 389);
                Assert.IsTrue(cn.Connected);
                cn.Disconnect();
            }

            [Test]
            public void InvalidHost_ThrowsSocketException()
            {
                Assert.ThrowsAsync<SocketException>(async () =>
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
                Assert.ThrowsAsync<SocketException>(async () =>
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
                var cn = new LdapConnection();
                await cn.Connect(ldapServer, 389);
                await cn.Bind("uid=riemann,dc=example,dc=com", "password");
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
                var cn = new LdapConnection();

                await cn.Connect(ldapServer, 389);
                await cn.Bind("uid=riemann,dc=example,dc=com", "password");
                var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.ScopeSub);

                if (lsc.HasMore())
                {
                    var entry = lsc.Next();
                    var ldapAttributes = entry.GetAttributeSet();
                    var obj = ldapAttributes.GetAttribute("uniqueMember")?.StringValue;
                    Assert.IsNotNull(obj);
                }

                Assert.AreNotEqual(lsc.Count, 0);
                Assert.IsTrue(lsc.HasMore());
                cn.Disconnect();
            }

            [Test]
            public async Task SingleSearchResult()
            {
                var cn = new LdapConnection();
                await cn.Connect(ldapServer, 389);
                await cn.Bind("uid=riemann,dc=example,dc=com", "password");
                var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.ScopeSub, "(uniqueMember=uid=tesla,dc=example,dc=com)");

                if (lsc.HasMore())
                {
                    var entry = lsc.Next();
                    var ldapAttributes = entry.GetAttributeSet();
                    var obj = ldapAttributes.GetAttribute("uniqueMember")?.StringValue;
                    Assert.IsNotNull(obj);
                }

                Assert.AreNotEqual(lsc.Count, 0);
                cn.Disconnect();
            }

            [Test]
            public void UsingInvalidDN_ThrowsLdapException()
            {
                Assert.ThrowsAsync<LdapException>(async () =>
                {
                    var cn = new LdapConnection();
                    await cn.Connect(ldapServer, 389);
                    await cn.Bind("uid=riemann,dc=example,dc=com", "password");
                    var lsc = await cn.Search("ou=scientists,dc=com", LdapConnection.ScopeSub);
                    cn.Disconnect();
                });
            }
        }
    }
}
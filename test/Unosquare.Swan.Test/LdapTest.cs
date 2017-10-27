using NUnit.Framework;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unosquare.Swan.Exceptions;
using Unosquare.Swan.Networking.Ldap;

namespace Unosquare.Swan.Test
{
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
            var cn = await GetDefaultConnection();
            var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.ScopeSub,
                "(uniqueMember=uid=tesla,dc=example,dc=com)");

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
                var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.ScopeSub,
                    "(uniqueMember=uid=tesla,dc=example,dc=com)");

                while (lsc.HasMore())
                {
                    lsc.Next();
                }

                lsc.Next();
            });
        }
    }
}
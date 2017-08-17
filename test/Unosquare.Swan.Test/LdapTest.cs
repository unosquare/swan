using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Networking.Ldap;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class LdapTest
    {
        [Test]
        public async Task ConnectTest()
        {
            var cn = new LdapConnection();

            await cn.Connect("ldap.forumsys.com", 389);
            Assert.IsTrue(cn.Connected);
        }

        [Test]
        public async Task BindTest()
        {
            var cn = new LdapConnection();

            await cn.Connect("ldap.forumsys.com", 389);
            await cn.Bind("uid=riemann,dc=example,dc=com", "password");
        }

        [Test]
        public async Task SearchTest()
        {
            var cn = new LdapConnection();

            await cn.Connect("ldap.forumsys.com", 389);
            await cn.Bind("uid=riemann,dc=example,dc=com", "password");
            var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.SCOPE_SUB);

            if (lsc.hasMore())
            {
                var entry = lsc.next();
                var ldapAttributes = entry.GetAttributeSet();
                var obj = ldapAttributes.GetAttribute("uniqueMember")?.StringValue ?? null;
                obj.Info(nameof(LdapTest));
                Assert.IsTrue(obj != null);
            }

            lsc.Count.ToString().Info(nameof(LdapTest));
            Assert.AreNotEqual(lsc.Count, 0);
            Assert.IsTrue(lsc.hasMore());
        }

        [Test]
        public async Task Disconnect()
        {
            var cn = new LdapConnection();
            await cn.Connect("ldap.forumsys.com", 389);
            await cn.Bind("uid=riemann,dc=example,dc=com", "password");
            Assert.IsTrue(cn.Connected);
            "Disconnecting...".Info(nameof(LdapTest));
            cn.Disconnect();
            Assert.IsFalse(cn.Connected);
        }
    }
}

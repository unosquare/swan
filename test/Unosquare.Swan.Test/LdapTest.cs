using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Unosquare.Swan.Networking.Ldap;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class LdapTest
    {
        [Test]
        public async void ConnectTest()
        {
            var cn = new LdapConnection();

            await cn.Connect("ldap.forumsys.com", 389);
            Assert.IsTrue(cn.Connected);;
        }

        [Test]
        public async void BindTest()
        {
            var cn = new LdapConnection();

            await cn.Connect("ldap.forumsys.com", 389);
            await cn.Bind("uid=riemann,dc=example,dc=com", "password");
        }

        [Test]
        public async void SearchTest()
        {
            var cn = new LdapConnection();

            await cn.Connect("ldap.forumsys.com", 389);
            await cn.Bind("uid=riemann,dc=example,dc=com", "password");
            var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.SCOPE_SUB);

            if (lsc.hasMore())
            {
                var entry = lsc.next();
                var ldapAttributes = entry.getAttributeSet();
                var obj = ldapAttributes.getAttribute("uniqueMember")?.StringValue ?? null;
                obj.Info(nameof(LdapTest));
                Assert.IsTrue(obj != null);
            }

            lsc.Count.ToString().Info(nameof(LdapTest));
            Assert.AreNotEqual(lsc.Count, 0);
            Assert.IsTrue(lsc.hasMore());
        }
    }
}

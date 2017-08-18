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

            if (lsc.HasMore())
            {
                var entry = lsc.Next();
                var ldapAttributes = entry.GetAttributeSet();
                var obj = ldapAttributes.GetAttribute("uniqueMember")?.StringValue;
                Assert.IsTrue(obj != null);
            }

            Assert.AreNotEqual(lsc.Count, 0);
            Assert.IsTrue(lsc.HasMore());
        }

        [Test]
        public async Task Disconnect()
        {
            var cn = new LdapConnection();
            await cn.Connect("ldap.forumsys.com", 389);
            await cn.Bind("uid=riemann,dc=example,dc=com", "password");
            Assert.IsTrue(cn.Connected);

            cn.Disconnect();
            Assert.IsFalse(cn.Connected);
        }
    }
}
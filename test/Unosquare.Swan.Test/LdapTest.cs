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
        public LdapConnection cn;
        protected const string LdapServer = "ldap.forumsys.com";
        protected const string DefaultDn = "uid=riemann,dc=example,dc=com";        

        [SetUp]
        public async Task Setup()
        {
            cn = new LdapConnection();
            await cn.Connect(LdapServer, 389);
            await cn.Bind(DefaultDn, "password");
        }

        [TearDown]
        public void GlobalTeardown()
        {
            cn.Dispose();
        }
    }

    [TestFixture]
    public class Bind : LdapTest
    {
        [Test]
        public void ValidCredentials_ReturnsTrue()
        {
            Assert.IsNotNull(cn.AuthenticationDn);
        }

        [Test]
        public void InvalidCredentials_ThrowsLdapException()
        {
            Assert.ThrowsAsync<LdapException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect(LdapServer, 389);
                await cn.Bind("uid=riemann,dc=example", "password");
                cn.Dispose();
            });
        }

        [Test]
        public async Task NullPassword_ReturnsNullAuthenticationDnProperty()
        {
            var cn = new LdapConnection();
            await cn.Connect(LdapServer, 389);
            await cn.Bind(DefaultDn, null);
            Assert.IsNull(cn.AuthenticationDn);
            cn.Dispose();
        }
    }

    [TestFixture]
    public class Connect : LdapTest
    {
        [Test]
        public async Task ValidHost_ReturnsTrue()
        {
            await cn.Connect(LdapServer, 389);
            Assert.IsTrue(cn.Connected);
        }

        [Test]
        public void InvalidHost_ThrowsSocketException()
        {
            Assert.CatchAsync<SocketException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect("ldap.forumsys", 389);
                await cn.Bind(DefaultDn, "password");
                cn.Dispose();
            });
        }

        [Test]
        public void InvalidPort_ThrowsSocketException()
        {
            Assert.CatchAsync<SocketException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect("ldap.forumsys", 388);
                await cn.Bind(DefaultDn, "password");
                cn.Dispose();
            });
        }

        [Test]
        public void Connected_ResetConnection()
        {
            Assert.IsTrue(cn.Connected);
            cn.Dispose();
            Assert.IsFalse(cn.Connected);
        }

        [Test]
        public void Default_ProtocolVersion()
        {
            Assert.AreEqual(3, cn.ProtocolVersion, "The default protocol version is 3");
        }

        [Test]
        public void Default_AuthenticationMethod()
        {
            Assert.AreEqual("simple", cn.AuthenticationMethod, "The default Authentication Method is simple");
        }
    }

    [TestFixture]
    public class Controls : LdapTest
    {
        [Test]
        public void Controls_Null()
        {
            Assert.IsNull(cn.ResponseControls);
        }

        [Test]
        public void Controls_Something()
        {
            // TODO: LDAP server with controls
        }
    }

    [TestFixture]
    public class Modify : LdapTest
    {
        [Test]
        public void Modify_DNNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var mods = new LdapModification(LdapModificationOp.Replace, new LdapAttribute("ui"));
                await cn.Modify(null, new[] { mods });
            });
        }
    }

    [TestFixture]
    public class Read : LdapTest
    {
        [Test]
        public async Task Read_DN()
        {
            var entry = await cn.Read(DefaultDn);

            Assert.AreEqual(DefaultDn, entry.DN);
        }

        [Test]
        public void Read_LdapException()
        {
            var dn = "ou=scientists,dc=example,dc=com";

            Assert.ThrowsAsync<LdapException>(async () =>
            {
                await cn.Read(dn);
            });
        }
    }

    [TestFixture]
    public class Search : LdapTest
    {
        [Test]
        public async Task MultipleSearchResults()
        {
            var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.ScopeSub);

            if (lsc.HasMore())
            {
                var entry = lsc.Next();
                var ldapAttributes = entry.GetAttributeSet();

                Assert.IsNotNull(ldapAttributes.GetAttribute("uniqueMember")?.StringValue);
            }

            Assert.AreNotEqual(lsc.Count, 0);
            Assert.IsTrue(lsc.HasMore());
        }

        [Test]
        public async Task SingleSearchResult()
        {
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
        }

        [Test]
        public void UsingInvalidDN_ThrowsLdapException()
        {
            Assert.ThrowsAsync<LdapException>(async () =>
            {
                await cn.Search("ou=scientists,dc=com", LdapConnection.ScopeSub);
            });
        }

        [Test]
        public void SearchForMore_ThrowsLdapException()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var lsc = await cn.Search(
                    "ou=scientists,dc=example,dc=com", 
                    LdapConnection.ScopeSub,
                    "(uniqueMember=uid=tesla,dc=example,dc=com)");

                while (lsc.HasMore())
                {
                    lsc.Next();
                }

                lsc.Next();
            });
        }

        public class ModifyTest : LdapTest
        {
            [Test]
            public void ChangeUserProperty()
            {
                var ex = Assert.ThrowsAsync<LdapException>(async () =>
                {
                    await cn.Modify(
                        "uid=euclid,dc=example,dc=com",
                        new[] { new LdapModification(LdapModificationOp.Replace, "mail", "new@ldap.forumsys.com")});

                    cn.Dispose();
                });

                Assert.AreEqual(ex.ResultCode, LdapStatusCode.InsufficientAccessRights);
            }
        }

        public class ReadTest : LdapTest
        {
            [Test]
            public async Task ReadUserProperties()
            {
                if (Runtime.OS == Swan.OperatingSystem.Osx)
                    Assert.Ignore("OSX can't load LDAP.js");
                
                var properties = await cn.Read("uid=einstein,dc=example,dc=com");
                var mail = properties.GetAttribute("MAIL");
                Assert.AreEqual(mail.StringValue, "einstein@ldap.forumsys.com");
                cn.Dispose();
            }
        }
    }
}
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
        public LdapConnection Cn;
        protected const string LdapServer = "ldap.forumsys.com";
        protected const string DefaultDn = "uid=riemann,dc=example,dc=com";

        [SetUp]
        public async Task Setup()
        {
            Cn = new LdapConnection();
            await Cn.Connect(LdapServer, 389);
            await Cn.Bind(DefaultDn, "password");
        }

        [TearDown]
        public void GlobalTeardown()
        {
            Cn.Dispose();
        }
    }

    [TestFixture]
    public class Bind : LdapTest
    {
        [Test]
        public void ValidCredentials_ReturnsTrue()
        {
            Assert.IsNotNull(Cn.AuthenticationDn);
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
            await Cn.Connect(LdapServer, 389);
            Assert.IsTrue(Cn.Connected);
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
            Assert.IsTrue(Cn.Connected);
            Cn.Dispose();
            Assert.IsFalse(Cn.Connected);
        }

        [Test]
        public void Default_ProtocolVersion()
        {
            Assert.AreEqual(3, Cn.ProtocolVersion, "The default protocol version is 3");
        }

        [Test]
        public void Default_AuthenticationMethod()
        {
            Assert.AreEqual("simple", Cn.AuthenticationMethod, "The default Authentication Method is simple");
        }
    }

    [TestFixture]
    public class Controls : LdapTest
    {
        [Test]
        public void Controls_Null()
        {
            Assert.IsNull(Cn.ResponseControls);
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
        public void ChangeUserProperty_LdapException()
        {
            var ex = Assert.CatchAsync(async () =>
            {
                await Cn.Modify(
                    "uid=euclid,dc=example,dc=com",
                    new[] {new LdapModification(LdapModificationOp.Replace, "mail", "new@ldap.forumsys.com")});

                Cn.Disconnect();
            });

            if (ex is LdapException ldapEx)
                Assert.AreEqual(ldapEx.ResultCode, LdapStatusCode.InsufficientAccessRights);
            else
                Assert.IsNotNull(ex);
        }

        [Test]
        public void Modify_DNNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var mods = new LdapModification(LdapModificationOp.Replace, new LdapAttribute("ui"));
                await Cn.Modify(null, new[] {mods});
            });
        }
    }

    [TestFixture]
    public class Read : LdapTest
    {
        [Test]
        public async Task ReadUserProperties_MailAttributeEqualsEinsteinMail()
        {
            if (Runtime.OS == Swan.OperatingSystem.Osx)
                Assert.Ignore("OSX can't load LDAP.js");

            var properties = await Cn.Read("uid=einstein,dc=example,dc=com");
            var mail = properties.GetAttribute("MAIL");
            Assert.AreEqual(mail.StringValue, "einstein@ldap.forumsys.com");
            Cn.Dispose();
        }

        [Test]
        public async Task Read_DN()
        {
            var entry = await Cn.Read(DefaultDn);

            Assert.AreEqual(DefaultDn, entry.DN);
        }

        [Test]
        public void Read_LdapException()
        {
            Assert.ThrowsAsync<LdapException>(async () =>
            {
                await Cn.Read("ou=scientists,dc=example,dc=com");
            });
        }
    }

    [TestFixture]
    public class Search : LdapTest
    {
        [Test]
        public async Task MultipleSearchResults()
        {
            var lsc = await Cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.ScopeSub);

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
            var lsc = await Cn.Search(
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
                await Cn.Search("ou=scientists,dc=com", LdapConnection.ScopeSub);
            });
        }

        [Test]
        public void SearchForMore_ThrowsLdapException()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var lsc = await Cn.Search(
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
    }
}
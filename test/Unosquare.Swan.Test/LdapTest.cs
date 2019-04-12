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
        protected const string LdapServer = "localhost";
        protected const string DefaultDn = "cn=root";
        protected const string DefaultOrgDn = "dn=sample, o=unosquare";
        protected const string DefaultPassword = "secret";
        protected const int DefaultPort = 1089;
        protected const string DefaultUserDn = "cn=Simio, dn=sample, o=unosquare";

        public LdapConnection Connection { get; private set; }

        [SetUp]
        public async Task Setup()
        {
            Connection = new LdapConnection();
            await Connection.Connect(LdapServer, DefaultPort);
            await Connection.Bind(DefaultDn, DefaultPassword);
        }

        [TearDown]
        public void GlobalTeardown() => Connection?.Dispose();
    }

    [TestFixture]
    public class Bind : LdapTest
    {
        [Test]
        public void ValidCredentials_ReturnsTrue()
        {
            Assert.IsNotNull(Connection.AuthenticationDn);
        }

        [Test]
        public void InvalidCredentials_ThrowsLdapException()
        {
            Assert.ThrowsAsync<LdapException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect(LdapServer, DefaultPort);
                await cn.Bind("cn=invalid", DefaultPassword);
                cn.Dispose();
            });
        }

        [Test]
        public async Task NullPassword_ReturnsNullAuthenticationDnProperty()
        {
            var cn = new LdapConnection();
            await cn.Connect(LdapServer, DefaultPort);
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
            await Connection.Connect(LdapServer, DefaultPort);
            Assert.IsTrue(Connection.Connected);
        }

        [Test]
        public void InvalidHost_ThrowsSocketException()
        {
            Assert.CatchAsync<SocketException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect("invalid.local", DefaultPort);
                await cn.Bind(DefaultDn, DefaultPassword);
                cn.Dispose();
            });
        }

        [Test]
        public void InvalidPort_ThrowsSocketException()
        {
            Assert.CatchAsync<SocketException>(async () =>
            {
                var cn = new LdapConnection();
                await cn.Connect(LdapServer, 388);
                await cn.Bind(DefaultDn, DefaultPassword);
                cn.Dispose();
            });
        }

        [Test]
        public void Connected_ResetConnection()
        {
            Assert.IsTrue(Connection.Connected);
            Connection.Dispose();
            Assert.IsFalse(Connection.Connected);
        }

        [Test]
        public void Default_ProtocolVersion()
        {
            Assert.AreEqual(3, Connection.ProtocolVersion, "The default protocol version is 3");
        }

        [Test]
        public void Default_AuthenticationMethod()
        {
            Assert.AreEqual("simple", Connection.AuthenticationMethod, "The default Authentication Method is simple");
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
                await Connection.Modify(
                    "cn=Invalid, o=unosquare",
                    new[] {new LdapModification(LdapModificationOp.Replace, "email", "new@unosquare.com")});

                Connection.Disconnect();
            });

            if (ex is LdapException ldapEx)
                Assert.AreEqual(LdapStatusCode.NoSuchObject, ldapEx.ResultCode);
            else
                Assert.IsNotNull(ex);
        }

        [Test]
        public void Modify_DNNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var mods = new LdapModification(LdapModificationOp.Replace, new LdapAttribute("ui"));
                await Connection.Modify(null, new[] {mods});
            });
        }
    }

    [TestFixture]
    public class Read : LdapTest
    {
        [Test]
        public async Task WithDefaultUser_MailShouldMatch()
        {
            var properties = await Connection.Read(DefaultUserDn);
            var mail = properties.GetAttribute("email");
            Assert.AreEqual(mail.StringValue, "gperez@unosquare.com");
        }
        
        [Test]
        public void Read_LdapException()
        {
            Assert.ThrowsAsync<LdapException>(async () =>
            {
                await Connection.Read("ou=scientists,dc=example,dc=com");
            });
        }
    }

    [TestFixture]
    public class Search : LdapTest
    {
        [Test]
        public async Task MultipleSearchResults()
        {
            var lsc = await Connection.Search(DefaultOrgDn, LdapScope.ScopeSub);

            if (lsc.HasMore())
            {
                var entry = lsc.Next();
                var ldapAttributes = entry.GetAttributeSet();

                Assert.IsNotNull(ldapAttributes["email"]?.StringValue);
            }

            Assert.AreNotEqual(lsc.Count, 0);
            Assert.IsTrue(lsc.HasMore());
        }

        [Test]
        public async Task SingleSearchResult()
        {
            var lsc = await Connection.Search(
                DefaultOrgDn,
                LdapScope.ScopeSub,
                "(email=gperez@unosquare.com)");

            if (lsc.HasMore())
            {
                var entry = lsc.Next();
                var ldapAttributes = entry.GetAttributeSet();

                Assert.AreEqual("gperez@unosquare.com", ldapAttributes["email"]?.StringValue);
            }

            Assert.IsFalse(lsc.HasMore());
        }

        [Test]
        public void UsingInvalidDN_ThrowsLdapException()
        {
            Assert.ThrowsAsync<LdapException>(async () =>
            {
                await Connection.Search("ou=scientists,dc=com", LdapScope.ScopeSub);
            });
        }

        [Test]
        public void SearchForMore_ThrowsLdapException()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var lsc = await Connection.Search(
                    DefaultOrgDn,
                    LdapScope.ScopeSub,
                    $"(uniqueMember={DefaultUserDn})");

                while (lsc.HasMore())
                {
                    lsc.Next();
                }

                lsc.Next();
            });
        }
    }
}
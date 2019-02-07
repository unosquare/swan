namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;
    using Exceptions;

    /// <summary>
    /// Encapsulates optional additional parameters or constraints to be applied to
    /// an Ldap operation.
    /// When included with LdapConstraints or LdapSearchConstraints
    /// on an LdapConnection or with a specific operation request, it is
    /// sent to the server along with operation requests.
    /// </summary>
    public class LdapControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapControl"/> class.
        /// Constructs a new LdapControl object using the specified values.
        /// </summary>
        /// <param name="oid">The OID of the control, as a dotted string.</param>
        /// <param name="critical">True if the Ldap operation should be discarded if
        /// the control is not supported. False if
        /// the operation can be processed without the control.</param>
        /// <param name="values">The control-specific data.</param>
        /// <exception cref="ArgumentException">An OID must be specified.</exception>
        public LdapControl(string oid, bool critical, sbyte[] values)
        {
            if (oid == null)
            {
                throw new ArgumentException("An OID must be specified");
            }

            Asn1Object = new RfcControl(
                oid,
                new Asn1Boolean(critical),
                values == null ? null : new Asn1OctetString(values));
        }

        /// <summary>
        /// Returns the identifier of the control.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id => Asn1Object.ControlType.StringValue();

        /// <summary>
        /// Returns whether the control is critical for the operation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if critical; otherwise, <c>false</c>.
        /// </value>
        public bool Critical => Asn1Object.Criticality.BooleanValue();

        internal static RespControlVector RegisteredControls { get; } = new RespControlVector(5);

        internal RfcControl Asn1Object { get; }

        /// <summary>
        /// Registers a class to be instantiated on receipt of a control with the
        /// given OID.
        /// Any previous registration for the OID is overridden. The
        /// controlClass must be an extension of LdapControl.
        /// </summary>
        /// <param name="oid">The object identifier of the control.</param>
        /// <param name="controlClass">A class which can instantiate an LdapControl.</param>
        public static void Register(string oid, Type controlClass)
            => RegisteredControls.RegisterResponseControl(oid, controlClass);

        /// <summary>
        ///     Returns the control-specific data of the object.
        /// </summary>
        /// <returns>
        ///     The control-specific data of the object as a byte array,
        ///     or null if the control has no data.
        /// </returns>
        public sbyte[] GetValue() => Asn1Object.ControlValue?.ByteValue();

        internal void SetValue(sbyte[] controlValue)
        {
            Asn1Object.ControlValue = new Asn1OctetString(controlValue);
        }
    }

    /// <summary>
    /// Represents a simple bind request.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapMessage" />
    public class LdapBindRequest : LdapMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapBindRequest"/> class.
        /// Constructs a simple bind request.
        /// </summary>
        /// <param name="version">The Ldap protocol version, use Ldap_V3.
        /// Ldap_V2 is not supported.</param>
        /// <param name="dn">If non-null and non-empty, specifies that the
        /// connection and all operations through it should
        /// be authenticated with dn as the distinguished
        /// name.</param>
        /// <param name="password">If non-null and non-empty, specifies that the
        /// connection and all operations through it should
        /// be authenticated with dn as the distinguished
        /// name and passwd as password.</param>
        public LdapBindRequest(int version, string dn, sbyte[] password)
            : base(LdapOperation.BindRequest, new RfcBindRequest(version, dn, password))
        {
        }

        /// <summary>
        /// Retrieves the Authentication DN for a bind request.
        /// </summary>
        /// <value>
        /// The authentication dn.
        /// </value>
        public string AuthenticationDN => Asn1Object.RequestDn;

        /// <inheritdoc />
        public override string ToString() => Asn1Object.ToString();
    }

    /// <summary>
    /// Encapsulates a continuation reference from an asynchronous search operation.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapMessage" />
    internal class LdapSearchResultReference : LdapMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchResultReference"/> class.
        /// Constructs an LdapSearchResultReference object.
        /// </summary>
        /// <param name="message">The LdapMessage with a search reference.</param>
        internal LdapSearchResultReference(RfcLdapMessage message)
            : base(message)
        {
        }

        /// <summary>
        /// Returns any URLs in the object.
        /// </summary>
        /// <value>
        /// The referrals.
        /// </value>
        public string[] Referrals
        {
            get
            {
                var references = ((RfcSearchResultReference)Message.Response).ToArray();
                var srefs = new string[references.Length];
                for (var i = 0; i < references.Length; i++)
                {
                    srefs[i] = ((Asn1OctetString)references[i]).StringValue();
                }

                return srefs;
            }
        }
    }

    internal class LdapResponse : LdapMessage
    {
        internal LdapResponse(RfcLdapMessage message)
            : base(message)
        {
        }

        public string ErrorMessage => ((IRfcResponse)Message.Response).GetErrorMessage().StringValue();

        public string MatchedDN => ((IRfcResponse)Message.Response).GetMatchedDN().StringValue();

        public LdapStatusCode ResultCode => Message.Response is RfcSearchResultEntry ||
                                            (IRfcResponse)Message.Response is RfcIntermediateResponse
            ? LdapStatusCode.Success
            : (LdapStatusCode)((IRfcResponse)Message.Response).GetResultCode().IntValue();

        internal LdapException Exception { get; set; }

        internal void ChkResultCode()
        {
            if (Exception != null)
            {
                throw Exception;
            }

            switch (ResultCode)
            {
                case LdapStatusCode.Success:
                case LdapStatusCode.CompareTrue:
                case LdapStatusCode.CompareFalse:
                    break;
                case LdapStatusCode.Referral:
                    throw new LdapException(
                        "Automatic referral following not enabled",
                        LdapStatusCode.Referral,
                        ErrorMessage);
                default:
                    throw new LdapException(ResultCode.ToString().Humanize(), ResultCode, ErrorMessage, MatchedDN);
            }
        }
    }

    /// <summary>
    /// The RespControlVector class implements extends the
    /// existing Vector class so that it can be used to maintain a
    /// list of currently registered control responses.
    /// </summary>
    internal class RespControlVector : List<RespControlVector.RegisteredControl>
    {
        private readonly object _syncLock = new object();

        public RespControlVector(int cap)
            : base(cap)
        {
        }

        public void RegisterResponseControl(string oid, Type controlClass)
        {
            lock (_syncLock)
            {
                Add(new RegisteredControl(this, oid, controlClass));
            }
        }

        /// <summary>
        /// Inner class defined to create a temporary object to encapsulate
        /// all registration information about a response control.
        /// </summary>
        internal class RegisteredControl
        {
            public RegisteredControl(RespControlVector enclosingInstance, string oid, Type controlClass)
            {
                EnclosingInstance = enclosingInstance;
                MyOid = oid;
                MyClass = controlClass;
            }

            internal Type MyClass { get; }

            internal string MyOid { get; }

            private RespControlVector EnclosingInstance { get; }
        }
    }

    /// <summary>
    /// Represents and Ldap Bind Request.
    /// <pre>
    /// BindRequest ::= [APPLICATION 0] SEQUENCE {
    /// version                 INTEGER (1 .. 127),
    /// name                    LdapDN,
    /// authentication          AuthenticationChoice }
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    /// <seealso cref="IRfcRequest" />
    internal sealed class RfcBindRequest
        : Asn1Sequence, IRfcRequest
    {
        private readonly sbyte[] _password;
        private static readonly Asn1Identifier Id = new Asn1Identifier(LdapOperation.BindRequest);

        public RfcBindRequest(int version, string name, sbyte[] password)
            : base(3)
        {
            _password = password;
            Add(new Asn1Integer(version));
            Add(name);
            Add(new RfcAuthenticationChoice(password));
        }

        public Asn1Integer Version
        {
            get => (Asn1Integer)Get(0);
            set => Set(0, value);
        }

        public Asn1OctetString Name
        {
            get => (Asn1OctetString)Get(1);
            set => Set(1, value);
        }

        public RfcAuthenticationChoice AuthenticationChoice
        {
            get => (RfcAuthenticationChoice)Get(2);
            set => Set(2, value);
        }

        public override Asn1Identifier GetIdentifier() => Id;

        public string GetRequestDN() => ((Asn1OctetString)Get(1)).StringValue();
    }
}
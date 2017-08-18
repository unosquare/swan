#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents an Ldap Control.
    /// <pre>
    /// Control ::= SEQUENCE {
    /// controlType             LdapOID,
    /// criticality             BOOLEAN DEFAULT FALSE,
    /// controlValue            OCTET STRING OPTIONAL }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    internal class RfcControl : Asn1Sequence
    {
        public virtual Asn1OctetString ControlType => (Asn1OctetString)Get(0);

        /// <summary>
        ///     Returns criticality.
        ///     If no value present, return the default value of FALSE.
        /// </summary>
        public virtual Asn1Boolean Criticality
        {
            get
            {
                if (Size() > 1)
                {
                    // MAY be a criticality
                    var obj = Get(1);
                    if (obj is Asn1Boolean)
                        return (Asn1Boolean)obj;
                }

                return new Asn1Boolean(false);
            }
        }

        /// <summary>
        ///     Since controlValue is an OPTIONAL component, we need to check
        ///     to see if one is available. Remember that if criticality is of default
        ///     value, it will not be present.
        /// </summary>
        /// <summary>
        ///     Called to set/replace the ControlValue.  Will normally be called by
        ///     the child classes after the parent has been instantiated.
        /// </summary>
        public virtual Asn1OctetString ControlValue
        {
            get
            {
                if (Size() > 2)
                {
                    // MUST be a control value
                    return (Asn1OctetString)Get(2);
                }

                if (Size() > 1)
                {
                    // MAY be a control value
                    var obj = Get(1);
                    if (obj is Asn1OctetString)
                        return (Asn1OctetString)obj;
                }

                return null;
            }
            set
            {
                if (value == null)
                    return;
                if (Size() == 3)
                {
                    // We already have a control value, replace it
                    Set(2, value);
                    return;
                }
                if (Size() == 2)
                {
                    // Get the second element
                    var obj = Get(1);
                    // Is this a control value
                    if (obj is Asn1OctetString)
                    {
                        // replace this one
                        Set(1, value);
                    }
                    else
                    {
                        // add a new one at the end
                        Add(value);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcControl"/> class.
        /// </summary>
        /// <param name="controlType">Type of the control.</param>
        public RfcControl(RfcLdapOID controlType)
            : this(controlType, new Asn1Boolean(false), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcControl"/> class.
        /// </summary>
        /// <param name="controlType">Type of the control.</param>
        /// <param name="criticality">The criticality.</param>
        public RfcControl(RfcLdapOID controlType, Asn1Boolean criticality)
            : this(controlType, criticality, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcControl"/> class.
        /// Note: criticality is only added if true, as per RFC 2251 sec 5.1 part
        /// (4): If a value of a type is its default value, it MUST be
        /// absent.
        /// </summary>
        /// <param name="controlType">Type of the control.</param>
        /// <param name="criticality">The criticality.</param>
        /// <param name="controlValue">The control value.</param>
        public RfcControl(RfcLdapOID controlType, Asn1Boolean criticality, Asn1OctetString controlValue)
            : base(3)
        {
            Add(controlType);
            if (criticality.BooleanValue())
                Add(criticality);
            if (controlValue != null)
                Add(controlValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcControl"/> class.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public RfcControl(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcControl"/> class.
        /// </summary>
        /// <param name="seqObj">The seq object.</param>
        public RfcControl(Asn1Sequence seqObj)
            : base(3)
        {
            var len = seqObj.Size();
            for (var i = 0; i < len; i++)
                Add(seqObj.Get(i));
        }
    }

    internal class RfcLdapOID : Asn1OctetString
    {
        public RfcLdapOID(string s)
            : base(s)
        {
        }

        public RfcLdapOID(sbyte[] s)
            : base(s)
        {
        }
    }

    /// <summary>
    /// Represents Ldap Sasl Credentials.
    /// <pre>
    /// SaslCredentials ::= SEQUENCE {
    /// mechanism               LdapString,
    /// credentials             OCTET STRING OPTIONAL }
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    internal class RfcSaslCredentials : Asn1Sequence
    {
        public RfcSaslCredentials(RfcLdapString mechanism)
            : this(mechanism, null)
        {
        }

        public RfcSaslCredentials(RfcLdapString mechanism, Asn1OctetString credentials) : base(2)
        {
            Add(mechanism);
            if (credentials != null)
                Add(credentials);
        }
    }

    /// <summary>
    /// Represents an Ldap Authentication Choice.
    /// <pre>
    /// AuthenticationChoice ::= CHOICE {
    /// simple                  [0] OCTET STRING,
    /// -- 1 and 2 reserved
    /// sasl                    [3] SaslCredentials }
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Choice" />
    internal class RfcAuthenticationChoice : Asn1Choice
    {
        public RfcAuthenticationChoice(Asn1Tagged choice)
            : base(choice)
        {
        }

        public RfcAuthenticationChoice(string mechanism, sbyte[] credentials)
            : base(
                new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, 3),
                    new RfcSaslCredentials(new RfcLdapString(mechanism),
                        credentials != null ? new Asn1OctetString(credentials) : null), false))
        {
            // implicit tagging
        }
    }

}
#endif
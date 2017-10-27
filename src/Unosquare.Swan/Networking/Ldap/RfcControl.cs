#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
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
    internal sealed class RfcControl : Asn1Sequence
    {
        public Asn1OctetString ControlType => (Asn1OctetString)Get(0);

        /// <summary>
        ///     Returns criticality.
        ///     If no value present, return the default value of FALSE.
        /// </summary>
        public Asn1Boolean Criticality
        {
            get
            {
                if (Size() > 1)
                {
                    // MAY be a criticality
                    var obj = Get(1);
                    if (obj is Asn1Boolean boolean)
                        return boolean;
                }

                return new Asn1Boolean(false);
            }
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
        public RfcControl(string controlType, Asn1Boolean criticality = null, Asn1OctetString controlValue = null)
            : base(3)
        {
            Add(controlType);
            Add(criticality ?? new Asn1Boolean(false));
            if (controlValue != null)
                Add(controlValue);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcControl"/> class.
        /// </summary>
        /// <param name="seqObj">The seq object.</param>
        public RfcControl(Asn1Sequence seqObj)
            : base(3)
        {
            for (var i = 0; i < seqObj.Size(); i++)
                Add(seqObj.Get(i));
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
        public Asn1OctetString ControlValue
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
                    if (obj is Asn1OctetString s)
                        return s;
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
    }

    /// <summary>
    /// Represents Ldap Sasl Credentials.
    /// <pre>
    /// SaslCredentials ::= SEQUENCE {
    /// mechanism               LdapString,
    /// credentials             OCTET STRING OPTIONAL }
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    internal sealed class RfcSaslCredentials : Asn1Sequence
    {
        public RfcSaslCredentials(string mechanism, sbyte[] credentials = null) 
            : base(2)
        {
            Add(mechanism);
            if (credentials != null)
                Add(new Asn1OctetString(credentials));
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
    internal sealed class RfcAuthenticationChoice : Asn1Choice
    {
        public RfcAuthenticationChoice(Asn1Tagged choice)
            : base(choice)
        {
        }

        public RfcAuthenticationChoice(string mechanism, sbyte[] credentials)
            : base(new Asn1Tagged(new Asn1Identifier(3, true), new RfcSaslCredentials(mechanism, credentials), false))
        {
            // implicit tagging
        }
    }
}
#endif
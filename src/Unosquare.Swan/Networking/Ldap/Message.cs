#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;

    /// <summary>
    /// The class performs token processing from strings
    /// </summary>
    internal class Tokenizer
    {
        // Element list identified
        private ArrayList elements;

        // Source string to use
        private string source;

        // The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character
        private readonly string delimiters = " \t\n\r";

        private readonly bool _returnDelims;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer" /> class.
        /// Initializes a new class instance with a specified string to process
        /// and the specified token delimiters to use
        /// </summary>
        /// <param name="source">String to tokenize</param>
        /// <param name="delimiters">String containing the delimiters</param>
        /// <param name="retDel">if set to <c>true</c> [ret delete].</param>
        public Tokenizer(string source, string delimiters, bool retDel = false)
        {
            elements = new ArrayList();
            this.delimiters = delimiters ?? this.delimiters;
            this.source = source;
            _returnDelims = retDel;
            if (_returnDelims)
                Tokenize();
            else
                elements.AddRange(source.Split(this.delimiters.ToCharArray()));
            RemoveEmptyStrings();
        }

        /// <summary>
        ///     Current token count for the source string
        /// </summary>
        public int Count => elements.Count;

        private void Tokenize()
        {
            var tempstr = source;
            if (tempstr.IndexOfAny(delimiters.ToCharArray()) < 0 && tempstr.Length > 0)
            {
                elements.Add(tempstr);
            }
            else if (tempstr.IndexOfAny(delimiters.ToCharArray()) < 0 && tempstr.Length <= 0)
            {
                return;
            }

            while (tempstr.IndexOfAny(delimiters.ToCharArray()) >= 0)
            {
                if (tempstr.IndexOfAny(delimiters.ToCharArray()) == 0)
                {
                    if (tempstr.Length > 1)
                    {
                        elements.Add(tempstr.Substring(0, 1));
                        tempstr = tempstr.Substring(1);
                    }
                    else
                    {
                        tempstr = string.Empty;
                    }
                }
                else
                {
                    var toks = tempstr.Substring(0, tempstr.IndexOfAny(delimiters.ToCharArray()));
                    elements.Add(toks);
                    elements.Add(tempstr.Substring(toks.Length, 1));

                    tempstr = tempstr.Length > toks.Length + 1 ? tempstr.Substring(toks.Length + 1) : string.Empty;
                }
            }

            if (tempstr.Length > 0)
            {
                elements.Add(tempstr);
            }
        }

        /// <summary>
        ///     Determines if there are more tokens to return from the source string
        /// </summary>
        /// <returns>True or false, depending if there are more tokens</returns>
        public bool HasMoreTokens() => elements.Count > 0;

        /// <summary>
        ///     Returns the next token from the token list
        /// </summary>
        /// <returns>The string value of the token</returns>
        public string NextToken()
        {
            if (source == string.Empty) throw new Exception();

            string result;
            if (_returnDelims)
            {
                RemoveEmptyStrings();
                result = (string) elements[0];
                elements.RemoveAt(0);
                return result;
            }

            elements = new ArrayList();
            elements.AddRange(source.Split(delimiters.ToCharArray()));
            RemoveEmptyStrings();
            result = (string) elements[0];
            elements.RemoveAt(0);
            source = source.Remove(source.IndexOf(result), result.Length);
            source = source.TrimStart(delimiters.ToCharArray());
            return result;
        }

        /// <summary>
        ///     Removes all empty strings from the token list
        /// </summary>
        private void RemoveEmptyStrings()
        {
            for (var index = 0; index < elements.Count; index++)
            {
                if ((string) elements[index] != string.Empty) continue;

                elements.RemoveAt(index);
                index--;
            }
        }
    }

    /// <summary>
    /// Represents an Ldap Matching Rule Assertion.
    /// <pre>
    /// MatchingRuleAssertion ::= SEQUENCE {
    /// matchingRule    [1] MatchingRuleId OPTIONAL,
    /// type            [2] AttributeDescription OPTIONAL,
    /// matchValue      [3] AssertionValue,
    /// dnAttributes    [4] BOOLEAN DEFAULT FALSE }
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    internal class RfcMatchingRuleAssertion : Asn1Sequence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcMatchingRuleAssertion"/> class.
        /// Creates a MatchingRuleAssertion.
        /// The value null may be passed for an optional value that is not used.
        /// </summary>
        /// <param name="matchingRule">Optional matching rule.</param>
        /// <param name="type">Optional attribute description.</param>
        /// <param name="matchValue">The assertion value.</param>
        /// <param name="dnAttributes">Asn1Boolean value. (default false)</param>
        public RfcMatchingRuleAssertion(
            Asn1OctetString matchingRule,
            Asn1OctetString type, 
            Asn1OctetString matchValue,
            Asn1Boolean dnAttributes = null)
            : base(4)
        {
            if (matchingRule != null)
                Add(new Asn1Tagged(new Asn1Identifier(1), matchingRule, false));
            if (type != null)
                Add(new Asn1Tagged(new Asn1Identifier(2), type, false));

            Add(new Asn1Tagged(new Asn1Identifier(3), matchValue, false));

            // if dnAttributes if false, that is the default value and we must not
            // encode it. (See RFC 2251 5.1 number 4)
            if (dnAttributes != null && dnAttributes.BooleanValue())
                Add(new Asn1Tagged(new Asn1Identifier(4), dnAttributes, false));
        }
    }

    /// <summary>
    /// The AttributeDescriptionList is used to list attributes to be returned in
    /// a search request.
    /// <pre>
    /// AttributeDescriptionList ::= SEQUENCE OF
    /// AttributeDescription
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1SequenceOf" />
    internal class RfcAttributeDescriptionList : Asn1SequenceOf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcAttributeDescriptionList" /> class.
        /// Convenience constructor. This constructor will construct an
        /// AttributeDescriptionList using the supplied array of Strings.
        /// </summary>
        /// <param name="attrs">The attrs.</param>
        public RfcAttributeDescriptionList(string[] attrs)
            : base(attrs?.Length ?? 0)
        {
            if (attrs == null) return;

            foreach (var attr in attrs)
            {
                Add(attr);
            }
        }
    }

    /// <summary>
    /// Represents an Ldap Search Request.
    /// <pre>
    /// SearchRequest ::= [APPLICATION 3] SEQUENCE {
    /// baseObject      LdapDN,
    /// scope           ENUMERATED {
    /// baseObject              (0),
    /// singleLevel             (1),
    /// wholeSubtree            (2) },
    /// derefAliases    ENUMERATED {
    /// neverDerefAliases       (0),
    /// derefInSearching        (1),
    /// derefFindingBaseObj     (2),
    /// derefAlways             (3) },
    /// sizeLimit       INTEGER (0 .. maxInt),
    /// timeLimit       INTEGER (0 .. maxInt),
    /// typesOnly       BOOLEAN,
    /// filter          Filter,
    /// attributes      AttributeDescriptionList }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.IRfcRequest" />
    internal class RfcSearchRequest : Asn1Sequence, IRfcRequest
    {
        public RfcSearchRequest(
            string basePath,
            int scope,
            int derefAliases,
            int sizeLimit,
            int timeLimit,
            bool typesOnly,
            string filter,
            string[] attributes)
            : base(8)
        {
            Add(basePath);
            Add(new Asn1Enumerated(scope));
            Add(new Asn1Enumerated(derefAliases));
            Add(new Asn1Integer(sizeLimit));
            Add(new Asn1Integer(timeLimit));
            Add(new Asn1Boolean(typesOnly));
            Add(new RfcFilter(filter));
            Add(new RfcAttributeDescriptionList(attributes));
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// <pre>
        /// ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 3. (0x63)
        /// </pre>
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.SearchRequest);

        public string GetRequestDN() => ((Asn1OctetString) Get(0)).StringValue();
    }

    /// <summary>
    ///     Represents an Ldap Substring Filter.
    ///     <pre>
    ///         SubstringFilter ::= SEQUENCE {
    ///         type            AttributeDescription,
    ///         -- at least one must be present
    ///         substrings      SEQUENCE OF CHOICE {
    ///         initial [0] LdapString,
    ///         any     [1] LdapString,
    ///         final   [2] LdapString } }
    ///     </pre>
    /// </summary>
    internal class RfcSubstringFilter : Asn1Sequence
    {
        public RfcSubstringFilter(string type, Asn1SequenceOf substrings)
            : base(2)
        {
            Add(type);
            Add(substrings);
        }
    }

    /// <summary>
    ///     Represents an Ldap Attribute Value Assertion.
    ///     <pre>
    ///         AttributeValueAssertion ::= SEQUENCE {
    ///         attributeDesc   AttributeDescription,
    ///         assertionValue  AssertionValue }
    ///     </pre>
    /// </summary>
    internal class RfcAttributeValueAssertion : Asn1Sequence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcAttributeValueAssertion" /> class.
        /// Creates an Attribute Value Assertion.
        /// </summary>
        /// <param name="ad">The assertion description</param>
        /// <param name="av">The assertion value</param>
        public RfcAttributeValueAssertion(string ad, Asn1OctetString av)
            : base(2)
        {
            Add(ad);
            Add(av);
        }

        /// <summary>
        ///     Returns the attribute description.
        /// </summary>
        /// <returns>
        ///     the attribute description
        /// </returns>
        public virtual string AttributeDescription => ((Asn1OctetString) Get(0)).StringValue();

        /// <summary>
        ///     Returns the assertion value.
        /// </summary>
        /// <returns>
        ///     the assertion value.
        /// </returns>
        public virtual sbyte[] AssertionValue => ((Asn1OctetString) Get(1)).ByteValue();
    }

    /// <summary>
    /// Substring Operators
    /// </summary>
    internal enum SubstringOp
    {
        /// <summary>
        /// Search Filter Identifier for an INITIAL component of a SUBSTRING.
        /// Note: An initial SUBSTRING is represented as "value*".
        /// </summary>
        Initial = 0,

        /// <summary>
        /// Search Filter Identifier for an ANY component of a SUBSTRING.
        /// Note: An ANY SUBSTRING is represented as "*value*".
        /// </summary>
        Any = 1,

        /// <summary>
        /// Search Filter Identifier for a FINAL component of a SUBSTRING.
        /// Note: A FINAL SUBSTRING is represented as "*value".
        /// </summary>
        Final = 2
    }

    /// <summary>
    /// Filtering Operators
    /// </summary>
    internal enum FilterOp
    {
        /// <summary>
        /// Identifier for AND component.
        /// </summary>
        And = 0,

        /// <summary>
        /// Identifier for OR component.
        /// </summary>
        Or = 1,

        /// <summary>
        /// Identifier for NOT component.
        /// </summary>
        Not = 2,

        /// <summary>
        /// Identifier for EQUALITY_MATCH component.
        /// </summary>
        EqualityMatch = 3,

        /// <summary>
        /// Identifier for SUBSTRINGS component.
        /// </summary>
        Substrings = 4,

        /// <summary>
        /// Identifier for GREATER_OR_EQUAL component.
        /// </summary>
        GreaterOrEqual = 5,

        /// <summary>
        /// Identifier for LESS_OR_EQUAL component.
        /// </summary>
        LessOrEqual = 6,

        /// <summary>
        /// Identifier for PRESENT component.
        /// </summary>
        Present = 7,

        /// <summary>
        /// Identifier for APPROX_MATCH component.
        /// </summary>
        ApproxMatch = 8,

        /// <summary>
        /// Identifier for EXTENSIBLE_MATCH component.
        /// </summary>
        ExtensibleMatch = 9
    }

    /// <summary> Encapsulates an Ldap Bind properties</summary>
    internal class BindProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindProperties" /> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="dn">The dn.</param>
        /// <param name="method">The method.</param>
        /// <param name="anonymous">if set to <c>true</c> [anonymous].</param>
        public BindProperties(
            int version, 
            string dn, 
            string method, 
            bool anonymous)
        {
            ProtocolVersion = version;
            AuthenticationDN = dn;
            AuthenticationMethod = method;
            Anonymous = anonymous;
        }
        
        public int ProtocolVersion { get; }
        
        public string AuthenticationDN { get; }
        
        public string AuthenticationMethod { get; }
        
        public bool Anonymous { get; }
    }
}

#endif
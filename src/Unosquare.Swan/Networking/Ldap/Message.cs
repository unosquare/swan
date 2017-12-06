#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The class performs token processing from strings
    /// </summary>
    internal class Tokenizer
    {
        // The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character
        private readonly string _delimiters = " \t\n\r";

        private readonly bool _returnDelims;

        private List<string> _elements;

        private string _source;

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
            _elements = new List<string>();
            _delimiters = delimiters ?? _delimiters;
            _source = source;
            _returnDelims = retDel;
            if (_returnDelims)
                Tokenize();
            else
                _elements.AddRange(source.Split(_delimiters.ToCharArray()));
            RemoveEmptyStrings();
        }

        public int Count => _elements.Count;

        public bool HasMoreTokens() => _elements.Count > 0;

        public string NextToken()
        {
            if (_source == string.Empty) throw new Exception();

            string result;
            if (_returnDelims)
            {
                RemoveEmptyStrings();
                result = _elements[0];
                _elements.RemoveAt(0);
                return result;
            }

            _elements = new List<string>();
            _elements.AddRange(_source.Split(_delimiters.ToCharArray()));
            RemoveEmptyStrings();
            result = _elements[0];
            _elements.RemoveAt(0);
            _source = _source.Remove(_source.IndexOf(result, StringComparison.Ordinal), result.Length);
            _source = _source.TrimStart(_delimiters.ToCharArray());
            return result;
        }

        private void RemoveEmptyStrings()
        {
            for (var index = 0; index < _elements.Count; index++)
            {
                if (_elements[index] != string.Empty) continue;

                _elements.RemoveAt(index);
                index--;
            }
        }

        private void Tokenize()
        {
            var tempstr = _source;
            if (tempstr.IndexOfAny(_delimiters.ToCharArray()) < 0 && tempstr.Length > 0)
            {
                _elements.Add(tempstr);
            }
            else if (tempstr.IndexOfAny(_delimiters.ToCharArray()) < 0 && tempstr.Length <= 0)
            {
                return;
            }

            while (tempstr.IndexOfAny(_delimiters.ToCharArray()) >= 0)
            {
                if (tempstr.IndexOfAny(_delimiters.ToCharArray()) == 0)
                {
                    if (tempstr.Length > 1)
                    {
                        _elements.Add(tempstr.Substring(0, 1));
                        tempstr = tempstr.Substring(1);
                    }
                    else
                    {
                        tempstr = string.Empty;
                    }
                }
                else
                {
                    var toks = tempstr.Substring(0, tempstr.IndexOfAny(_delimiters.ToCharArray()));
                    _elements.Add(toks);
                    _elements.Add(tempstr.Substring(toks.Length, 1));

                    tempstr = tempstr.Length > toks.Length + 1 ? tempstr.Substring(toks.Length + 1) : string.Empty;
                }
            }

            if (tempstr.Length > 0)
            {
                _elements.Add(tempstr);
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
        public RfcMatchingRuleAssertion(
            string matchingRule,
            string type,
            sbyte[] matchValue,
            Asn1Boolean dnAttributes = null)
            : base(4)
        {
            if (matchingRule != null)
                Add(new Asn1Tagged(new Asn1Identifier(1), new Asn1OctetString(matchingRule), false));
            if (type != null)
                Add(new Asn1Tagged(new Asn1Identifier(2), new Asn1OctetString(type), false));

            Add(new Asn1Tagged(new Asn1Identifier(3), new Asn1OctetString(matchValue), false));

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
    /// Represents an Ldap Substring Filter.
    /// <pre>
    /// SubstringFilter ::= SEQUENCE {
    /// type            AttributeDescription,
    /// -- at least one must be present
    /// substrings      SEQUENCE OF CHOICE {
    /// initial [0] LdapString,
    /// any     [1] LdapString,
    /// final   [2] LdapString } }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    internal class RfcSubstringFilter : Asn1Sequence
    {
        public RfcSubstringFilter(string type, Asn1Object substrings)
            : base(2)
        {
            Add(type);
            Add(substrings);
        }
    }

    /// <summary>
    /// Represents an Ldap Attribute Value Assertion.
    /// <pre>
    /// AttributeValueAssertion ::= SEQUENCE {
    /// attributeDesc   AttributeDescription,
    /// assertionValue  AssertionValue }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    internal class RfcAttributeValueAssertion : Asn1Sequence
    {
        public RfcAttributeValueAssertion(string ad, sbyte[] av)
            : base(2)
        {
            Add(ad);
            Add(new Asn1OctetString(av));
        }

        public string AttributeDescription => ((Asn1OctetString) Get(0)).StringValue();

        public sbyte[] AssertionValue => ((Asn1OctetString) Get(1)).ByteValue();
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
﻿#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

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
        private string delimiters = " \t\n\r";
        private readonly bool returnDelims;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// Initializes a new class instance with a specified string to process
        /// </summary>
        /// <param name="source">String to tokenize</param>
        public Tokenizer(string source)
        {
            elements = new ArrayList();
            elements.AddRange(source.Split(delimiters.ToCharArray()));
            RemoveEmptyStrings();
            this.source = source;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        ///     Initializes a new class instance with a specified string to process
        ///     and the specified token delimiters to use
        /// </summary>
        /// <param name="source">String to tokenize</param>
        /// <param name="delimiters">String containing the delimiters</param>
        public Tokenizer(string source, string delimiters)
        {
            elements = new ArrayList();
            this.delimiters = delimiters;
            elements.AddRange(source.Split(this.delimiters.ToCharArray()));
            RemoveEmptyStrings();
            this.source = source;
        }

        public Tokenizer(string source, string delimiters, bool retDel)
        {
            elements = new ArrayList();
            this.delimiters = delimiters;
            this.source = source;
            returnDelims = retDel;
            if (returnDelims)
                Tokenize();
            else
                elements.AddRange(source.Split(this.delimiters.ToCharArray()));
            RemoveEmptyStrings();
        }

        private void Tokenize()
        {
            var tempstr = source;
            var toks = string.Empty;
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
                        tempstr = "";
                }
                else
                {
                    toks = tempstr.Substring(0, tempstr.IndexOfAny(delimiters.ToCharArray()));
                    elements.Add(toks);
                    elements.Add(tempstr.Substring(toks.Length, 1));
                    if (tempstr.Length > toks.Length + 1)
                    {
                        tempstr = tempstr.Substring(toks.Length + 1);
                    }
                    else
                        tempstr = "";
                }
            }
            if (tempstr.Length > 0)
            {
                elements.Add(tempstr);
            }
        }

        /// <summary>
        ///     Current token count for the source string
        /// </summary>
        public int Count => elements.Count;

        /// <summary>
        ///     Determines if there are more tokens to return from the source string
        /// </summary>
        /// <returns>True or false, depending if there are more tokens</returns>
        public bool HasMoreTokens()
        {
            return elements.Count > 0;
        }

        /// <summary>
        ///     Returns the next token from the token list
        /// </summary>
        /// <returns>The string value of the token</returns>
        public string NextToken()
        {
            string result;
            if (source == string.Empty) throw new Exception();
            if (returnDelims)
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
        ///     Returns the next token from the source string, using the provided
        ///     token delimiters
        /// </summary>
        /// <param name="delimiters">String containing the delimiters to use</param>
        /// <returns>The string value of the token</returns>
        public string NextToken(string delimiters)
        {
            this.delimiters = delimiters;
            return NextToken();
        }

        /// <summary>
        ///     Removes all empty strings from the token list
        /// </summary>
        private void RemoveEmptyStrings()
        {
            for (var index = 0; index < elements.Count; index++)
            {
                if ((string) elements[index] == string.Empty)
                {
                    elements.RemoveAt(index);
                    index--;
                }
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
        /// Creates a MatchingRuleAssertion with the only required parameter.
        /// </summary>
        /// <param name="matchValue">The assertion value.</param>
        public RfcMatchingRuleAssertion(Asn1OctetString matchValue) : this(null, null, matchValue, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcMatchingRuleAssertion"/> class.
        /// Creates a MatchingRuleAssertion.
        /// The value null may be passed for an optional value that is not used.
        /// </summary>
        /// <param name="matchingRule">Optional matching rule.</param>
        /// <param name="type">Optional attribute description.</param>
        /// <param name="matchValue">The assertion value.</param>
        /// <param name="dnAttributes">Asn1Boolean value. (default false)</param>
        public RfcMatchingRuleAssertion(RfcLdapString matchingRule, RfcLdapString type,
            Asn1OctetString matchValue, Asn1Boolean dnAttributes) : base(4)
        {
            if (matchingRule != null)
                Add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 1), matchingRule, false));
            if (type != null)
                Add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 2), type, false));
            Add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 3), matchValue, false));
            // if dnAttributes if false, that is the default value and we must not
            // encode it. (See RFC 2251 5.1 number 4)
            if (dnAttributes != null && dnAttributes.BooleanValue())
                Add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 4), dnAttributes, false));
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
        /// Initializes a new instance of the <see cref="RfcAttributeDescriptionList"/> class.
        /// </summary>
        /// <param name="size">Specifies the initial size of the collection.</param>
        public RfcAttributeDescriptionList(int size) 
            : base(size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcAttributeDescriptionList" /> class.
        /// Convenience constructor. This constructor will construct an
        /// AttributeDescriptionList using the supplied array of Strings.
        /// </summary>
        /// <param name="attrs">The attrs.</param>
        public RfcAttributeDescriptionList(string[] attrs)
            : base(attrs?.Length ?? 0)
        {
            if (attrs != null)
            {
                for (var i = 0; i < attrs.Length; i++)
                {
                    Add(new RfcLdapString(attrs[i]));
                }
            }
        }
    }

    /// <summary>
    ///     Represents an Ldap Search Request.
    ///     <pre>
    ///         SearchRequest ::= [APPLICATION 3] SEQUENCE {
    ///         baseObject      LdapDN,
    ///         scope           ENUMERATED {
    ///         baseObject              (0),
    ///         singleLevel             (1),
    ///         wholeSubtree            (2) },
    ///         derefAliases    ENUMERATED {
    ///         neverDerefAliases       (0),
    ///         derefInSearching        (1),
    ///         derefFindingBaseObj     (2),
    ///         derefAlways             (3) },
    ///         sizeLimit       INTEGER (0 .. maxInt),
    ///         timeLimit       INTEGER (0 .. maxInt),
    ///         typesOnly       BOOLEAN,
    ///         filter          Filter,
    ///         attributes      AttributeDescriptionList }
    ///     </pre>
    /// </summary>
    internal class RfcSearchRequest : Asn1Sequence, RfcRequest
    {
        public RfcSearchRequest(
            RfcLdapDN baseObject,
            Asn1Enumerated scope,
            Asn1Enumerated derefAliases,
            Asn1Integer sizeLimit, 
            Asn1Integer timeLimit,
            Asn1Boolean typesOnly, 
            RfcFilter filter,
            RfcAttributeDescriptionList attributes)
            : base(8)
        {
            Add(baseObject);
            Add(scope);
            Add(derefAliases);
            Add(sizeLimit);
            Add(timeLimit);
            Add(typesOnly);
            Add(filter);
            Add(attributes);
        }

        /// <summary>
        /// Constructs a new Search Request copying from an existing request.
        /// </summary>
        /// <param name="origRequest">The original request.</param>
        /// <param name="base_Renamed">The base renamed.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="request">if set to <c>true</c> [request].</param>
        internal RfcSearchRequest(Asn1Object[] origRequest, string base_Renamed, string filter, bool request)
            : base(origRequest, origRequest.Length)
        {
            // Replace the base if specified, otherwise keep original base
            if ((object) base_Renamed != null)
            {
                Set(0, new RfcLdapDN(base_Renamed));
            }
            // If this is a reencode of a search continuation reference
            // and if original scope was one-level, we need to change the scope to
            // base so we don't return objects a level deeper than requested
            if (request)
            {
                var scope = ((Asn1Enumerated) origRequest[1]).IntValue();
                if (scope == LdapConnection.SCOPE_ONE)
                {
                    Set(1, new Asn1Enumerated(LdapConnection.SCOPE_BASE));
                }
            }
            // Replace the filter if specified, otherwise keep original filter
            if ((object) filter != null)
            {
                Set(6, new RfcFilter(filter));
            }
        }

        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 3. (0x63)
        ///     </pre>
        /// </summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.SEARCH_REQUEST);
        }

        public RfcRequest dupRequest(string base_Renamed, string filter, bool request)
        {
            return new RfcSearchRequest(ToArray(), base_Renamed, filter, request);
        }

        public string GetRequestDN()
        {
            return ((RfcLdapDN) Get(0)).StringValue();
        }
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
        public RfcSubstringFilter(RfcLdapString type, Asn1SequenceOf substrings) : base(2)
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
        ///     Returns the attribute description.
        /// </summary>
        /// <returns>
        ///     the attribute description
        /// </returns>
        public virtual string AttributeDescription => ((RfcLdapString) Get(0)).StringValue();

        /// <summary>
        ///     Returns the assertion value.
        /// </summary>
        /// <returns>
        ///     the assertion value.
        /// </returns>
        public virtual sbyte[] AssertionValue => ((Asn1OctetString) Get(1)).ByteValue();

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcAttributeValueAssertion" /> class.
        /// Creates an Attribute Value Assertion.
        /// </summary>
        /// <param name="ad">The assertion description</param>
        /// <param name="av">The assertion value</param>
        public RfcAttributeValueAssertion(RfcLdapString ad, Asn1OctetString av) 
            : base(2)
        {
            Add(ad);
            Add(av);
        }
    }

    /// <summary>
    ///     Represents an Ldap Filter.
    ///     This filter object can be created from a String or can be built up
    ///     programatically by adding filter components one at a time.  Existing filter
    ///     components can be iterated though.
    ///     Each filter component has an integer identifier defined in this class.
    ///     The following are basic filter components: {@link #EQUALITY_MATCH},
    ///     {@link #GREATER_OR_EQUAL}, {@link #LESS_OR_EQUAL}, {@link #SUBSTRINGS},
    ///     {@link #PRESENT}, {@link #APPROX_MATCH}, {@link #EXTENSIBLE_MATCH}.
    ///     More filters can be nested together into more complex filters with the
    ///     following filter components: {@link #AND}, {@link #OR}, {@link #NOT}
    ///     Substrings can have three components:
    ///     <pre>
    ///         Filter ::= CHOICE {
    ///         and             [0] SET OF Filter,
    ///         or              [1] SET OF Filter,
    ///         not             [2] Filter,
    ///         equalityMatch   [3] AttributeValueAssertion,
    ///         substrings      [4] SubstringFilter,
    ///         greaterOrEqual  [5] AttributeValueAssertion,
    ///         lessOrEqual     [6] AttributeValueAssertion,
    ///         present         [7] AttributeDescription,
    ///         approxMatch     [8] AttributeValueAssertion,
    ///         extensibleMatch [9] MatchingRuleAssertion }
    ///     </pre>
    /// </summary>
    internal class RfcFilter : Asn1Choice
    {
        // Public variables for Filter
        /// <summary> Identifier for AND component.</summary>
        public const int AND = LdapSearchRequest.AND;

        /// <summary> Identifier for OR component.</summary>
        public const int OR = LdapSearchRequest.OR;

        /// <summary> Identifier for NOT component.</summary>
        public const int NOT = LdapSearchRequest.NOT;

        /// <summary> Identifier for EQUALITY_MATCH component.</summary>
        public const int EQUALITY_MATCH = LdapSearchRequest.EQUALITY_MATCH;

        /// <summary> Identifier for SUBSTRINGS component.</summary>
        public const int SUBSTRINGS = LdapSearchRequest.SUBSTRINGS;

        /// <summary> Identifier for GREATER_OR_EQUAL component.</summary>
        public const int GREATER_OR_EQUAL = LdapSearchRequest.GREATER_OR_EQUAL;

        /// <summary> Identifier for LESS_OR_EQUAL component.</summary>
        public const int LESS_OR_EQUAL = LdapSearchRequest.LESS_OR_EQUAL;

        /// <summary> Identifier for PRESENT component.</summary>
        public const int PRESENT = LdapSearchRequest.PRESENT;

        /// <summary> Identifier for APPROX_MATCH component.</summary>
        public const int APPROX_MATCH = LdapSearchRequest.APPROX_MATCH;

        /// <summary> Identifier for EXTENSIBLE_MATCH component.</summary>
        public const int EXTENSIBLE_MATCH = LdapSearchRequest.EXTENSIBLE_MATCH;

        /// <summary> Identifier for INITIAL component.</summary>
        public const int INITIAL = LdapSearchRequest.INITIAL;

        /// <summary> Identifier for ANY component.</summary>
        public const int ANY = LdapSearchRequest.ANY;

        /// <summary> Identifier for FINAL component.</summary>
        public const int FINAL = LdapSearchRequest.FINAL;

        // Private variables for Filter
        private FilterTokenizer ft;
        private Stack filterStack;
        private bool finalFound;

        /// <summary>
        /// Constructs a Filter object by parsing an RFC 2254 Search Filter String.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public RfcFilter(string filter)
            : base(null)
        {
            ChoiceValue = Parse(filter);
        }

        /// <summary>
        /// Constructs a Filter object that will be built up piece by piece.
        /// </summary>
        public RfcFilter() 
            : base(null)
        {
            filterStack = new Stack();
        }

        // Helper methods for RFC 2254 Search Filter parsing.

        /// <summary>
        /// Parses an RFC 2251 filter string into an ASN.1 Ldap Filter object.
        /// </summary>
        /// <param name="filterExpr">The filter expr.</param>
        /// <returns></returns>
        /// <exception cref="LdapLocalException">
        /// </exception>
        private Asn1Tagged Parse(string filterExpr)
        {
            if ((object) filterExpr == null || filterExpr.Equals(string.Empty))
            {
                filterExpr = new StringBuilder("(objectclass=*)").ToString();
            }
            int idx;
            if ((idx = filterExpr.IndexOf('\\')) != -1)
            {
                var sb = new StringBuilder(filterExpr);
                var i = idx;
                while (i < sb.Length - 1)
                {
                    var c = sb[i++];
                    if (c == '\\')
                    {
                        // found '\' (backslash)
                        // If V2 escape, turn to a V3 escape
                        c = sb[i];
                        if (c == '*' || c == '(' || c == ')' || c == '\\')
                        {
                            // Ldap v2 filter, convert them into hex chars
                            sb.Remove(i, i + 1 - i);
                            sb.Insert(i, Convert.ToString(c, 16));
                            i += 2;
                        }
                    }
                }
                filterExpr = sb.ToString();
            }
            // missing opening and closing parentheses, must be V2, add parentheses
            if (filterExpr[0] != '(' && filterExpr[filterExpr.Length - 1] != ')')
            {
                filterExpr = "(" + filterExpr + ")";
            }
            var ch = filterExpr[0];
            var len = filterExpr.Length;
            // missing opening parenthesis ?
            if (ch != '(')
            {
                throw new LdapLocalException(ExceptionMessages.MISSING_LEFT_PAREN, LdapException.FILTER_ERROR);
            }
            // missing closing parenthesis ?
            if (filterExpr[len - 1] != ')')
            {
                throw new LdapLocalException(ExceptionMessages.MISSING_RIGHT_PAREN, LdapException.FILTER_ERROR);
            }
            // unmatched parentheses ?
            var parenCount = 0;
            for (var i = 0; i < len; i++)
            {
                if (filterExpr[i] == '(')
                {
                    parenCount++;
                }
                if (filterExpr[i] == ')')
                {
                    parenCount--;
                }
            }
            if (parenCount > 0)
            {
                throw new LdapLocalException(ExceptionMessages.MISSING_RIGHT_PAREN, LdapException.FILTER_ERROR);
            }
            if (parenCount < 0)
            {
                throw new LdapLocalException(ExceptionMessages.MISSING_LEFT_PAREN, LdapException.FILTER_ERROR);
            }
            ft = new FilterTokenizer(this, filterExpr);
            return parseFilter();
        }

        /// <summary>
        /// Parses an RFC 2254 filter
        /// </summary>
        /// <returns></returns>
        private Asn1Tagged parseFilter()
        {
            ft.getLeftParen();
            var filter = parseFilterComp();
            ft.GetRightParen();
            return filter;
        }

        /// <summary>
        /// RFC 2254 filter helper method. Will Parse a filter component.
        /// </summary>
        /// <returns></returns>
        private Asn1Tagged parseFilterComp()
        {
            Asn1Tagged tag = null;
            var filterComp = ft.OpOrAttr;
            switch (filterComp)
            {
                case AND:
                case OR:
                    tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, filterComp), ParseFilterList(),
                        false);
                    break;
                case NOT:
                    tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, filterComp), parseFilter(),
                        true);
                    break;
                default:
                    var filterType = ft.FilterType;
                    var value_Renamed = ft.Value;
                    switch (filterType)
                    {
                        case GREATER_OR_EQUAL:
                        case LESS_OR_EQUAL:
                        case APPROX_MATCH:
                            tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, filterType), new RfcAttributeValueAssertion(new RfcLdapString(ft.Attr), new Asn1OctetString(UnescapeString(value_Renamed))), false);
                            break;
                        case EQUALITY_MATCH:
                            if (value_Renamed.Equals("*"))
                            {
                                // present
                                tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, PRESENT),
                                    new RfcLdapString(ft.Attr), false);
                            }
                            else if (value_Renamed.IndexOf('*') != -1)
                            {
                                var sub = new Tokenizer(value_Renamed, "*", true);
                                var seq = new Asn1SequenceOf(5);
                                var tokCnt = sub.Count;
                                var cnt = 0;
                                var lastTok = new StringBuilder("").ToString();
                                while (sub.HasMoreTokens())
                                {
                                    var subTok = sub.NextToken();
                                    cnt++;
                                    if (subTok.Equals("*"))
                                    {
                                        // if previous token was '*', and since the current
                                        // token is a '*', we need to insert 'any'
                                        if (lastTok.Equals(subTok))
                                        {
                                            // '**'
                                            seq.Add(
                                                new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, ANY), new RfcLdapString(UnescapeString(string.Empty)), false));
                                        }
                                    }
                                    else
                                    {
                                        // value (RfcLdapString)
                                        if (cnt == 1)
                                        {
                                            // initial
                                            seq.Add(
                                                new Asn1Tagged(
                                                    new Asn1Identifier(Asn1Identifier.CONTEXT, false, INITIAL), new RfcLdapString(UnescapeString(subTok)), false));
                                        }
                                        else if (cnt < tokCnt)
                                        {
                                            // any
                                            seq.Add(
                                                new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, ANY), new RfcLdapString(UnescapeString(subTok)), false));
                                        }
                                        else
                                        {
                                            // final
                                            seq.Add(
                                                new Asn1Tagged(
                                                    new Asn1Identifier(Asn1Identifier.CONTEXT, false, FINAL), new RfcLdapString(UnescapeString(subTok)), false));
                                        }
                                    }
                                    lastTok = subTok;
                                }
                                tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, SUBSTRINGS), new RfcSubstringFilter(new RfcLdapString(ft.Attr), seq), false);
                            }
                            else
                            {
                                tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, EQUALITY_MATCH), new RfcAttributeValueAssertion(new RfcLdapString(ft.Attr), new Asn1OctetString(UnescapeString(value_Renamed))), false);
                            }
                            break;
                        case EXTENSIBLE_MATCH:
                            string type = null, matchingRule = null;
                            var dnAttributes = false;
                            var st = new Tokenizer(ft.Attr, ":");
                            var first = true;
                            while (st.HasMoreTokens())
                            {
                                var s = st.NextToken().Trim();
                                if (first && !s.Equals(":"))
                                {
                                    type = s;
                                }
                                else if (s.Equals("dn"))
                                {
                                    dnAttributes = true;
                                }
                                else if (!s.Equals(":"))
                                {
                                    matchingRule = s;
                                }
                                first = false;
                            }
                            tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, EXTENSIBLE_MATCH), new RfcMatchingRuleAssertion((object) matchingRule == null ? null : new RfcLdapString(matchingRule),(object) type == null ? null : new RfcLdapString(type),new Asn1OctetString(UnescapeString(value_Renamed)),dnAttributes == false ? null : new Asn1Boolean(true)), false);
                            break;
                    }
                    break;
            }
            return tag;
        }

        /// <summary>
        /// Must have 1 or more Filters
        /// </summary>
        /// <returns></returns>
        private Asn1SetOf ParseFilterList()
        {
            var set_Renamed = new Asn1SetOf();
            set_Renamed.Add(parseFilter()); // must have at least 1 filter
            while (ft.PeekChar() == '(')
            {
                // check for more filters
                set_Renamed.Add(parseFilter());
            }
            return set_Renamed;
        }

        /// <summary>
        /// Convert hex character to an integer. Return -1 if char is something
        /// other than a hex char.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>Converted integer</returns>
        internal static int Hex2int(char c)
        {
            return c >= '0' && c <= '9'
                ? c - '0'
                : c >= 'A' && c <= 'F' ? c - 'A' + 10 : c >= 'a' && c <= 'f' ? c - 'a' + 10 : -1;
        }

        /// <summary>
        /// Replace escaped hex digits with the equivalent binary representation.
        /// Assume either V2 or V3 escape mechanisms:
        /// V2: \*,  \(,  \),  \\.
        /// V3: \2A, \28, \29, \5C, \00.
        /// </summary>
        /// <param name="string_Renamed">The string renamed.</param>
        /// <returns>
        /// octet-string encoding of the specified string.
        /// </returns>
        /// <exception cref="LdapLocalException">
        /// </exception>
        /// <exception cref="Exception">UTF-8 String encoding not supported by JVM</exception>
        /// <exception cref="Unosquare.Swan.Networking.Ldap.LdapLocalException">The exception.</exception>
        private sbyte[] UnescapeString(string string_Renamed)
        {
            // give octets enough space to grow
            var octets = new sbyte[string_Renamed.Length * 3];
            // index for string and octets
            int iString, iOctets;
            // escape==true means we are in an escape sequence.
            var escape = false;
            // escStart==true means we are reading the first character of an escape.
            var escStart = false;
            int ival, length = string_Renamed.Length;
            sbyte[] utf8Bytes;
            char ch; // Character we are adding to the octet string
            var ca = new char[1]; // used while converting multibyte UTF-8 char
            var temp = (char) 0; // holds the value of the escaped sequence
            // loop through each character of the string and copy them into octets
            // converting escaped sequences when needed
            for (iString = 0, iOctets = 0; iString < length; iString++)
            {
                ch = string_Renamed[iString];
                if (escape)
                {
                    if ((ival = Hex2int(ch)) < 0)
                    {
                        // Invalid escape value(not a hex character)
                        throw new LdapLocalException(ExceptionMessages.INVALID_ESCAPE, new object[] {ch}, LdapException.FILTER_ERROR);
                    }
                    // V3 escaped: \\**
                    if (escStart)
                    {
                        temp = (char) (ival << 4); // high bits of escaped char
                        escStart = false;
                    }
                    else
                    {
                        temp |= (char) ival; // all bits of escaped char
                        octets[iOctets++] = (sbyte) temp;
                        escStart = escape = false;
                    }
                }
                else if (ch == '\\')
                {
                    escStart = escape = true;
                }
                else
                {
                    try
                    {
                        // place the character into octets.
                        if (ch >= 0x01 && ch <= 0x27 || ch >= 0x2B && ch <= 0x5B || ch >= 0x5D)
                        {
                            // found valid char
                            if (ch <= 0x7f)
                            {
                                // char = %x01-27 / %x2b-5b / %x5d-7f
                                octets[iOctets++] = (sbyte) ch;
                            }
                            else
                            {
                                // char > 0x7f, could be encoded in 2 or 3 bytes
                                ca[0] = ch;
                                var encoder = Encoding.UTF8;
                                var ibytes = encoder.GetBytes(new string(ca));
                                utf8Bytes = ibytes.ToSByteArray();
                                // copy utf8 encoded character into octets
                                Array.Copy(utf8Bytes, 0, octets, iOctets, utf8Bytes.Length);
                                iOctets = iOctets + utf8Bytes.Length;
                            }
                            escape = false;
                        }
                        else
                        {
                            // found invalid character
                            var escString = "";
                            ca[0] = ch;
                            var encoder = Encoding.UTF8;
                            var ibytes = encoder.GetBytes(new string(ca));
                            utf8Bytes = ibytes.ToSByteArray();
                            for (var i = 0; i < utf8Bytes.Length; i++)
                            {
                                var u = utf8Bytes[i];
                                if (u >= 0 && u < 0x10)
                                {
                                    escString = escString + "\\0" + Convert.ToString(u & 0xff, 16);
                                }
                                else
                                {
                                    escString = escString + "\\" + Convert.ToString(u & 0xff, 16);
                                }
                            }
                            throw new LdapLocalException(ExceptionMessages.INVALID_CHAR_IN_FILTER, new object[] {ch, escString}, LdapException.FILTER_ERROR);
                        }
                    }
                    catch (IOException ue)
                    {
                        throw new Exception("UTF-8 String encoding not supported by JVM", ue);
                    }
                }
            }
            // Verify that any escape sequence completed
            if (escStart || escape)
            {
                throw new LdapLocalException(ExceptionMessages.SHORT_ESCAPE, LdapException.FILTER_ERROR);
            }

            var toReturn = new sbyte[iOctets];
            Array.Copy(octets, 0, toReturn, 0, iOctets);
            octets = null;
            return toReturn;
        }
        
        /// <summary>
        ///     Called by sequential filter building methods to add to a filter
        ///     component.
        ///     Verifies that the specified Asn1Object can be added, then adds the
        ///     object to the filter.
        /// </summary>
        /// <param name="current">
        ///     Filter component to be added to the filter
        ///     @throws LdapLocalException Occurs when an invalid component is added, or
        ///     when the component is out of sequence.
        /// </param>
        private void AddObject(Asn1Object current)
        {
            if (filterStack == null)
            {
                filterStack = new Stack();
            }
            if (choiceValue() == null)
            {
                //ChoiceValue is the root Asn1 node
                ChoiceValue = current;
            }
            else
            {
                var topOfStack = (Asn1Tagged) filterStack.Peek();
                var value_Renamed = topOfStack.taggedValue();
                if (value_Renamed == null)
                {
                    topOfStack.TaggedValue = current;
                    filterStack.Push(current);
                }
                else if (value_Renamed is Asn1SetOf)
                {
                    ((Asn1SetOf) value_Renamed).Add(current);
                }
                else if (value_Renamed is Asn1Set)
                {
                    ((Asn1Set) value_Renamed).Add(current);
                }
                else if (value_Renamed.GetIdentifier().Tag == LdapSearchRequest.NOT)
                {
                    throw new LdapLocalException("Attemp to create more than one 'not' sub-filter",
                        LdapException.FILTER_ERROR);
                }
            }
            var type = current.GetIdentifier().Tag;
            if (type == AND || type == OR || type == NOT)
            {
                filterStack.Push(current);
            }
        }

        /// <summary>
        /// Creates and addes a substrings filter component.
        /// startSubstrings must be immediatly followed by at least one
        /// {@link #addSubstring} method and one {@link #endSubstrings} method
        /// @throws Novell.Directory.Ldap.LdapLocalException
        /// Occurs when this component is created out of sequence.
        /// </summary>
        /// <param name="attrName">Name of the attribute.</param>
        public virtual void StartSubstrings(string attrName)
        {
            finalFound = false;
            var seq = new Asn1SequenceOf(5);
            Asn1Object current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, SUBSTRINGS),
                new RfcSubstringFilter(new RfcLdapString(attrName), seq), false);
            AddObject(current);
            filterStack.Push(seq);
        }

        /// <summary>
        /// Adds a Substring component of initial, any or final substring matching.
        /// This method can be invoked only if startSubString was the last filter-
        /// building method called.  A substring is not required to have an 'INITIAL'
        /// substring.  However, when a filter contains an 'INITIAL' substring only
        /// one can be added, and it must be the first substring added. Any number of
        /// 'ANY' substrings can be added. A substring is not required to have a
        /// 'FINAL' substrings either.  However, when a filter does contain a 'FINAL'
        /// substring only one can be added, and it must be the last substring added.
        /// </summary>
        /// <param name="type">Substring type: INITIAL | ANY | FINAL]</param>
        /// <param name="value_Renamed">The value renamed.</param>
        /// <exception cref="Unosquare.Swan.Networking.Ldap.LdapLocalException">
        /// Attempt to add an invalid " + "substring type
        /// or
        /// Attempt to add an initial " + "substring match after the first substring
        /// or
        /// Attempt to add a substring " + "match after a final substring match
        /// or
        /// A call to addSubstring occured " + "without calling startSubstring
        /// </exception>
        public virtual void AddSubstring(int type, sbyte[] value_Renamed)
        {
            try
            {
                var substringSeq = (Asn1SequenceOf) filterStack.Peek();
                if (type != INITIAL && type != ANY && type != FINAL)
                {
                    throw new LdapLocalException("Attempt to add an invalid " + "substring type",
                        LdapException.FILTER_ERROR);
                }
                if (type == INITIAL && substringSeq.Size() != 0)
                {
                    throw new LdapLocalException(
                        "Attempt to add an initial " + "substring match after the first substring",
                        LdapException.FILTER_ERROR);
                }
                if (finalFound)
                {
                    throw new LdapLocalException("Attempt to add a substring " + "match after a final substring match",
                        LdapException.FILTER_ERROR);
                }
                if (type == FINAL)
                {
                    finalFound = true;
                }
                substringSeq.Add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, type),
                    new RfcLdapString(value_Renamed), false));
            }
            catch (InvalidCastException e)
            {
                throw new LdapLocalException("A call to addSubstring occured " + "without calling startSubstring",
                    LdapException.FILTER_ERROR, e);
            }
        }

        /// <summary>
        ///     Completes a SubString filter component.
        ///     @throws LdapLocalException Occurs when this is called out of sequence,
        ///     or the substrings filter is empty.
        /// </summary>
        public virtual void EndSubstrings()
        {
            try
            {
                finalFound = false;
                var substringSeq = (Asn1SequenceOf) filterStack.Peek();
                if (substringSeq.Size() == 0)
                {
                    throw new LdapLocalException("Empty substring filter", LdapException.FILTER_ERROR);
                }
            }
            catch (InvalidCastException e)
            {
                throw new LdapLocalException("Missmatched ending of substrings", LdapException.FILTER_ERROR, e);
            }
            filterStack.Pop();
        }

        /// <summary>
        ///     Creates and adds an AttributeValueAssertion to the filter.
        /// </summary>
        /// <param name="rfcType">
        ///     Filter type: EQUALITY_MATCH | GREATER_OR_EQUAL
        ///     | LESS_OR_EQUAL | APPROX_MATCH ]
        /// </param>
        /// <param name="attrName">
        ///     Name of the attribute to be asserted
        /// </param>
        /// <param name="value">
        ///     Value of the attribute to be asserted
        ///     @throws LdapLocalException
        ///     Occurs when the filter type is not a valid attribute assertion.
        /// </param>
        public virtual void AddAttributeValueAssertion(int rfcType, string attrName, sbyte[] value_Renamed)
        {
            if (filterStack != null && !(filterStack.Count == 0) && filterStack.Peek() is Asn1SequenceOf)
            {
                throw new LdapLocalException("Cannot insert an attribute assertion in a substring",
                    LdapException.FILTER_ERROR);
            }
            if (rfcType != EQUALITY_MATCH && rfcType != GREATER_OR_EQUAL && rfcType != LESS_OR_EQUAL &&
                rfcType != APPROX_MATCH)
            {
                throw new LdapLocalException("Invalid filter type for AttributeValueAssertion",
                    LdapException.FILTER_ERROR);
            }

            Asn1Object current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, rfcType), new RfcAttributeValueAssertion(new RfcLdapString(attrName), new Asn1OctetString(value_Renamed)), false);
            AddObject(current);
        }

        /// <summary>
        ///     Creates and adds a present matching to the filter.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute to check for presence.
        ///     @throws LdapLocalException
        ///     Occurs if addPresent is called out of sequence.
        /// </param>
        public virtual void AddPresent(string attrName)
        {
            Asn1Object current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, PRESENT),
                new RfcLdapString(attrName), false);
            AddObject(current);
        }
        
        /// <summary>
        ///     Creates and adds the Asn1Tagged value for a nestedFilter: AND, OR, or
        ///     NOT.
        ///     Note that a Not nested filter can only have one filter, where AND
        ///     and OR do not
        /// </summary>
        /// <param name="rfcType">
        ///     Filter type:
        ///     [AND | OR | NOT]
        ///     @throws Novell.Directory.Ldap.LdapLocalException
        /// </param>
        public virtual void StartNestedFilter(int rfcType)
        {
            Asn1Object current;
            if (rfcType == AND || rfcType == OR)
            {
                current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, rfcType), new Asn1SetOf(),
                    false);
            }
            else if (rfcType == NOT)
            {
                current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, rfcType), null, true);
            }
            else
            {
                throw new LdapLocalException("Attempt to create a nested filter other than AND, OR or NOT",
                    LdapException.FILTER_ERROR);
            }
            AddObject(current);
        }

        /// <summary> Completes a nested filter and checks for the valid filter type.</summary>
        /// <param name="rfcType">
        ///     Type of filter to complete.
        ///     @throws Novell.Directory.Ldap.LdapLocalException  Occurs when the specified
        ///     type differs from the current filter component.
        /// </param>
        public virtual void EndNestedFilter(int rfcType)
        {
            if (rfcType == NOT)
            {
                //if this is a Not than Not should be the second thing on the stack
                filterStack.Pop();
            }
            var topOfStackType = ((Asn1Object) filterStack.Peek()).GetIdentifier().Tag;
            if (topOfStackType != rfcType)
            {
                throw new LdapLocalException("Missmatched ending of nested filter", LdapException.FILTER_ERROR);
            }
            filterStack.Pop();
        }

        /// <summary>
        ///     Creates an iterator over the preparsed segments of a filter.
        ///     The first object returned by an iterator is an integer indicating the
        ///     type of filter components.  Subseqence values are returned.  If a
        ///     component is of type 'AND' or 'OR' or 'NOT' then the value
        ///     returned is another iterator.  This iterator is used by ToString.
        /// </summary>
        /// <returns>
        ///     Iterator over filter segments
        /// </returns>
        public virtual IEnumerator GetFilterIterator()
        {
            return new FilterIterator(this, (Asn1Tagged) choiceValue());
        }

        /// <summary>
        /// Creates and returns a String representation of this filter.
        /// </summary>
        /// <returns>Filtered string.</returns>
        public virtual string FilterToString()
        {
            var filter = new StringBuilder();
            StringFilter(GetFilterIterator(), filter);
            return filter.ToString();
        }

        /// <summary>
        ///     Uses a filterIterator to create a string representation of a filter.
        /// </summary>
        /// <param name="itr">
        ///     Iterator of filter components
        /// </param>
        /// <param name="filter">
        ///     Buffer to place a string representation of the filter
        /// </param>
        /// <seealso cref="FilterIterator">
        /// </seealso>
        private static void StringFilter(IEnumerator itr, StringBuilder filter)
        {
            var op = -1;
            filter.Append('(');
            while (itr.MoveNext())
            {
                var filterpart = itr.Current;
                if (filterpart is int)
                {
                    op = (int) filterpart;
                    switch (op)
                    {
                        case AND:
                            filter.Append('&');
                            break;
                        case OR:
                            filter.Append('|');
                            break;
                        case NOT:
                            filter.Append('!');
                            break;
                        case EQUALITY_MATCH:
                        {
                            filter.Append((string) itr.Current);
                            filter.Append('=');
                            var value_Renamed = (sbyte[]) itr.Current;
                            filter.Append(ByteString(value_Renamed));
                            break;
                        }
                        case GREATER_OR_EQUAL:
                        {
                            filter.Append((string) itr.Current);
                            filter.Append(">=");
                            var value_Renamed = (sbyte[]) itr.Current;
                            filter.Append(ByteString(value_Renamed));
                            break;
                        }
                        case LESS_OR_EQUAL:
                        {
                            filter.Append((string) itr.Current);
                            filter.Append("<=");
                            var value_Renamed = (sbyte[]) itr.Current;
                            filter.Append(ByteString(value_Renamed));
                            break;
                        }
                        case PRESENT:
                            filter.Append((string) itr.Current);
                            filter.Append("=*");
                            break;
                        case APPROX_MATCH:
                            filter.Append((string) itr.Current);
                            filter.Append("~=");
                            var value_Renamed2 = (sbyte[]) itr.Current;
                            filter.Append(ByteString(value_Renamed2));
                            break;
                        case EXTENSIBLE_MATCH:
                            var oid = (string) itr.Current;
                            filter.Append((string) itr.Current);
                            filter.Append(':');
                            filter.Append(oid);
                            filter.Append(":=");
                            filter.Append((string) itr.Current);
                            break;
                        case SUBSTRINGS:
                        {
                            filter.Append((string) itr.Current);
                            filter.Append('=');
                            var noStarLast = false;
                            while (itr.MoveNext())
                            {
                                op = (int) itr.Current;
                                switch (op)
                                {
                                    case INITIAL:
                                        filter.Append((string) itr.Current);
                                        filter.Append('*');
                                        noStarLast = false;
                                        break;
                                    case ANY:
                                        if (noStarLast)
                                            filter.Append('*');
                                        filter.Append((string) itr.Current);
                                        filter.Append('*');
                                        noStarLast = false;
                                        break;
                                    case FINAL:
                                        if (noStarLast)
                                            filter.Append('*');
                                        filter.Append((string) itr.Current);
                                        break;
                                }
                            }
                            break;
                        }
                    }
                }
                else if (filterpart is IEnumerator)
                {
                    StringFilter((IEnumerator) filterpart, filter);
                }
            }

            filter.Append(')');
        }

        /// <summary>
        /// Convert a UTF8 encoded string, or binary data, into a String encoded for
        /// a string filter.
        /// </summary>
        /// <param name="value_Renamed">The value renamed.</param>
        /// <returns>String byte</returns>
        /// <exception cref="Exception">Default JVM does not support UTF-8 encoding" + e</exception>
        private static string ByteString(sbyte[] value_Renamed)
        {
            try
            {
                var dchar = Encoding.UTF8.GetChars(value_Renamed.ToByteArray());
                return new string(dchar);
            }
            catch (IOException e)
            {
                throw new Exception("Default JVM does not support UTF-8 encoding" + e);
            }
        }

        /// <summary>
        ///     This inner class wrappers the Search Filter with an iterator.
        ///     This iterator will give access to all the individual components
        ///     preparsed.  The first call to next will return an Integer identifying
        ///     the type of filter component.  Then the component values will be returned
        ///     AND, NOT, and OR components values will be returned as Iterators.
        /// </summary>
        private class FilterIterator : IEnumerator
        {
            public void Reset()
            {
            }

            private void InitBlock(RfcFilter enclosingInstance)
            {
                this.enclosingInstance = enclosingInstance;
            }

            private RfcFilter enclosingInstance;

            /// <summary>
            ///     Returns filter identifiers and components of a filter.
            ///     The first object returned is an Integer identifying
            ///     its type.
            /// </summary>
            public virtual object Current
            {
                get
                {
                    object toReturn = null;
                    if (!TagReturned)
                    {
                        TagReturned = true;
                        toReturn = root.GetIdentifier().Tag;
                    }
                    else
                    {
                        var asn1 = root.taggedValue();
                        if (asn1 is RfcLdapString)
                        {
                            //one value to iterate
                            hasMore = false;
                            toReturn = ((RfcLdapString) asn1).StringValue();
                        }
                        else if (asn1 is RfcSubstringFilter)
                        {
                            var sub = (RfcSubstringFilter) asn1;
                            if (Index == -1)
                            {
                                //return attribute name
                                Index = 0;
                                var attr = (RfcLdapString) sub.Get(0);
                                toReturn = attr.StringValue();
                            }
                            else if (Index % 2 == 0)
                            {
                                //return substring identifier
                                var substrs = (Asn1SequenceOf) sub.Get(1);
                                toReturn = ((Asn1Tagged) substrs.Get(Index / 2)).GetIdentifier().Tag;
                                Index++;
                            }
                            else
                            {
                                //return substring value
                                var substrs = (Asn1SequenceOf) sub.Get(1);
                                var tag = (Asn1Tagged) substrs.Get(Index / 2);
                                var value_Renamed = (RfcLdapString) tag.taggedValue();
                                toReturn = value_Renamed.StringValue();
                                Index++;
                            }
                            if (Index / 2 >= ((Asn1SequenceOf) sub.Get(1)).Size())
                            {
                                hasMore = false;
                            }
                        }
                        else if (asn1 is RfcAttributeValueAssertion)
                        {
                            // components: =,>=,<=,~=
                            var assertion = (RfcAttributeValueAssertion) asn1;
                            if (Index == -1)
                            {
                                toReturn = assertion.AttributeDescription;
                                Index = 1;
                            }
                            else if (Index == 1)
                            {
                                toReturn = assertion.AssertionValue;
                                Index = 2;
                                hasMore = false;
                            }
                        }
                        else if (asn1 is RfcMatchingRuleAssertion)
                        {
                            //Extensible match
                            var exMatch = (RfcMatchingRuleAssertion) asn1;
                            if (Index == -1)
                            {
                                Index = 0;
                            }
                            toReturn =
                                ((Asn1OctetString) ((Asn1Tagged) exMatch.Get(Index++)).taggedValue())
                                .StringValue();
                            if (Index > 2)
                            {
                                hasMore = false;
                            }
                        }
                        else if (asn1 is Asn1SetOf)
                        {
                            //AND and OR nested components
                            var set_Renamed = (Asn1SetOf) asn1;
                            if (Index == -1)
                            {
                                Index = 0;
                            }
                            toReturn = new FilterIterator(enclosingInstance,
                                (Asn1Tagged) set_Renamed.Get(Index++));
                            if (Index >= set_Renamed.Size())
                            {
                                hasMore = false;
                            }
                        }
                        else if (asn1 is Asn1Tagged)
                        {
                            //NOT nested component.
                            toReturn = new FilterIterator(enclosingInstance, (Asn1Tagged) asn1);
                            hasMore = false;
                        }
                    }
                    return toReturn;
                }
            }

            public RfcFilter Enclosing_Instance => enclosingInstance;

            internal readonly Asn1Tagged root;

            /// <summary>
            /// indicates if the identifier for a component has been returned yet
            /// </summary>
            internal bool TagReturned;

            /// <summary>
            /// indexes the several parts a component may have
            /// </summary>
            internal int Index = -1;

            private bool hasMore = true;

            public FilterIterator(RfcFilter enclosingInstance, Asn1Tagged root)
            {
                InitBlock(enclosingInstance);
                this.root = root;
            }

            public virtual bool MoveNext()
            {
                return hasMore;
            }

            public void Remove()
            {
                throw new NotSupportedException("Remove is not supported on a filter iterator");
            }
        }

        /// <summary>
        /// This inner class will tokenize the components of an RFC 2254 search filter.
        /// </summary>
        internal class FilterTokenizer
        {
            private void InitBlock(RfcFilter enclosingInstance)
            {
                this.enclosingInstance = enclosingInstance;
            }

            private RfcFilter enclosingInstance;

            /// <summary>
            ///     Reads either an operator, or an attribute, whichever is
            ///     next in the filter string.
            ///     If the next component is an attribute, it is read and stored in the
            ///     attr field of this class which may be retrieved with getAttr()
            ///     and a -1 is returned. Otherwise, the int value of the operator read is
            ///     returned.
            /// </summary>
            public virtual int OpOrAttr
            {
                get
                {
                    int index;
                    if (offset >= filterLength)
                    {
                        //"Unexpected end of filter",
                        throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
                    }
                    int ret;
                    int testChar = filter[offset];
                    if (testChar == '&')
                    {
                        offset++;
                        ret = AND;
                    }
                    else if (testChar == '|')
                    {
                        offset++;
                        ret = OR;
                    }
                    else if (testChar == '!')
                    {
                        offset++;
                        ret = NOT;
                    }
                    else
                    {
                        if (filter.Substring(offset).StartsWith(":="))
                        {
                            throw new LdapLocalException(ExceptionMessages.NO_MATCHING_RULE, LdapException.FILTER_ERROR);
                        }
                        if (filter.Substring(offset).StartsWith("::=") || filter.Substring(offset).StartsWith(":::="))
                        {
                            throw new LdapLocalException(ExceptionMessages.NO_DN_NOR_MATCHING_RULE,
                                LdapException.FILTER_ERROR);
                        }
                        // get first component of 'item' (attr or :dn or :matchingrule)
                        var delims = "=~<>()";
                        var sb = new StringBuilder();
                        while (delims.IndexOf(filter[offset]) == -1 &&
                               filter.Substring(offset).StartsWith(":=") == false)
                        {
                            sb.Append(filter[offset++]);
                        }
                        attr = sb.ToString().Trim();
                        // is there an attribute name specified in the filter ?
                        if (attr.Length == 0 || attr[0] == ';')
                        {
                            throw new LdapLocalException(ExceptionMessages.NO_ATTRIBUTE_NAME, LdapException.FILTER_ERROR);
                        }
                        for (index = 0; index < attr.Length; index++)
                        {
                            var atIndex = attr[index];
                            if (
                                !(char.IsLetterOrDigit(atIndex) || atIndex == '-' || atIndex == '.' || atIndex == ';' ||
                                  atIndex == ':'))
                            {
                                if (atIndex == '\\')
                                {
                                    throw new LdapLocalException(ExceptionMessages.INVALID_ESC_IN_DESCR,
                                        LdapException.FILTER_ERROR);
                                }
                                throw new LdapLocalException(ExceptionMessages.INVALID_CHAR_IN_DESCR,
                                    new object[] {atIndex}, LdapException.FILTER_ERROR);
                            }
                        }
                        // is there an option specified in the filter ?
                        index = attr.IndexOf(';');
                        if (index != -1 && index == attr.Length - 1)
                        {
                            throw new LdapLocalException(ExceptionMessages.NO_OPTION, LdapException.FILTER_ERROR);
                        }
                        ret = -1;
                    }
                    return ret;
                }
            }

            /// <summary>
            ///     Reads an RFC 2251 filter type from the filter string and returns its
            ///     int value.
            /// </summary>
            public virtual int FilterType
            {
                get
                {
                    if (offset >= filterLength)
                    {
                        //"Unexpected end of filter",
                        throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
                    }
                    int ret;
                    if (filter.Substring(offset).StartsWith(">="))
                    {
                        offset += 2;
                        ret = GREATER_OR_EQUAL;
                    }
                    else if (filter.Substring(offset).StartsWith("<="))
                    {
                        offset += 2;
                        ret = LESS_OR_EQUAL;
                    }
                    else if (filter.Substring(offset).StartsWith("~="))
                    {
                        offset += 2;
                        ret = APPROX_MATCH;
                    }
                    else if (filter.Substring(offset).StartsWith(":="))
                    {
                        offset += 2;
                        ret = EXTENSIBLE_MATCH;
                    }
                    else if (filter[offset] == '=')
                    {
                        offset++;
                        ret = EQUALITY_MATCH;
                    }
                    else
                    {
                        //"Invalid comparison operator",
                        throw new LdapLocalException(ExceptionMessages.INVALID_FILTER_COMPARISON,
                            LdapException.FILTER_ERROR);
                    }
                    return ret;
                }
            }

            /// <summary> Reads a value from a filter string.</summary>
            public virtual string Value
            {
                get
                {
                    if (offset >= filterLength)
                    {
                        //"Unexpected end of filter",
                        throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
                    }
                    var idx = filter.IndexOf(')', offset);
                    if (idx == -1)
                    {
                        idx = filterLength;
                    }
                    var ret = filter.Substring(offset, idx - offset);
                    offset = idx;
                    return ret;
                }
            }

            /// <summary> Returns the current attribute identifier.</summary>
            public virtual string Attr => attr;

            public RfcFilter Enclosing_Instance => enclosingInstance;

            // Private variables
            private readonly string filter; // The filter string to parse
            private string attr; // Name of the attribute just parsed
            private int offset; // Offset pointer into the filter string
            private readonly int filterLength; // Length of the filter string to parse
            // Constructor

            /// <summary>
            /// Constructs a FilterTokenizer for a filter.
            /// </summary>
            /// <param name="enclosingInstance">The enclosing instance.</param>
            /// <param name="filter">The filter.</param>
            public FilterTokenizer(RfcFilter enclosingInstance, string filter)
            {
                InitBlock(enclosingInstance);
                this.filter = filter;
                offset = 0;
                filterLength = filter.Length;
            }

            // Tokenizer methods

            /// <summary>
            /// Reads the current char and throws an Exception if it is not a left
            /// parenthesis.
            /// </summary>
            /// <exception cref="LdapLocalException">
            /// </exception>
            public void getLeftParen()
            {
                if (offset >= filterLength)
                {
                    //"Unexpected end of filter",
                    throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
                }
                if (filter[offset++] != '(')
                {
                    //"Missing left paren",
                    throw new LdapLocalException(ExceptionMessages.EXPECTING_LEFT_PAREN,
                        new object[] {filter[offset -= 1]}, LdapException.FILTER_ERROR);
                }
            }

            /// <summary>
            ///     Reads the current char and throws an Exception if it is not a right
            ///     parenthesis.
            /// </summary>
            public void GetRightParen()
            {
                if (offset >= filterLength)
                {
                    //"Unexpected end of filter",
                    throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
                }
                if (filter[offset++] != ')')
                {
                    //"Missing right paren",
                    throw new LdapLocalException(ExceptionMessages.EXPECTING_RIGHT_PAREN,
                        new object[] {filter[offset - 1]}, LdapException.FILTER_ERROR);
                }
            }

            /// <summary>
            /// Return the current char without advancing the offset pointer. This is
            /// used by ParseFilterList when determining if there are any more
            /// Filters in the list.
            /// </summary>
            /// <returns>Current char</returns>
            /// <exception cref="LdapLocalException"></exception>
            public char PeekChar()
            {
                if (offset >= filterLength)
                {
                    //"Unexpected end of filter",
                    throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
                }
                return filter[offset];
            }
        }
    }

    /// <summary> Encapsulates an Ldap Bind properties</summary>
    internal class BindProperties
    {
        /// <summary> gets the protocol version</summary>
        public virtual int ProtocolVersion { get; } = 3;

        /// <summary>
        ///     Gets the authentication dn
        /// </summary>
        /// <returns>
        ///     the authentication dn for this connection
        /// </returns>
        public virtual string AuthenticationDN { get; }

        /// <summary>
        ///     Gets the authentication method
        /// </summary>
        /// <returns>
        ///     the authentication method for this connection
        /// </returns>
        public virtual string AuthenticationMethod { get; }

        /// <summary>
        ///     Gets the SASL Bind properties
        /// </summary>
        /// <returns>
        ///     the sasl bind properties for this connection
        /// </returns>
        public virtual Hashtable SaslBindProperties { get; }

        /// <summary>
        ///     Gets the SASL callback handler
        /// </summary>
        /// <returns>
        ///     the sasl callback handler for this connection
        /// </returns>
        public virtual object SaslCallbackHandler { get; }

        /// <summary>
        ///     Indicates whether or not the bind properties specify an anonymous bind
        /// </summary>
        /// <returns>
        ///     true if the bind properties specify an anonymous bind
        /// </returns>
        public virtual bool Anonymous { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindProperties" /> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="dn">The dn.</param>
        /// <param name="method">The method.</param>
        /// <param name="anonymous">if set to <c>true</c> [anonymous].</param>
        /// <param name="bindProperties">The bind properties.</param>
        /// <param name="bindCallbackHandler">The bind callback handler.</param>
        public BindProperties(int version, string dn, string method, bool anonymous, Hashtable bindProperties, object bindCallbackHandler)
        {
            ProtocolVersion = version;
            AuthenticationDN = dn;
            AuthenticationMethod = method;
            Anonymous = anonymous;
            SaslBindProperties = bindProperties;
            SaslCallbackHandler = bindCallbackHandler;
        }
    }
}

#endif
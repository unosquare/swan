#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Exceptions;

    /// <summary>
    /// Represents an Ldap Filter  by parsing an RFC 2254 Search Filter String.
    /// This filter object can be created from a String or can be built up
    /// programatically by adding filter components one at a time.  Existing filter
    /// components can be iterated though.
    /// Each filter component has an integer identifier defined in this class.
    /// The following are basic filter components: {EQUALITY_MATCH},
    /// {GREATER_OR_EQUAL}, {LESS_OR_EQUAL}, {SUBSTRINGS},
    /// {PRESENT}, {APPROX_MATCH}, {EXTENSIBLE_MATCH}.
    /// More filters can be nested together into more complex filters with the
    /// following filter components: {AND}, {OR}, {NOT}
    /// Substrings can have three components:
    /// <pre>
    /// Filter ::= CHOICE {
    /// and             [0] SET OF Filter,
    /// or              [1] SET OF Filter,
    /// not             [2] Filter,
    /// equalityMatch   [3] AttributeValueAssertion,
    /// substrings      [4] SubstringFilter,
    /// greaterOrEqual  [5] AttributeValueAssertion,
    /// lessOrEqual     [6] AttributeValueAssertion,
    /// present         [7] AttributeDescription,
    /// approxMatch     [8] AttributeValueAssertion,
    /// extensibleMatch [9] MatchingRuleAssertion }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Choice" />
    internal class RfcFilter : Asn1Choice
    {
        private FilterTokenizer _ft;

        private Stack<Asn1Object> _filterStack;
        private bool _finalFound;
        
        public RfcFilter(string filter)
        {
            ChoiceValue = Parse(filter);
        }
        
        /// <summary>
        /// Creates and addes a substrings filter component.
        /// startSubstrings must be immediatly followed by at least one
        /// <c>AddSubstring</c> method and one <c>EndSubstrings</c> method
        /// </summary>
        /// <param name="attrName">Name of the attribute.</param>
        public void StartSubstrings(string attrName)
        {
            _finalFound = false;
            var seq = new Asn1SequenceOf(5);
            Asn1Object current =
                new Asn1Tagged(new Asn1Identifier((int) FilterOp.Substrings, true),
                    new RfcSubstringFilter(attrName, seq),
                    false);
            AddObject(current);
            _filterStack.Push(seq);
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
        /// <param name="values">The value renamed.</param>
        /// <exception cref="LdapException">
        /// Attempt to add an invalid " + "substring type
        /// or
        /// Attempt to add an initial " + "substring match after the first substring
        /// or
        /// Attempt to add a substring " + "match after a final substring match
        /// or
        /// A call to addSubstring occured " + "without calling startSubstring
        /// </exception>
        public void AddSubstring(SubstringOp type, sbyte[] values)
        {
            try
            {
                var substringSeq = (Asn1SequenceOf) _filterStack.Peek();
                if (type != SubstringOp.Initial && type != SubstringOp.Any && type != SubstringOp.Final)
                {
                    throw new LdapException("Attempt to add an invalid substring type",
                        LdapStatusCode.FilterError);
                }

                if (type == SubstringOp.Initial && substringSeq.Size() != 0)
                {
                    throw new LdapException(
                        "Attempt to add an initial substring match after the first substring",
                        LdapStatusCode.FilterError);
                }

                if (_finalFound)
                {
                    throw new LdapException("Attempt to add a substring match after a final substring match",
                        LdapStatusCode.FilterError);
                }

                if (type == SubstringOp.Final)
                {
                    _finalFound = true;
                }

                substringSeq.Add(new Asn1Tagged(new Asn1Identifier((int) type), values));
            }
            catch (InvalidCastException)
            {
                throw new LdapException("A call to addSubstring occured without calling startSubstring",
                    LdapStatusCode.FilterError);
            }
        }

        /// <summary>
        /// Completes a SubString filter component.
        /// </summary>
        /// <exception cref="LdapException">
        /// Empty substring filter
        /// or
        /// Missmatched ending of substrings
        /// </exception>
        public void EndSubstrings()
        {
            try
            {
                _finalFound = false;
                var substringSeq = (Asn1SequenceOf) _filterStack.Peek();

                if (substringSeq.Size() == 0)
                {
                    throw new LdapException("Empty substring filter", LdapStatusCode.FilterError);
                }
            }
            catch (InvalidCastException)
            {
                throw new LdapException("Missmatched ending of substrings", LdapStatusCode.FilterError);
            }

            _filterStack.Pop();
        }

        /// <summary>
        /// Creates and adds an AttributeValueAssertion to the filter.
        /// </summary>
        /// <param name="rfcType">Filter type: EQUALITY_MATCH | GREATER_OR_EQUAL
        /// | LESS_OR_EQUAL | APPROX_MATCH ]</param>
        /// <param name="attrName">Name of the attribute to be asserted</param>
        /// <param name="valueArray">Value of the attribute to be asserted</param>
        /// <exception cref="LdapException">
        /// Cannot insert an attribute assertion in a substring
        /// or
        /// Invalid filter type for AttributeValueAssertion
        /// </exception>
        public void AddAttributeValueAssertion(FilterOp rfcType, string attrName, sbyte[] valueArray)
        {
            if (_filterStack != null && _filterStack.Count != 0 && _filterStack.Peek() is Asn1SequenceOf)
            {
                throw new LdapException("Cannot insert an attribute assertion in a substring",
                    LdapStatusCode.FilterError);
            }

            if (rfcType != FilterOp.EqualityMatch && rfcType != FilterOp.GreaterOrEqual &&
                rfcType != FilterOp.LessOrEqual &&
                rfcType != FilterOp.ApproxMatch)
            {
                throw new LdapException("Invalid filter type for AttributeValueAssertion",
                    LdapStatusCode.FilterError);
            }

            Asn1Object current = new Asn1Tagged(
                new Asn1Identifier((int) rfcType, true),
                new RfcAttributeValueAssertion(attrName, valueArray),
                false);
            AddObject(current);
        }

        /// <summary>
        /// Creates and adds a present matching to the filter.
        /// </summary>
        /// <param name="attrName">Name of the attribute to check for presence.</param>
        public void AddPresent(string attrName)
        {
            Asn1Object current = new Asn1Tagged(
                new Asn1Identifier((int) FilterOp.Present),
                new Asn1OctetString(attrName),
                false);
            AddObject(current);
        }

        /// <summary>
        /// Creates and adds the Asn1Tagged value for a nestedFilter: AND, OR, or
        /// NOT.
        /// Note that a Not nested filter can only have one filter, where AND
        /// and OR do not
        /// </summary>
        /// <param name="rfcType">Filter type:
        /// [AND | OR | NOT]</param>
        /// <exception cref="LdapException">Attempt to create a nested filter other than AND, OR or NOT</exception>
        public void StartNestedFilter(FilterOp rfcType)
        {
            Asn1Object current;

            if (rfcType == FilterOp.And || rfcType == FilterOp.Or)
            {
                current = new Asn1Tagged(new Asn1Identifier((int) rfcType, true), new Asn1SetOf(), false);
            }
            else if (rfcType == FilterOp.Not)
            {
                current = new Asn1Tagged(new Asn1Identifier((int) rfcType, true));
            }
            else
            {
                throw new LdapException("Attempt to create a nested filter other than AND, OR or NOT",
                    LdapStatusCode.FilterError);
            }

            AddObject(current);
        }

        /// <summary>
        /// Completes a nested filter and checks for the valid filter type.
        /// </summary>
        /// <param name="rfcType">Type of filter to complete.</param>
        /// <exception cref="LdapException">Mismatched ending of nested filter</exception>
        public void EndNestedFilter(FilterOp rfcType)
        {
            if (rfcType == FilterOp.Not)
            {
                // if this is a Not than Not should be the second thing on the stack
                _filterStack.Pop();
            }

            var topOfStackType = _filterStack.Peek().GetIdentifier().Tag;
            if (topOfStackType != (int) rfcType)
            {
                throw new LdapException("Mismatched ending of nested filter", LdapStatusCode.FilterError);
            }

            _filterStack.Pop();
        }

        /// <summary>
        /// Creates an iterator over the preparsed segments of a filter.
        /// The first object returned by an iterator is an integer indicating the
        /// type of filter components.  Subseqence values are returned.  If a
        /// component is of type 'AND' or 'OR' or 'NOT' then the value
        /// returned is another iterator.  This iterator is used by ToString.
        /// </summary>
        /// <returns>
        /// Iterator over filter segments
        /// </returns>
        public IEnumerator GetFilterIterator() => new FilterIterator(this, (Asn1Tagged) ChoiceValue);

        /// <summary>
        /// Creates and returns a String representation of this filter.
        /// </summary>
        /// <returns>Filtered string.</returns>
        public string FilterToString()
        {
            var filter = new StringBuilder();
            StringFilter(GetFilterIterator(), filter);
            return filter.ToString();
        }

        private static void StringFilter(IEnumerator itr, StringBuilder filter)
        {
            filter.Append('(');
            while (itr.MoveNext())
            {
                var filterpart = itr.Current;
                if (filterpart is int i)
                {
                    var op = (FilterOp) i;
                    switch (op)
                    {
                        case FilterOp.And:
                            filter.Append('&');
                            break;
                        case FilterOp.Or:
                            filter.Append('|');
                            break;
                        case FilterOp.Not:
                            filter.Append('!');
                            break;
                        case FilterOp.EqualityMatch:
                        {
                            filter.Append((string) itr.Current);
                            filter.Append('=');
                            filter.Append(Encoding.UTF8.GetString((sbyte[]) itr.Current));
                            break;
                        }

                        case FilterOp.GreaterOrEqual:
                        {
                            filter
                                .Append((string) itr.Current)
                                .Append(">=")
                                .Append(Encoding.UTF8.GetString((sbyte[]) itr.Current));
                            break;
                        }

                        case FilterOp.LessOrEqual:
                        {
                            filter.Append((string) itr.Current);
                            filter.Append("<=");
                            filter.Append(Encoding.UTF8.GetString((sbyte[]) itr.Current));
                            break;
                        }

                        case FilterOp.Present:
                            filter.Append((string) itr.Current);
                            filter.Append("=*");
                            break;
                        case FilterOp.ApproxMatch:
                            filter.Append((string) itr.Current);
                            filter.Append("~=");
                            filter.Append(Encoding.UTF8.GetString((sbyte[]) itr.Current));
                            break;
                        case FilterOp.ExtensibleMatch:
                            var oid = (string) itr.Current;
                            filter.Append((string) itr.Current);
                            filter.Append(':');
                            filter.Append(oid);
                            filter.Append(":=");
                            filter.Append((string) itr.Current);
                            break;
                        case FilterOp.Substrings:
                        {
                            filter.Append((string) itr.Current);
                            filter.Append('=');

                            while (itr.MoveNext())
                            {
                                switch ((SubstringOp) (int) itr.Current)
                                {
                                    case SubstringOp.Initial:
                                        filter.Append(itr.Current as string);
                                        filter.Append('*');
                                        break;
                                    case SubstringOp.Any:
                                        filter.Append(itr.Current as string);
                                        filter.Append('*');
                                        break;
                                    case SubstringOp.Final:
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

        private Asn1Tagged Parse(string filterExpr)
        {
            if (string.IsNullOrWhiteSpace(filterExpr))
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

                    if (c != '\\') continue;

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
                throw new LdapException(LdapException.MissingLeftParen, LdapStatusCode.FilterError);
            }

            // missing closing parenthesis ?
            if (filterExpr[len - 1] != ')')
            {
                throw new LdapException(LdapException.MissingRightParen, LdapStatusCode.FilterError);
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
                throw new LdapException(LdapException.MissingRightParen, LdapStatusCode.FilterError);
            }

            if (parenCount < 0)
            {
                throw new LdapException(LdapException.MissingLeftParen, LdapStatusCode.FilterError);
            }

            _ft = new FilterTokenizer(this, filterExpr);
            return ParseFilter();
        }

        private Asn1Tagged ParseFilter()
        {
            _ft.GetLeftParen();
            var filter = ParseFilterComp();
            _ft.GetRightParen();
            return filter;
        }

        private Asn1Tagged ParseFilterComp()
        {
            Asn1Tagged tag = null;
            var filterComp = (FilterOp)_ft.OpOrAttr;

            switch (filterComp)
            {
                case FilterOp.And:
                case FilterOp.Or:
                    tag = new Asn1Tagged(new Asn1Identifier((int)filterComp, true),
                        ParseFilterList(),
                        false);
                    break;
                case FilterOp.Not:
                    tag = new Asn1Tagged(new Asn1Identifier((int)filterComp, true),
                        ParseFilter());
                    break;
                default:
                    var filterType = _ft.FilterType;
                    var valueRenamed = _ft.Value;

                    switch (filterType)
                    {
                        case FilterOp.GreaterOrEqual:
                        case FilterOp.LessOrEqual:
                        case FilterOp.ApproxMatch:
                            tag = new Asn1Tagged(new Asn1Identifier((int)filterType, true),
                                new RfcAttributeValueAssertion(_ft.Attr, UnescapeString(valueRenamed)),
                                false);
                            break;
                        case FilterOp.EqualityMatch:
                            if (valueRenamed.Equals("*"))
                            {
                                // present
                                tag = new Asn1Tagged(
                                    new Asn1Identifier((int)FilterOp.Present),
                                    new Asn1OctetString(_ft.Attr),
                                    false);
                            }
                            else if (valueRenamed.IndexOf('*') != -1)
                            {
                                var sub = new Tokenizer(valueRenamed, "*", true);
                                var seq = new Asn1SequenceOf(5);
                                var tokCnt = sub.Count;
                                var cnt = 0;
                                var lastTok = new StringBuilder(string.Empty).ToString();
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
                                            seq.Add(new Asn1Tagged(
                                                new Asn1Identifier((int)SubstringOp.Any),
                                                UnescapeString(string.Empty)));
                                        }
                                    }
                                    else
                                    {
                                        // value (RfcLdapString)
                                        if (cnt == 1)
                                        {
                                            // initial
                                            seq.Add(new Asn1Tagged(
                                                new Asn1Identifier((int)SubstringOp.Initial),
                                                UnescapeString(subTok)));
                                        }
                                        else if (cnt < tokCnt)
                                        {
                                            // any
                                            seq.Add(new Asn1Tagged(
                                                new Asn1Identifier((int)SubstringOp.Any),
                                                UnescapeString(subTok)));
                                        }
                                        else
                                        {
                                            // final
                                            seq.Add(new Asn1Tagged(
                                                new Asn1Identifier((int)SubstringOp.Final),
                                                UnescapeString(subTok)));
                                        }
                                    }

                                    lastTok = subTok;
                                }

                                tag = new Asn1Tagged(
                                    new Asn1Identifier((int)FilterOp.Substrings, true),
                                    new RfcSubstringFilter(_ft.Attr, seq),
                                    false);
                            }
                            else
                            {
                                tag = new Asn1Tagged(
                                    new Asn1Identifier((int)FilterOp.EqualityMatch, true),
                                    new RfcAttributeValueAssertion(_ft.Attr, UnescapeString(valueRenamed)),
                                    false);
                            }

                            break;
                        case FilterOp.ExtensibleMatch:
                            string type = null, matchingRule = null;
                            var attr = false;
                            var st = new Tokenizer(_ft.Attr, ":");
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
                                    attr = true;
                                }
                                else if (!s.Equals(":"))
                                {
                                    matchingRule = s;
                                }

                                first = false;
                            }

                            tag = new Asn1Tagged(
                                new Asn1Identifier((int)FilterOp.ExtensibleMatch, true),
                                new RfcMatchingRuleAssertion(matchingRule, type, UnescapeString(valueRenamed), attr == false ? null : new Asn1Boolean(true)), 
                                false);

                            break;
                    }

                    break;
            }

            return tag;
        }

        private Asn1SetOf ParseFilterList()
        {
            var setOf = new Asn1SetOf();
            setOf.Add(ParseFilter()); // must have at least 1 filter
            while (_ft.PeekChar() == '(')
            {
                // check for more filters
                setOf.Add(ParseFilter());
            }

            return setOf;
        }

        /// <summary>
        /// Replace escaped hex digits with the equivalent binary representation.
        /// Assume either V2 or V3 escape mechanisms:
        /// V2: \*,  \(,  \),  \\.
        /// V3: \2A, \28, \29, \5C, \00.
        /// </summary>
        /// <param name="value">The string renamed.</param>
        /// <returns>
        /// octet-string encoding of the specified string.
        /// </returns>
        /// <exception cref="LdapException">Invalid Escape</exception>
        /// <exception cref="Exception">UTF-8 String encoding not supported by JVM</exception>
        /// <exception cref="LdapException">The exception.</exception>
        private sbyte[] UnescapeString(string value)
        {
            var octets = new sbyte[value.Length * 3];
            int str, octs;
            var escape = false;
            var escStart = false;

            var length = value.Length;
            var ca = new char[1]; // used while converting multibyte UTF-8 char
            var temp = (char)0; // holds the value of the escaped sequence
            // loop through each character of the string and copy them into octets
            // converting escaped sequences when needed
            for (str = 0, octs = 0; str < length; str++)
            {
                var ch = value[str]; // Character we are adding to the octet string
                if (escape)
                {
                    int ival;
                    if ((ival = ch.Hex2Int()) < 0)
                    {
                        throw new LdapException($"Invalid value in escape sequence \"{ch}\"",
                            LdapStatusCode.FilterError);
                    }

                    // V3 escaped: \\**
                    if (escStart)
                    {
                        temp = (char)(ival << 4); // high bits of escaped char
                        escStart = false;
                    }
                    else
                    {
                        temp |= (char)ival; // all bits of escaped char
                        octets[octs++] = (sbyte)temp;
                        escStart = escape = false;
                    }
                }
                else if (ch == '\\')
                {
                    escStart = escape = true;
                }
                else
                {
                    // place the character into octets.
                    if ((ch >= 0x01 && ch <= 0x27) || (ch >= 0x2B && ch <= 0x5B) || ch >= 0x5D)
                    {
                        // found valid char
                        if (ch <= 0x7f)
                        {
                            // char = %x01-27 / %x2b-5b / %x5d-7f
                            octets[octs++] = (sbyte)ch;
                        }
                        else
                        {
                            // char > 0x7f, could be encoded in 2 or 3 bytes
                            ca[0] = ch;
                            var utf8Bytes = Encoding.UTF8.GetSBytes(new string(ca));

                            // copy utf8 encoded character into octets
                            Array.Copy(utf8Bytes, 0, octets, octs, utf8Bytes.Length);
                            octs = octs + utf8Bytes.Length;
                        }

                        escape = false;
                    }
                    else
                    {
                        // found invalid character
                        var escString = string.Empty;
                        ca[0] = ch;

                        foreach (var u in Encoding.UTF8.GetSBytes(new string(ca)))
                        {
                            if (u >= 0 && u < 0x10)
                            {
                                escString = escString + "\\0" + Convert.ToString(u & 0xff, 16);
                            }
                            else
                            {
                                escString = escString + "\\" + Convert.ToString(u & 0xff, 16);
                            }
                        }

                        throw new LdapException(
                            $"The invalid character \"{ch}\" needs to be escaped as \"{escString}\"",
                            LdapStatusCode.FilterError);
                    }
                }
            }

            // Verify that any escape sequence completed
            if (escStart || escape)
            {
                throw new LdapException("Incomplete escape sequence", LdapStatusCode.FilterError);
            }

            var toReturn = new sbyte[octs];
            Array.Copy(octets, 0, toReturn, 0, octs);

            return toReturn;
        }

        private void AddObject(Asn1Object current)
        {
            if (_filterStack == null)
            {
                _filterStack = new Stack<Asn1Object>();
            }

            if (ChoiceValue == null)
            {
                // ChoiceValue is the root Asn1 node
                ChoiceValue = current;
            }
            else
            {
                var topOfStack = (Asn1Tagged) _filterStack.Peek();
                var value = topOfStack.TaggedValue;

                if (value == null)
                {
                    topOfStack.TaggedValue = current;
                    _filterStack.Push(current);
                }
                else if (value is Asn1SetOf)
                {
                    ((Asn1SetOf) value).Add(current);
                }
                else if (value is Asn1Set)
                {
                    ((Asn1Set) value).Add(current);
                }
                else if (value.GetIdentifier().Tag == (int) FilterOp.Not)
                {
                    throw new LdapException("Attemp to create more than one 'not' sub-filter",
                        LdapStatusCode.FilterError);
                }
            }

            var type = (FilterOp) current.GetIdentifier().Tag;
            if (type == FilterOp.And || type == FilterOp.Or || type == FilterOp.Not)
            {
                _filterStack.Push(current);
            }
        }
        
        internal class FilterTokenizer
        {
            private readonly string _filter; // The filter string to parse
            private readonly int _filterLength; // Length of the filter string to parse
            private int _offset; // Offset pointer into the filter string

            public FilterTokenizer(RfcFilter enclosingInstance, string filter)
            {
                EnclosingInstance = enclosingInstance;
                _filter = filter;
                _offset = 0;
                _filterLength = filter.Length;
            }

            /// <summary>
            /// Reads either an operator, or an attribute, whichever is
            /// next in the filter string.
            /// If the next component is an attribute, it is read and stored in the
            /// attr field of this class which may be retrieved with getAttr()
            /// and a -1 is returned. Otherwise, the int value of the operator read is
            /// returned.
            /// </summary>
            /// <value>
            /// The op or attribute.
            /// </value>
            /// <exception cref="LdapException">Unexpect end</exception>
            public int OpOrAttr
            {
                get
                {
                    if (_offset >= _filterLength)
                    {
                        throw new LdapException(LdapException.UnexpectedEnd, LdapStatusCode.FilterError);
                    }

                    int ret;
                    int testChar = _filter[_offset];

                    if (testChar == '&')
                    {
                        _offset++;
                        ret = (int)FilterOp.And;
                    }
                    else if (testChar == '|')
                    {
                        _offset++;
                        ret = (int)FilterOp.Or;
                    }
                    else if (testChar == '!')
                    {
                        _offset++;
                        ret = (int)FilterOp.Not;
                    }
                    else
                    {
                        if (_filter.Substring(_offset).StartsWith(":="))
                        {
                            throw new LdapException("Missing matching rule", LdapStatusCode.FilterError);
                        }

                        if (_filter.Substring(_offset).StartsWith("::=") ||
                            _filter.Substring(_offset).StartsWith(":::="))
                        {
                            throw new LdapException("DN and matching rule not specified", LdapStatusCode.FilterError);
                        }

                        // get first component of 'item' (attr or :dn or :matchingrule)
                        const string delims = "=~<>()";
                        var sb = new StringBuilder();
                        while (delims.IndexOf(_filter[_offset]) == -1 &&
                               _filter.Substring(_offset).StartsWith(":=") == false)
                        {
                            sb.Append(_filter[_offset++]);
                        }

                        Attr = sb.ToString().Trim();

                        // is there an attribute name specified in the filter ?
                        if (Attr.Length == 0 || Attr[0] == ';')
                        {
                            throw new LdapException("Missing attribute description", LdapStatusCode.FilterError);
                        }

                        int index;
                        for (index = 0; index < Attr.Length; index++)
                        {
                            var atIndex = Attr[index];
                            if (
                                !(char.IsLetterOrDigit(atIndex) || atIndex == '-' || atIndex == '.' || atIndex == ';' ||
                                  atIndex == ':'))
                            {
                                if (atIndex == '\\')
                                {
                                    throw new LdapException("Escape sequence not allowed in attribute description",
                                        LdapStatusCode.FilterError);
                                }

                                throw new LdapException($"Invalid character \"{atIndex}\" in attribute description",
                                    LdapStatusCode.FilterError);
                            }
                        }

                        // is there an option specified in the filter ?
                        index = Attr.IndexOf(';');
                        if (index != -1 && index == Attr.Length - 1)
                        {
                            throw new LdapException("Semicolon present, but no option specified",
                                LdapStatusCode.FilterError);
                        }

                        ret = -1;
                    }

                    return ret;
                }
            }

            public FilterOp FilterType
            {
                get
                {
                    if (_offset >= _filterLength)
                    {
                        throw new LdapException(LdapException.UnexpectedEnd, LdapStatusCode.FilterError);
                    }

                    if (_filter.Substring(_offset).StartsWith(">="))
                    {
                        _offset += 2;
                        return FilterOp.GreaterOrEqual;
                    }

                    if (_filter.Substring(_offset).StartsWith("<="))
                    {
                        _offset += 2;
                        return FilterOp.LessOrEqual;
                    }

                    if (_filter.Substring(_offset).StartsWith("~="))
                    {
                        _offset += 2;
                        return FilterOp.ApproxMatch;
                    }

                    if (_filter.Substring(_offset).StartsWith(":="))
                    {
                        _offset += 2;
                        return FilterOp.ExtensibleMatch;
                    }

                    if (_filter[_offset] == '=')
                    {
                        _offset++;
                        return FilterOp.EqualityMatch;
                    }

                    throw new LdapException("Invalid comparison operator", LdapStatusCode.FilterError);
                }
            }

            public string Value
            {
                get
                {
                    if (_offset >= _filterLength)
                    {
                        throw new LdapException(LdapException.UnexpectedEnd, LdapStatusCode.FilterError);
                    }

                    var idx = _filter.IndexOf(')', _offset);
                    if (idx == -1)
                    {
                        idx = _filterLength;
                    }

                    var ret = _filter.Substring(_offset, idx - _offset);
                    _offset = idx;
                    return ret;
                }
            }

            public string Attr { get; private set; }

            public RfcFilter EnclosingInstance { get; }

            public void GetLeftParen()
            {
                if (_offset >= _filterLength)
                {
                    throw new LdapException(LdapException.UnexpectedEnd, LdapStatusCode.FilterError);
                }

                if (_filter[_offset++] != '(')
                {
                    throw new LdapException(string.Format(LdapException.ExpectingLeftParen, _filter[_offset -= 1]),
                        LdapStatusCode.FilterError);
                }
            }

            public void GetRightParen()
            {
                if (_offset >= _filterLength)
                {
                    throw new LdapException(LdapException.UnexpectedEnd, LdapStatusCode.FilterError);
                }

                if (_filter[_offset++] != ')')
                {
                    throw new LdapException(string.Format(LdapException.ExpectingRightParen, _filter[_offset - 1]),
                        LdapStatusCode.FilterError);
                }
            }

            public char PeekChar()
            {
                if (_offset >= _filterLength)
                {
                    throw new LdapException(LdapException.UnexpectedEnd, LdapStatusCode.FilterError);
                }

                return _filter[_offset];
            }
        }

        /// <summary>
        /// This inner class wrappers the Search Filter with an iterator.
        /// This iterator will give access to all the individual components
        /// preparsed.  The first call to next will return an Integer identifying
        /// the type of filter component.  Then the component values will be returned
        /// AND, NOT, and OR components values will be returned as Iterators.
        /// </summary>
        /// <seealso cref="System.Collections.IEnumerator" />
        private sealed class FilterIterator
            : IEnumerator
        {
            private readonly Asn1Tagged _root;

            private readonly RfcFilter _enclosingInstance;

            private bool _tagReturned;

            private int _index = -1;

            private bool _hasMore = true;

            public FilterIterator(RfcFilter enclosingInstance, Asn1Tagged root)
            {
                _enclosingInstance = enclosingInstance;
                _root = root;
            }

            public object Current
            {
                get
                {
                    object toReturn = null;

                    if (!_tagReturned)
                    {
                        _tagReturned = true;
                        toReturn = _root.GetIdentifier().Tag;
                    }
                    else
                    {
                        var asn1 = _root.TaggedValue;

                        if (asn1 is Asn1OctetString s)
                        {
                            // one value to iterate
                            _hasMore = false;
                            toReturn = s.StringValue();
                        }
                        else if (asn1 is RfcSubstringFilter sub)
                        {
                            if (_index == -1)
                            {
                                // return attribute name
                                _index = 0;
                                var attr = (Asn1OctetString) sub.Get(0);
                                toReturn = attr.StringValue();
                            }
                            else if (_index % 2 == 0)
                            {
                                // return substring identifier
                                var substrs = (Asn1SequenceOf) sub.Get(1);
                                toReturn = ((Asn1Tagged) substrs.Get(_index / 2)).GetIdentifier().Tag;
                                _index++;
                            }
                            else
                            {
                                // return substring value
                                var substrs = (Asn1SequenceOf) sub.Get(1);
                                var tag = (Asn1Tagged) substrs.Get(_index / 2);
                                toReturn = ((Asn1OctetString) tag.TaggedValue).StringValue();
                                _index++;
                            }

                            if (_index / 2 >= ((Asn1SequenceOf) sub.Get(1)).Size())
                            {
                                _hasMore = false;
                            }
                        }
                        else if (asn1 is RfcAttributeValueAssertion assertion)
                        {
                            // components: =,>=,<=,~=
                            if (_index == -1)
                            {
                                toReturn = assertion.AttributeDescription;
                                _index = 1;
                            }
                            else if (_index == 1)
                            {
                                toReturn = assertion.AssertionValue;
                                _index = 2;
                                _hasMore = false;
                            }
                        }
                        else if (asn1 is RfcMatchingRuleAssertion exMatch)
                        {
                            // Extensible match
                            if (_index == -1)
                            {
                                _index = 0;
                            }

                            toReturn =
                                ((Asn1OctetString) ((Asn1Tagged) exMatch.Get(_index++)).TaggedValue)
                                .StringValue();
                            if (_index > 2)
                            {
                                _hasMore = false;
                            }
                        }
                        else if (asn1 is Asn1SetOf setRenamed)
                        {
                            // AND and OR nested components
                            if (_index == -1)
                            {
                                _index = 0;
                            }

                            toReturn = new FilterIterator(_enclosingInstance,
                                (Asn1Tagged) setRenamed.Get(_index++));
                            if (_index >= setRenamed.Size())
                            {
                                _hasMore = false;
                            }
                        }
                        else if (asn1 is Asn1Tagged)
                        {
                            // NOT nested component.
                            toReturn = new FilterIterator(_enclosingInstance, (Asn1Tagged) asn1);
                            _hasMore = false;
                        }
                    }

                    return toReturn;
                }
            }

            public void Reset()
            {
            }

            public bool MoveNext() => _hasMore;
        }
    }
}
#endif
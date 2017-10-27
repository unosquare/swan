﻿#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.Linq;
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a single entry in a directory, consisting of
    /// a distinguished name (DN) and zero or more attributes.
    /// An instance of
    /// LdapEntry is created in order to add an entry to a directory, and
    /// instances of LdapEntry are returned on a search by enumerating an
    /// LdapSearchResults.
    /// </summary>
    /// <seealso cref="LdapAttribute"></seealso>
    /// <seealso cref="LdapAttributeSet"></seealso>
    public sealed class LdapEntry
    {
        /// <summary>
        /// Returns the distinguished name of the entry.
        /// </summary>
        /// <value>
        /// The dn.
        /// </value>
        public string DN => Dn;

        protected internal string Dn;
        protected internal LdapAttributeSet Attrs;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapEntry" /> class.
        /// Constructs a new entry with the specified distinguished name and set
        /// of attributes.
        /// </summary>
        /// <param name="dn">The distinguished name of the new entry. The
        /// value is not validated. An invalid distinguished
        /// name will cause operations using this entry to fail.</param>
        /// <param name="attrs">The initial set of attributes assigned to the
        /// entry.</param>
        public LdapEntry(string dn = null, LdapAttributeSet attrs = null)
        {
            if (dn == null)
            {
                dn = string.Empty;
            }

            Dn = dn;
            Attrs = attrs ?? new LdapAttributeSet();
        }

        /// <summary>
        ///     Returns the attributes matching the specified attrName.
        /// </summary>
        /// <param name="attrName">
        ///     The name of the attribute or attributes to return.
        /// </param>
        /// <returns>
        ///     An array of LdapAttribute objects.
        /// </returns>
        public LdapAttribute GetAttribute(string attrName)
            => Attrs.GetAttribute(attrName);

        /// <summary>
        ///     Returns the attribute set of the entry.
        ///     All base and subtype variants of all attributes are
        ///     returned. The LdapAttributeSet returned may be
        ///     empty if there are no attributes in the entry.
        /// </summary>
        /// <returns>
        ///     The attribute set of the entry.
        /// </returns>
        public LdapAttributeSet GetAttributeSet() => Attrs;

        /// <summary>
        ///     Returns an attribute set from the entry, consisting of only those
        ///     attributes matching the specified subtypes.
        ///     The getAttributeSet method can be used to extract only
        ///     a particular language variant subtype of each attribute,
        ///     if it exists. The "subtype" may be, for example, "lang-ja", "binary",
        ///     or "lang-ja;phonetic". If more than one subtype is specified, separated
        ///     with a semicolon, only those attributes with all of the named
        ///     subtypes will be returned. The LdapAttributeSet returned may be
        ///     empty if there are no matching attributes in the entry.
        /// </summary>
        /// <param name="subtype">
        ///     One or more subtype specification(s), separated
        ///     with semicolons. The "lang-ja" and
        ///     "lang-en;phonetic" are valid subtype
        ///     specifications.
        /// </param>
        /// <returns>
        ///     An attribute set from the entry with the attributes that
        ///     match the specified subtypes or an empty set if no attributes
        ///     match.
        /// </returns>
        public LdapAttributeSet GetAttributeSet(string subtype) => Attrs.GetSubset(subtype);
    }

    /// <summary>
    /// The name and values of one attribute of a directory entry.
    /// LdapAttribute objects are used when searching for, adding,
    /// modifying, and deleting attributes from the directory.
    /// LdapAttributes are often used in conjunction with an
    /// LdapAttributeSet when retrieving or adding multiple
    /// attributes to an entry.
    /// </summary>
    public sealed class LdapAttribute
    {
        private readonly string _name; // full attribute name
        private readonly string _baseName; // cn of cn;lang-ja;phonetic
        private readonly string[] _subTypes; // lang-ja of cn;lang-ja
        private object[] _values; // Array of byte[] attribute values

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAttribute"/> class.
        /// Constructs an attribute with no values.
        /// </summary>
        /// <param name="attrName">Name of the attribute.
        /// @throws IllegalArgumentException if attrName is null</param>
        /// <exception cref="ArgumentException">Attribute name cannot be null</exception>
        public LdapAttribute(string attrName)
        {
            _name = attrName ?? throw new ArgumentNullException(nameof(attrName));
            _baseName = GetBaseName(attrName);
            _subTypes = GetSubtypes(attrName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAttribute" /> class.
        /// Constructs an attribute with a single string value.
        /// </summary>
        /// <param name="attrName">Name of the attribute.</param>
        /// <param name="attrString">Value of the attribute as a string.
        /// @throws IllegalArgumentException if attrName or attrString is null</param>
        /// <exception cref="ArgumentException">Attribute value cannot be null</exception>
        public LdapAttribute(string attrName, string attrString)
            : this(attrName)
        {
            if (attrString == null)
            {
                throw new ArgumentNullException(nameof(attrString));
            }

            Add(Encoding.UTF8.GetSBytes(attrString));
        }

        /// <summary>
        ///     Returns the values of the attribute as an array of bytes.
        /// </summary>
        /// <returns>
        ///     The values as an array of bytes or an empty array if there are
        ///     no values.
        /// </returns>
        public sbyte[][] ByteValueArray
        {
            get
            {
                if (_values == null)
                    return new sbyte[0][];

                var size = _values.Length;
                var bva = new sbyte[size][];

                // Deep copy so application cannot change values
                for (int i = 0, u = size; i < u; i++)
                {
                    bva[i] = new sbyte[((sbyte[])_values[i]).Length];
                    Array.Copy((Array)_values[i], 0, bva[i], 0, bva[i].Length);
                }

                return bva;
            }
        }

        /// <summary>
        ///     Returns the values of the attribute as an array of strings.
        /// </summary>
        /// <returns>
        ///     The values as an array of strings or an empty array if there are
        ///     no values
        /// </returns>
        public string[] StringValueArray
        {
            get
            {
                if (_values == null)
                    return new string[0];

                var size = _values.Length;
                var sva = new string[size];

                for (var j = 0; j < size; j++)
                {
                    sva[j] = Encoding.UTF8.GetString((sbyte[])_values[j]);
                }

                return sva;
            }
        }

        /// <summary>
        ///     Returns the the first value of the attribute as a <code>String</code>.
        /// </summary>
        /// <returns>
        ///     The UTF-8 encoded<code>String</code> value of the attribute's
        ///     value.  If the value wasn't a UTF-8 encoded <code>String</code>
        ///     to begin with the value of the returned <code>String</code> is
        ///     non deterministic.
        ///     If <code>this</code> attribute has more than one value the
        ///     first value is converted to a UTF-8 encoded <code>String</code>
        ///     and returned. It should be noted, that the directory may
        ///     return attribute values in any order, so that the first
        ///     value may vary from one call to another.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        public string StringValue => _values == null ? null : Encoding.UTF8.GetString((sbyte[])_values[0]);

        /// <summary>
        ///     Returns the the first value of the attribute as a byte array.
        /// </summary>
        /// <returns>
        ///     The binary value of <code>this</code> attribute or
        ///     <code>null</code> if <code>this</code> attribute doesn't have a value.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        public sbyte[] ByteValue
        {
            get
            {
                if (_values == null) return null;

                // Deep copy so app can't change the value
                var bva = new sbyte[((sbyte[])_values[0]).Length];
                Array.Copy((Array)_values[0], 0, bva, 0, bva.Length);

                return bva;
            }
        }

        /// <summary>
        ///     Returns the language subtype of the attribute, if any.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns the string, lang-ja.
        /// </summary>
        /// <returns>
        ///     The language subtype of the attribute or null if the attribute
        ///     has none.
        /// </returns>
        public string LangSubtype => _subTypes?.FirstOrDefault(t => t.StartsWith("lang-"));

        /// <summary>
        ///     Returns the name of the attribute.
        /// </summary>
        /// <returns>
        ///     The name of the attribute.
        /// </returns>
        public string Name => _name;

        /// <summary>
        ///     Replaces all values with the specified value. This protected method is
        ///     used by sub-classes of LdapSchemaElement because the value cannot be set
        ///     with a contructor.
        /// </summary>
        internal string Value
        {
            set
            {
                _values = null;

                Add(Encoding.UTF8.GetSBytes(value));
            }
        }

        /// <summary>
        /// Returns the base name of the specified attribute name.
        /// For example, if the attribute name is cn;lang-ja;phonetic,
        /// this method returns cn.
        /// </summary>
        /// <param name="attrName">Name of the attribute from which to extract the
        /// base name.</param>
        /// <returns> The base name of the attribute. </returns>
        /// <exception cref="ArgumentException">Attribute name cannot be null</exception>
        public static string GetBaseName(string attrName)
        {
            if (attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }

            var idx = attrName.IndexOf(';');
            return idx == -1 ? attrName : attrName.Substring(0, idx - 0);
        }

        /// <summary>
        ///     Returns a clone of this LdapAttribute.
        /// </summary>
        /// <returns>
        ///     clone of this LdapAttribute.
        /// </returns>
        public object Clone()
        {
            var newObj = MemberwiseClone();
            if (_values != null)
            {
                Array.Copy(_values, 0, ((LdapAttribute)newObj)._values, 0, _values.Length);
            }

            return newObj;
        }

        /// <summary>
        ///     Adds a string value to the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a String.
        /// </param>
        public void AddValue(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(Encoding.UTF8.GetSBytes(attrString));
        }

        /// <summary>
        ///     Adds a byte-formatted value to the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        /// </param>
        public void AddValue(sbyte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(attrBytes);
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  String
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrString">
        ///     The base64 value of the attribute as a String.
        /// </param>
        public void AddBase64Value(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(Convert.FromBase64String(attrString).ToSByteArray());
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  Character
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrString">
        ///     The base64 value of the attribute as a StringBuffer.
        /// </param>
        /// <param name="start">
        ///     The start index of base64 encoded part, inclusive.
        /// </param>
        /// <param name="end">
        ///     The end index of base encoded part, exclusive.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public void AddBase64Value(StringBuilder attrString, int start, int end)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(Convert.FromBase64String(attrString.ToString(start, end)).ToSByteArray());
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  Character
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrChars">
        ///     The base64 value of the attribute as an array of
        ///     characters.
        /// </param>
        public void AddBase64Value(char[] attrChars)
        {
            if (attrChars == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(Convert.FromBase64CharArray(attrChars, 0, attrChars.Length).ToSByteArray());
        }

        /// <summary>
        ///     Returns the base name of the attribute.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <returns>
        ///     The base name of the attribute.
        /// </returns>
        public string GetBaseName() => _baseName;

        /// <summary>
        ///     Extracts the subtypes from the attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        /// </returns>
        public string[] GetSubtypes() => _subTypes;

        /// <summary>
        /// Extracts the subtypes from the specified attribute name.
        /// For example, if the attribute name is cn;lang-ja;phonetic,
        /// this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <param name="attrName">Name of the attribute from which to extract
        /// the subtypes.</param>
        /// <returns>
        /// An array subtypes or null if the attribute has none.
        /// @throws IllegalArgumentException if attrName is null
        /// </returns>
        /// <exception cref="ArgumentException">Attribute name cannot be null</exception>
        public static string[] GetSubtypes(string attrName)
        {
            if (attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }

            var st = new Tokenizer(attrName, ";");
            string[] subTypes = null;
            var cnt = st.Count;

            if (cnt > 0)
            {
                st.NextToken(); // skip over basename
                subTypes = new string[cnt - 1];
                var i = 0;
                while (st.HasMoreTokens())
                {
                    subTypes[i++] = st.NextToken();
                }
            }

            return subTypes;
        }

        /// <summary>
        ///     Reports if the attribute name contains the specified subtype.
        ///     For example, if you check for the subtype lang-en and the
        ///     attribute name is cn;lang-en, this method returns true.
        /// </summary>
        /// <param name="subtype">
        ///     The single subtype to check for.
        /// </param>
        /// <returns>
        ///     True, if the attribute has the specified subtype;
        ///     false, if it doesn't.
        ///     @throws IllegalArgumentException if subtype is null
        /// </returns>
        public bool HasSubtype(string subtype)
        {
            if (subtype == null)
            {
                throw new ArgumentNullException(nameof(subtype));
            }

            return _subTypes != null && _subTypes.Any(t => t.ToUpper().Equals(subtype.ToUpper()));
        }

        /// <summary>
        ///     Reports if the attribute name contains all the specified subtypes.
        ///     For example, if you check for the subtypes lang-en and phonetic
        ///     and if the attribute name is cn;lang-en;phonetic, this method
        ///     returns true. If the attribute name is cn;phonetic or cn;lang-en,
        ///     this method returns false.
        /// </summary>
        /// <param name="subtypes">
        ///     An array of subtypes to check for.
        /// </param>
        /// <returns>
        ///     True, if the attribute has all the specified subtypes;
        ///     false, if it doesn't have all the subtypes.
        ///     @throws IllegalArgumentException if subtypes is null or if array member
        ///     is null.
        /// </returns>
        public bool HasSubtypes(string[] subtypes)
        {
            if (subtypes == null)
            {
                throw new ArgumentNullException(nameof(subtypes));
            }

            for (var i = 0; i < subtypes.Length; i++)
            {
                foreach (var sub in _subTypes)
                {
                    if (sub == null)
                    {
                        throw new ArgumentException($"subtype at array index {i} cannot be null");
                    }

                    if (sub.ToUpper().Equals(subtypes[i].ToUpper()))
                    {
                        goto gotSubType;
                    }
                }

                return false;
                gotSubType:
                ;
            }

            return true;
        }

        /// <summary>
        ///     Removes a string value from the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     Note: Removing a value which is not present in the attribute has
        ///     no effect.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public void RemoveValue(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            RemoveValue(Encoding.UTF8.GetSBytes(attrString));
        }

        /// <summary>
        ///     Removes a byte-formatted value from the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     Example: <code>String.getBytes("UTF-8");</code>
        ///     Note: Removing a value which is not present in the attribute has
        ///     no effect.
        ///     @throws IllegalArgumentException if attrBytes is null
        /// </param>
        public void RemoveValue(sbyte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            for (var i = 0; i < _values.Length; i++)
            {
                if (!Equals(attrBytes, (sbyte[])_values[i])) continue;

                if (i == 0 && _values.Length == 1)
                {
                    // Optimize if first element of a single valued attr
                    _values = null;
                    return;
                }

                if (_values.Length == 1)
                {
                    _values = null;
                }
                else
                {
                    var moved = _values.Length - i - 1;
                    var tmp = new object[_values.Length - 1];
                    if (i != 0)
                    {
                        Array.Copy(_values, 0, tmp, 0, i);
                    }

                    if (moved != 0)
                    {
                        Array.Copy(_values, i + 1, tmp, i, moved);
                    }

                    _values = tmp;
                }

                break;
            }
        }

        /// <summary>
        ///     Returns the number of values in the attribute.
        /// </summary>
        /// <returns>
        ///     The number of values in the attribute.
        /// </returns>
        public int Size() => _values?.Length ?? 0;

        /// <summary>
        ///     Compares this object with the specified object for order.
        ///     Ordering is determined by comparing attribute names (see
        ///     {getName() }) using the method compareTo() of the String class.
        /// </summary>
        /// <param name="attribute">
        ///     The LdapAttribute to be compared to this object.
        /// </param>
        /// <returns>
        ///     Returns a negative integer, zero, or a positive
        ///     integer as this object is less than, equal to, or greater than the
        ///     specified object.
        /// </returns>
        public int CompareTo(object attribute)
            => _name.CompareTo(((LdapAttribute)attribute)._name);

        /// <summary>
        ///     Adds an object to <code>this</code> object's list of attribute values
        /// </summary>
        /// <param name="bytes">
        ///     Ultimately all of this attribute's values are treated
        ///     as binary data so we simplify the process by requiring
        ///     that all data added to our list is in binary form.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        /// </param>
        private void Add(sbyte[] bytes)
        {
            if (_values == null)
            {
                _values = new object[] { bytes };
            }
            else
            {
                // Duplicate attribute values not allowed
                if (_values.Any(t => Equals(bytes, (sbyte[])t)))
                {
                    return; // Duplicate, don't add
                }

                var tmp = new object[_values.Length + 1];
                Array.Copy(_values, 0, tmp, 0, _values.Length);
                tmp[_values.Length] = bytes;
                _values = tmp;
            }
        }

        /// <summary>
        ///     Returns true if the two specified arrays of bytes are equal to each
        ///     another.  Matches the logic of Arrays.equals which is not available
        ///     in jdk 1.1.x.
        /// </summary>
        /// <param name="e1">
        ///     the first array to be tested
        /// </param>
        /// <param name="e2">
        ///     the second array to be tested
        /// </param>
        /// <returns>
        ///     true if the two arrays are equal
        /// </returns>
        private bool Equals(sbyte[] e1, sbyte[] e2)
        {
            // If same object, they compare true
            if (e1 == e2)
                return true;

            // If either but not both are null, they compare false
            if (e1 == null || e2 == null)
                return false;

            // If arrays have different length, they compare false
            var length = e1.Length;
            if (e2.Length != length)
                return false;

            // If any of the bytes are different, they compare false
            for (var i = 0; i < length; i++)
            {
                if (e1[i] != e2[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a string representation of this LdapAttribute
        /// </summary>
        /// <returns>
        /// a string representation of this LdapAttribute
        /// </returns>
        /// <exception cref="Exception">NullReferenceException</exception>
        public override string ToString()
        {
            var result = new StringBuilder("LdapAttribute: ");

            result.Append("{type='" + _name + "'");

            if (_values != null)
            {
                result.Append(", ");
                result.Append(_values.Length == 1 ? "value='" : "values='");

                for (var i = 0; i < _values.Length; i++)
                {
                    if (i != 0)
                    {
                        result.Append("','");
                    }

                    if (((sbyte[])_values[i]).Length == 0)
                    {
                        continue;
                    }

                    var sval = Encoding.UTF8.GetString((sbyte[])_values[i]);
                    if (sval.Length == 0)
                    {
                        // didn't decode well, must be binary
                        result.Append("<binary value, length:" + sval.Length);
                        continue;
                    }

                    result.Append(sval);
                }

                result.Append("'");
            }

            result.Append("}");

            return result.ToString();
        }
    }

    /// <summary>
    ///     A set of {@link LdapAttribute} objects.
    ///     An <code>LdapAttributeSet</code> is a collection of <code>LdapAttribute</code>
    ///     classes as returned from an <code>LdapEntry</code> on a search or read
    ///     operation. <code>LdapAttributeSet</code> may be also used to contruct an entry
    ///     to be added to a directory.  If the <code>add()</code> or <code>addAll()</code>
    ///     methods are called and one or more of the objects to be added is not
    ///     an <code>LdapAttribute, ClassCastException</code> is thrown (as discussed in the
    ///     documentation for <code>java.util.Collection</code>).
    /// </summary>
    /// <seealso cref="LdapAttribute">
    /// </seealso>
    /// <seealso cref="LdapEntry">
    /// </seealso>
    public class LdapAttributeSet : ArrayList
    {
        /// <summary>
        ///     This is the underlying data structure for this set.
        ///     HashSet is similar to the functionality of this set.  The difference
        ///     is we use the name of an attribute as keys in the Map and LdapAttributes
        ///     as the values.  We also do not declare the map as transient, making the
        ///     map serializable.
        /// </summary>
        private readonly Hashtable _map = new Hashtable();

        /// <summary>
        ///     Returns the number of attributes in this set.
        /// </summary>
        /// <returns>
        ///     number of attributes in this set.
        /// </returns>
        public override int Count => _map.Count;

        /// <summary>
        ///     Returns a deep copy of this attribute set.
        /// </summary>
        /// <returns>
        ///     A deep copy of this attribute set.
        /// </returns>
        public override object Clone()
        {
            var newObj = MemberwiseClone();
            var i = GetEnumerator();
            while (i.MoveNext())
            {
                ((LdapAttributeSet)newObj).Add(((LdapAttribute)i.Current).Clone());
            }

            return newObj;
        }

        /// <summary>
        ///     Returns the attribute matching the specified attrName.
        ///     For example:
        ///     <ul>
        ///         <li><code>getAttribute("cn")</code>      returns only the "cn" attribute</li>
        ///         <li>
        ///             <code>getAttribute("cn;lang-en")</code> returns only the "cn;lang-en"
        ///             attribute.
        ///         </li>
        ///     </ul>
        ///     In both cases, <code>null</code> is returned if there is no exact match to
        ///     the specified attrName.
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </summary>
        /// <param name="attrName">
        ///     The name of an attribute to retrieve, with or without
        ///     subtype specifications. For example, "cn", "cn;phonetic", and
        ///     "cn;binary" are valid attribute names.
        /// </param>
        /// <returns>
        ///     The attribute matching the specified attrName, or <code>null</code>
        ///     if there is no exact match.
        /// </returns>
        public virtual LdapAttribute GetAttribute(string attrName)
            => (LdapAttribute)_map[attrName.ToUpper()];

        /// <summary>
        ///     Returns a single best-match attribute, or <code>null</code> if no match is
        ///     available in the entry.
        ///     Ldap version 3 allows adding a subtype specification to an attribute
        ///     name. For example, "cn;lang-ja" indicates a Japanese language
        ///     subtype of the "cn" attribute and "cn;lang-ja-JP-kanji" may be a subtype
        ///     of "cn;lang-ja". This feature may be used to provide multiple
        ///     localizations in the same directory. For attributes which do not vary
        ///     among localizations, only the base attribute may be stored, whereas
        ///     for others there may be varying degrees of specialization.
        ///     For example, <code>getAttribute(attrName,lang)</code> returns the
        ///     <code>LdapAttribute</code> that exactly matches attrName and that
        ///     best matches lang.
        ///     If there are subtypes other than "lang" subtypes included
        ///     in attrName, for example, "cn;binary", only attributes with all of
        ///     those subtypes are returned. If lang is <code>null</code> or empty, the
        ///     method behaves as getAttribute(attrName). If there are no matching
        ///     attributes, <code>null</code> is returned.
        ///     Assume the entry contains only the following attributes:
        ///     <ul>
        ///         <li>cn;lang-en</li>
        ///         <li>cn;lang-ja-JP-kanji</li>
        ///         <li>sn</li>
        ///     </ul>
        ///     Examples:
        ///     <ul>
        ///         <li><code>getAttribute( "cn" )</code>       returns <code>null</code>.</li>
        ///         <li><code>getAttribute( "sn" )</code>       returns the "sn" attribute.</li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-en-us" )</code>
        ///             returns the "cn;lang-en" attribute.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-en" )</code>
        ///             returns the "cn;lang-en" attribute.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-ja" )</code>
        ///             returns <code>null</code>.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "sn", "lang-en" )</code>
        ///             returns the "sn" attribute.
        ///         </li>
        ///     </ul>
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </summary>
        /// <param name="attrName">
        ///     The name of an attribute to retrieve, with or without
        ///     subtype specifications. For example, "cn", "cn;phonetic", and
        ///     cn;binary" are valid attribute names.
        /// </param>
        /// <param name="lang">
        ///     A language specification with optional subtypes
        ///     appended using "-" as separator. For example, "lang-en", "lang-en-us",
        ///     "lang-ja", and "lang-ja-JP-kanji" are valid language specification.
        /// </param>
        /// <returns>
        ///     A single best-match <code>LdapAttribute</code>, or <code>null</code>
        ///     if no match is found in the entry.
        /// </returns>
        public virtual LdapAttribute GetAttribute(string attrName, string lang)
            => (LdapAttribute)_map[(attrName + ";" + lang).ToUpper()];

        /// <summary>
        ///     Creates a new attribute set containing only the attributes that have
        ///     the specified subtypes.
        ///     For example, suppose an attribute set contains the following
        ///     attributes:
        ///     <ul>
        ///         <li>    cn</li>
        ///         <li>    cn;lang-ja</li>
        ///         <li>    sn;phonetic;lang-ja</li>
        ///         <li>    sn;lang-us</li>
        ///     </ul>
        ///     Calling the <code>getSubset</code> method and passing lang-ja as the
        ///     argument, the method returns an attribute set containing the following
        ///     attributes:
        ///     <ul>
        ///         <li>cn;lang-ja</li>
        ///         <li>sn;phonetic;lang-ja</li>
        ///     </ul>
        /// </summary>
        /// <param name="subtype">
        ///     Semi-colon delimited list of subtypes to include. For
        ///     example:
        ///     <ul>
        ///         <li> "lang-ja" specifies only Japanese language subtypes</li>
        ///         <li> "binary" specifies only binary subtypes</li>
        ///         <li>
        ///             "binary;lang-ja" specifies only Japanese language subtypes
        ///             which also are binary
        ///         </li>
        ///     </ul>
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </param>
        /// <returns>
        ///     An attribute set containing the attributes that match the
        ///     specified subtype.
        /// </returns>
        public virtual LdapAttributeSet GetSubset(string subtype)
        {
            // Create a new tempAttributeSet
            var tempAttributeSet = new LdapAttributeSet();
            var i = GetEnumerator();

            // Cycle throught this.attributeSet
            while (i.MoveNext())
            {
                var attr = (LdapAttribute)i.Current;

                // Does this attribute have the subtype we are looking for. If
                // yes then add it to our AttributeSet, else next attribute
                if (attr.HasSubtype(subtype))
                    tempAttributeSet.Add(attr.Clone());
            }

            return tempAttributeSet;
        }

        /// <summary>
        /// Returns an iterator over the attributes in this set.  The attributes
        /// returned from this iterator are not in any particular order.
        /// </summary>
        /// <returns>
        /// iterator over the attributes in this set
        /// </returns>
        public IEnumerator GetEnumerator()
            => _map.Values.GetEnumerator();

        /// <summary>
        ///     Returns <code>true</code> if this set contains no elements
        /// </summary>
        /// <returns>
        ///     <code>true</code> if this set contains no elements
        /// </returns>
        public bool IsEmpty()
            => _map.Count == 0;

        /// <summary>
        ///     Returns <code>true</code> if this set contains an attribute of the same name
        ///     as the specified attribute.
        /// </summary>
        /// <param name="attr">
        ///     Object of type <code>LdapAttribute</code>
        /// </param>
        /// <returns>
        ///     true if this set contains the specified attribute
        ///     @throws ClassCastException occurs the specified Object
        ///     is not of type LdapAttribute.
        /// </returns>
        public override bool Contains(object attr)
            => _map.ContainsKey(((LdapAttribute)attr).Name.ToUpper());

        /// <summary>
        ///     Adds the specified attribute to this set if it is not already present.
        ///     If an attribute with the same name already exists in the set then the
        ///     specified attribute will not be added.
        /// </summary>
        /// <param name="attr">
        ///     Object of type <code>LdapAttribute</code>
        /// </param>
        /// <returns>
        ///     true if the attribute was added.
        ///     @throws ClassCastException occurs the specified Object
        ///     is not of type <code>LdapAttribute</code>.
        /// </returns>
        public bool Add(object attr)
        {
            var attribute = (LdapAttribute)attr;
            var name = attribute.Name.ToUpper();
            if (_map.ContainsKey(name))
                return false;

            _map[name] = attribute;
            return true;
        }

        /// <summary>
        /// Removes the specified object from this set if it is present.
        /// If the specified object is of type <code>LdapAttribute</code>, the
        /// specified attribute will be removed.  If the specified object is of type
        /// <code>String</code>, the attribute with a name that matches the string will
        /// be removed.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// true if the object was removed.
        /// @throws ClassCastException occurs the specified Object
        /// is not of type <code>LdapAttribute</code> or of type <code>String</code>.
        /// </returns>
        public bool Remove(object entry)
        {
            var attributeName = entry is string s ? s : ((LdapAttribute)entry).Name;

            if (attributeName == null)
            {
                return false;
            }

            var e = _map[attributeName.ToUpper()];
            _map.Remove(e);

            return true;
        }

        /// <summary>
        /// Removes all of the elements from this set.
        /// </summary>
        public override void Clear() => _map.Clear();

        /// <summary>
        ///     Returns a string representation of this LdapAttributeSet
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapAttributeSet
        /// </returns>
        public override string ToString()
        {
            var retValue = new StringBuilder("LdapAttributeSet: ");
            var attrs = GetEnumerator();
            var first = true;
            while (attrs.MoveNext())
            {
                if (!first)
                {
                    retValue.Append(" ");
                }

                first = false;
                var attr = (LdapAttribute)attrs.Current;
                retValue.Append(attr);
            }

            return retValue.ToString();
        }
    }
}
#endif
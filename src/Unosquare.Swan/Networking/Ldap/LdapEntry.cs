#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    
    /// <summary>
    ///     Represents a single entry in a directory, consisting of
    ///     a distinguished name (DN) and zero or more attributes.
    ///     An instance of
    ///     LdapEntry is created in order to add an entry to a directory, and
    ///     instances of LdapEntry are returned on a search by enumerating an
    ///     LdapSearchResults.
    /// </summary>
    /// <seealso cref="LdapAttribute">
    /// </seealso>
    /// <seealso cref="LdapAttributeSet">
    /// </seealso>
    public class LdapEntry
    {
        /// <summary>
        ///     Returns the distinguished name of the entry.
        /// </summary>
        /// <returns>
        ///     The distinguished name of the entry.
        /// </returns>
        public virtual string DN => dn;

        protected internal string dn;
        protected internal LdapAttributeSet attrs;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapEntry"/> class.
        /// Constructs an empty entry.
        /// </summary>
        public LdapEntry()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapEntry"/> class.
        ///     Constructs a new entry with the specified distinguished name and with
        ///     an empty attribute set.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry. The
        ///     value is not validated. An invalid distinguished
        ///     name will cause operations using this entry to fail.
        /// </param>
        public LdapEntry(string dn)
            : this(dn, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapEntry"/> class.
        ///     Constructs a new entry with the specified distinguished name and set
        ///     of attributes.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the new entry. The
        ///     value is not validated. An invalid distinguished
        ///     name will cause operations using this entry to fail.
        /// </param>
        /// <param name="attrs">
        ///     The initial set of attributes assigned to the
        ///     entry.
        /// </param>
        public LdapEntry(string dn, LdapAttributeSet attrs)
        {
            if ((object)dn == null)
            {
                dn = string.Empty;
            }
            
            this.dn = dn;
            this.attrs = attrs ?? new LdapAttributeSet();
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
        public virtual LdapAttribute GetAttribute(string attrName)
        {
            return attrs.GetAttribute(attrName);
        }

        /// <summary>
        ///     Returns the attribute set of the entry.
        ///     All base and subtype variants of all attributes are
        ///     returned. The LdapAttributeSet returned may be
        ///     empty if there are no attributes in the entry.
        /// </summary>
        /// <returns>
        ///     The attribute set of the entry.
        /// </returns>
        public virtual LdapAttributeSet GetAttributeSet()
        {
            return attrs;
        }

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
        public virtual LdapAttributeSet GetAttributeSet(string subtype)
        {
            return attrs.GetSubset(subtype);
        }
    }
    
    /// <summary>
    ///     The name and values of one attribute of a directory entry.
    ///     LdapAttribute objects are used when searching for, adding,
    ///     modifying, and deleting attributes from the directory.
    ///     LdapAttributes are often used in conjunction with an
    ///     {@link LdapAttributeSet} when retrieving or adding multiple
    ///     attributes to an entry.
    /// </summary>
    /// <seealso cref="LdapEntry">
    /// </seealso>
    /// <seealso cref="LdapAttributeSet">
    /// </seealso>
    /// <seealso cref="LdapModification">
    /// </seealso>
    public class LdapAttribute
    {
        /// <summary>
        ///     Returns the values of the attribute as an array of bytes.
        /// </summary>
        /// <returns>
        ///     The values as an array of bytes or an empty array if there are
        ///     no values.
        /// </returns>
        public virtual sbyte[][] ByteValueArray
        {
            get
            {
                if (values == null)
                    return new sbyte[0][];

                var size = values.Length;
                var bva = new sbyte[size][];

                // Deep copy so application cannot change values
                for (int i = 0, u = size; i < u; i++)
                {
                    bva[i] = new sbyte[((sbyte[])values[i]).Length];
                    Array.Copy((Array)values[i], 0, bva[i], 0, bva[i].Length);
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
        public virtual string[] StringValueArray
        {
            get
            {
                if (values == null)
                    return new string[0];

                var size = values.Length;
                var sva = new string[size];

                for (var j = 0; j < size; j++)
                {
                    try
                    {
                        var dchar = Encoding.UTF8.GetChars(((sbyte[])values[j]).ToByteArray());
                        sva[j] = new string(dchar);
                    }
                    catch (IOException uee)
                    {
                        // Exception should NEVER get thrown but just in case it does ...
                        throw new Exception(uee.ToString());
                    }
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
        public virtual string StringValue
        {
            get
            {
                string rval = null;

                if (values != null)
                {
                    try
                    {
                        var dchar = Encoding.UTF8.GetChars(((sbyte[])values[0]).ToByteArray());
                        rval = new string(dchar);
                    }
                    catch (IOException use)
                    {
                        throw new Exception(use.ToString());
                    }
                }

                return rval;
            }
        }

        /// <summary>
        ///     Returns the the first value of the attribute as a byte array.
        /// </summary>
        /// <returns>
        ///     The binary value of <code>this</code> attribute or
        ///     <code>null</code> if <code>this</code> attribute doesn't have a value.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        public virtual sbyte[] ByteValue
        {
            get
            {
                sbyte[] bva = null;
                if (values != null)
                {
                    // Deep copy so app can't change the value
                    bva = new sbyte[((sbyte[])values[0]).Length];
                    Array.Copy((Array)values[0], 0, bva, 0, bva.Length);
                }

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
        public virtual string LangSubtype
        {
            get
            {
                if (subTypes != null)
                {
                    for (var i = 0; i < subTypes.Length; i++)
                    {
                        if (subTypes[i].StartsWith("lang-"))
                        {
                            return subTypes[i];
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        ///     Returns the name of the attribute.
        /// </summary>
        /// <returns>
        ///     The name of the attribute.
        /// </returns>
        public virtual string Name => name;

        /// <summary>
        ///     Replaces all values with the specified value. This protected method is
        ///     used by sub-classes of LdapSchemaElement because the value cannot be set
        ///     with a contructor.
        /// </summary>
        protected internal virtual string Value
        {
            set
            {
                values = null;
                try
                {
                    var encoder = Encoding.UTF8;
                    var ibytes = encoder.GetBytes(value);
                    var sbytes = ibytes.ToSByteArray();
                    Add(sbytes);
                }
                catch (IOException ue)
                {
                    throw new Exception(ue.ToString());
                }
            }
        }

        private readonly string name; // full attribute name
        private readonly string baseName; // cn of cn;lang-ja;phonetic
        private readonly string[] subTypes; // lang-ja of cn;lang-ja
        private object[] values; // Array of byte[] attribute values

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAttribute" /> class.
        /// Constructs an attribute with copies of all values of the input
        /// attribute.
        /// </summary>
        /// <param name="attr">An LdapAttribute to use as a template.
        /// @throws IllegalArgumentException if attr is null</param>
        /// <exception cref="ArgumentException">LdapAttribute class cannot be null</exception>
        public LdapAttribute(LdapAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentException("LdapAttribute class cannot be null");
            }

            // Do a deep copy of the LdapAttribute template
            name = attr.name;
            baseName = attr.baseName;
            if (attr.subTypes != null)
            {
                subTypes = new string[attr.subTypes.Length];
                Array.Copy(attr.subTypes, 0, subTypes, 0, subTypes.Length);
            }

            // OK to just copy attributes, as the app only sees a deep copy of them
            if (attr.values != null)
            {
                values = new object[attr.values.Length];
                Array.Copy(attr.values, 0, values, 0, values.Length);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAttribute"/> class.
        /// Constructs an attribute with no values.
        /// </summary>
        /// <param name="attrName">Name of the attribute.
        /// @throws IllegalArgumentException if attrName is null</param>
        /// <exception cref="ArgumentException">Attribute name cannot be null</exception>
        public LdapAttribute(string attrName)
        {
            if ((object)attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }

            name = attrName;
            baseName = GetBaseName(attrName);
            subTypes = GetSubtypes(attrName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAttribute"/> class.
        ///     Constructs an attribute with a byte-formatted value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrName or attrBytes is null
        /// </param>
        public LdapAttribute(string attrName, sbyte[] attrBytes)
            : this(attrName)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            // Make our own copy of the byte array to prevent app from changing it
            var tmp = new sbyte[attrBytes.Length];
            Array.Copy(attrBytes, 0, tmp, 0, attrBytes.Length);
            Add(tmp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAttribute"/> class.
        ///     Constructs an attribute with a single string value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     @throws IllegalArgumentException if attrName or attrString is null
        /// </param>
        public LdapAttribute(string attrName, string attrString)
            : this(attrName)
        {
            if ((object)attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.UTF8;
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                Add(sbytes);
            }
            catch (IOException e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAttribute"/> class.
        ///     Constructs an attribute with an array of string values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrStrings">
        ///     Array of values as strings.
        ///     @throws IllegalArgumentException if attrName, attrStrings, or a member
        ///     of attrStrings is null
        /// </param>
        public LdapAttribute(string attrName, string[] attrStrings)
            : this(attrName)
        {
            if (attrStrings == null)
            {
                throw new ArgumentException("Attribute values array cannot be null");
            }

            for (int i = 0, u = attrStrings.Length; i < u; i++)
            {
                try
                {
                    if ((object)attrStrings[i] == null)
                    {
                        throw new ArgumentException("Attribute value " + "at array index " + i + " cannot be null");
                    }

                    var encoder = Encoding.UTF8;
                    var ibytes = encoder.GetBytes(attrStrings[i]);
                    var sbytes = ibytes.ToSByteArray();
                    Add(sbytes);
                }
                catch (IOException e)
                {
                    throw new Exception(e.ToString());
                }
            }
        }

        /// <summary>
        ///     Returns a clone of this LdapAttribute.
        /// </summary>
        /// <returns>
        ///     clone of this LdapAttribute.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                if (values != null)
                {
                    Array.Copy(values, 0, ((LdapAttribute)newObj).values, 0, values.Length);
                }

                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }

        /// <summary>
        ///     Adds a string value to the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a String.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void AddValue(string attrString)
        {
            if ((object)attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            try
            {
                var encoder = Encoding.UTF8;
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                Add(sbytes);
            }
            catch (IOException ue)
            {
                throw new Exception(ue.ToString());
            }
        }

        /// <summary>
        ///     Adds a byte-formatted value to the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrBytes is null
        /// </param>
        public virtual void AddValue(sbyte[] attrBytes)
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
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void AddBase64Value(string attrString)
        {
            if ((object)attrString == null)
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
        public virtual void AddBase64Value(StringBuilder attrString, int start, int end)
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
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void AddBase64Value(char[] attrChars)
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
        public virtual string GetBaseName()
        {
            return baseName;
        }

        /// <summary>
        ///     Returns the base name of the specified attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute from which to extract the
        ///     base name.
        /// </param>
        /// <returns>
        ///     The base name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null
        /// </returns>
        public static string GetBaseName(string attrName)
        {
            if ((object)attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }

            var idx = attrName.IndexOf(';');
            if (idx == -1)
            {
                return attrName;
            }

            return attrName.Substring(0, idx - 0);
        }

        /// <summary>
        ///     Extracts the subtypes from the attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        /// </returns>
        public virtual string[] GetSubtypes() =>  subTypes;

        /// <summary>
        ///     Extracts the subtypes from the specified attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute from which to extract
        ///     the subtypes.
        /// </param>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        ///     @throws IllegalArgumentException if attrName is null
        /// </returns>
        public static string[] GetSubtypes(string attrName)
        {
            if ((object)attrName == null)
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
        public virtual bool HasSubtype(string subtype)
        {
            if ((object)subtype == null)
            {
                throw new ArgumentException("subtype cannot be null");
            }

            if (subTypes != null)
            {
                for (var i = 0; i < subTypes.Length; i++)
                {
                    if (subTypes[i].ToUpper().Equals(subtype.ToUpper()))
                        return true;
                }
            }

            return false;
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
        public virtual bool HasSubtypes(string[] subtypes)
        {
            if (subtypes == null)
            {
                throw new ArgumentException("subtypes cannot be null");
            }

            for (var i = 0; i < subtypes.Length; i++)
            {
                for (var j = 0; j < subTypes.Length; j++)
                {
                    if ((object)subTypes[j] == null)
                    {
                        throw new ArgumentException("subtype " + "at array index " + i + " cannot be null");
                    }
                    if (subTypes[j].ToUpper().Equals(subtypes[i].ToUpper()))
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
        public virtual void RemoveValue(string attrString)
        {
            if ((object)attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            try
            {
                var encoder = Encoding.UTF8;
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                RemoveValue(sbytes);
            }
            catch (IOException uee)
            {
                throw new Exception(uee.ToString());
            }
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
        public virtual void RemoveValue(sbyte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (Equals(attrBytes, (sbyte[])values[i]))
                {
                    if (i == 0 && values.Length == 1)
                    {
                        // Optimize if first element of a single valued attr
                        values = null;
                        return;
                    }
                    if (values.Length == 1)
                    {
                        values = null;
                    }
                    else
                    {
                        var moved = values.Length - i - 1;
                        var tmp = new object[values.Length - 1];
                        if (i != 0)
                        {
                            Array.Copy(values, 0, tmp, 0, i);
                        }
                        if (moved != 0)
                        {
                            Array.Copy(values, i + 1, tmp, i, moved);
                        }
                        values = tmp;
                        tmp = null;
                    }
                    break;
                }
            }
        }

        /// <summary>
        ///     Returns the number of values in the attribute.
        /// </summary>
        /// <returns>
        ///     The number of values in the attribute.
        /// </returns>
        public virtual int Size() => values?.Length ?? 0;

        /// <summary>
        ///     Compares this object with the specified object for order.
        ///     Ordering is determined by comparing attribute names (see
        ///     {@link #getName() }) using the method compareTo() of the String class.
        /// </summary>
        /// <param name="attribute">
        ///     The LdapAttribute to be compared to this object.
        /// </param>
        /// <returns>
        ///     Returns a negative integer, zero, or a positive
        ///     integer as this object is less than, equal to, or greater than the
        ///     specified object.
        /// </returns>
        public virtual int CompareTo(object attribute)
        {
            return name.CompareTo(((LdapAttribute)attribute).name);
        }

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
            if (values == null)
            {
                values = new object[] { bytes };
            }
            else
            {
                // Duplicate attribute values not allowed
                for (var i = 0; i < values.Length; i++)
                {
                    if (Equals(bytes, (sbyte[])values[i]))
                    {
                        return; // Duplicate, don't add
                    }
                }

                var tmp = new object[values.Length + 1];
                Array.Copy(values, 0, tmp, 0, values.Length);
                tmp[values.Length] = bytes;
                values = tmp;
                tmp = null;
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
        /// <exception cref="Exception"></exception>
        public override string ToString()
        {
            var result = new StringBuilder("LdapAttribute: ");
            try
            {
                result.Append("{type='" + name + "'");
                if (values != null)
                {
                    result.Append(", ");
                    if (values.Length == 1)
                    {
                        result.Append("value='");
                    }
                    else
                    {
                        result.Append("values='");
                    }

                    for (var i = 0; i < values.Length; i++)
                    {
                        if (i != 0)
                        {
                            result.Append("','");
                        }
                        if (((sbyte[])values[i]).Length == 0)
                        {
                            continue;
                        }
                        var encoder = Encoding.UTF8;
                        var dchar = encoder.GetChars(((sbyte[])values[i]).ToByteArray());
                        var sval = new string(dchar);
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
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

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
    public class LdapAttributeSet : SetSupport
    {
        /// <summary>
        ///     Returns the number of attributes in this set.
        /// </summary>
        /// <returns>
        ///     number of attributes in this set.
        /// </returns>
        public override int Count => map.Count;

        /// <summary>
        ///     This is the underlying data structure for this set.
        ///     HashSet is similar to the functionality of this set.  The difference
        ///     is we use the name of an attribute as keys in the Map and LdapAttributes
        ///     as the values.  We also do not declare the map as transient, making the
        ///     map serializable.
        /// </summary>
        private readonly Hashtable map;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAttributeSet"/> class.
        /// Constructs an empty set of attributes.
        /// </summary>
        public LdapAttributeSet()
        {
            map = new Hashtable();
        }

        /// <summary>
        ///     Returns a deep copy of this attribute set.
        /// </summary>
        /// <returns>
        ///     A deep copy of this attribute set.
        /// </returns>
        public override object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                var i = GetEnumerator();
                while (i.MoveNext())
                {
                    ((LdapAttributeSet)newObj).Add(((LdapAttribute)i.Current).Clone());
                }

                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
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
        {
            return (LdapAttribute)map[attrName.ToUpper()];
        }

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
        {
            var key = attrName + ";" + lang;
            return (LdapAttribute)map[key.ToUpper()];
        }

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
        public override IEnumerator GetEnumerator()
        {
            return map.Values.GetEnumerator();
        }

        /// <summary>
        ///     Returns <code>true</code> if this set contains no elements
        /// </summary>
        /// <returns>
        ///     <code>true</code> if this set contains no elements
        /// </returns>
        public override bool IsEmpty()
        {
            return map.Count == 0;
        }

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
        {
            var attribute = (LdapAttribute)attr;
            return map.ContainsKey(attribute.Name.ToUpper());
        }

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
        public override bool Add(object attr)
        {
            var attribute = (LdapAttribute)attr;
            var name = attribute.Name.ToUpper();
            if (map.ContainsKey(name))
                return false;
            map[name] = attribute;
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
        public override bool Remove(object entry)
        {
            string attributeName;
            if (entry is string)
            {
                attributeName = (string)entry;
            }
            else
            {
                attributeName = ((LdapAttribute)entry).Name;
            }

            if ((object)attributeName == null)
            {
                return false;
            }

            var e = map[attributeName.ToUpper()];
            map.Remove(e);
            return e != null;
        }

        /// <summary>
        /// Removes all of the elements from this set.
        /// </summary>
        public override void Clear()
        {
            map.Clear();
        }

        /// <summary>
        ///     Adds all <code>LdapAttribute</code> objects in the specified collection to
        ///     this collection.
        /// </summary>
        /// <param name="c">
        ///     Collection of <code>LdapAttribute</code> objects.
        ///     @throws ClassCastException occurs when an element in the
        ///     collection is not of type <code>LdapAttribute</code>.
        /// </param>
        /// <returns>
        ///     true if this set changed as a result of the call.
        /// </returns>
        public override bool AddAll(ICollection c)
        {
            var setChanged = false;
            var i = c.GetEnumerator();

            while (i.MoveNext())
            {
                // we must enforce that everything in c is an LdapAttribute
                // add will return true if the attribute was added
                if (Add(i.Current))
                {
                    setChanged = true;
                }
            }

            return setChanged;
        }

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
#if !UWP

namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    /// <summary>
    ///     An implementation of LdapAuthHandler must be able to provide an
    ///     LdapAuthProvider object at the time of a referral.  The class
    ///     encapsulates information that is used by the client for authentication
    ///     when following referrals automatically.
    /// </summary>
    /// <seealso cref="LdapAuthHandler">
    /// </seealso>
    /// <seealso cref="LdapBindHandler">
    /// </seealso>
    internal class LdapAuthProvider
    {
        /// <summary>
        ///     Returns the distinguished name to be used for authentication on
        ///     automatic referral following.
        /// </summary>
        /// <returns>
        ///     The distinguished name from the object.
        /// </returns>
        public virtual string DN { get; }

        /// <summary>
        ///     Returns the password to be used for authentication on automatic
        ///     referral following.
        /// </summary>
        /// <returns>
        ///     The byte[] value (UTF-8) of the password from the object.
        /// </returns>
        public virtual sbyte[] Password => password;

        private readonly sbyte[] password;

        /// <summary>
        ///     Constructs information that is used by the client for authentication
        ///     when following referrals automatically.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name to use when authenticating to
        ///     a server.
        /// </param>
        /// <param name="password">
        ///     The password to use when authenticating to a server.
        /// </param>
        public LdapAuthProvider(string dn, sbyte[] password)
        {
            this.DN = dn;
            this.password = password;
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
        ///     Returns an enumerator for the values of the attribute in byte format.
        /// </summary>
        /// <returns>
        ///     The values of the attribute in byte format.
        ///     Note: All string values will be UTF-8 encoded. To decode use the
        ///     String constructor. Example: new String( byteArray, "UTF-8" );
        /// </returns>
        /*public virtual IEnumerator ByteValues
        {
            get { return ByteValueArray; }
        }
*/
        /// <summary>
        ///     Returns an enumerator for the string values of an attribute.
        /// </summary>
        /// <returns>
        ///     The string values of an attribute.
        /// </returns>
        /*
        public virtual IEnumerator StringValues
        {
            get { return new ArrayEnumeration(); }
        }
*/
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
                if (null == values)
                    return new sbyte[0][];
                var size = values.Length;
                var bva = new sbyte[size][];
                // Deep copy so application cannot change values
                for (int i = 0, u = size; i < u; i++)
                {
                    bva[i] = new sbyte[((sbyte[]) values[i]).Length];
                    Array.Copy((Array) values[i], 0, bva[i], 0, bva[i].Length);
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
                if (null == values)
                    return new string[0];
                var size = values.Length;
                var sva = new string[size];
                for (var j = 0; j < size; j++)
                {
                    try
                    {
                        var encoder = Encoding.UTF8;
                        var dchar = encoder.GetChars(((sbyte[]) values[j]).ToByteArray());
                        // char[] dchar = encoder.GetChars((byte[])values[j]);
                        sva[j] = new string(dchar);
                        // sva[j] = new String((sbyte[]) values[j], "UTF-8");
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
                        var encoder = Encoding.UTF8;
                        var dchar = encoder.GetChars(((sbyte[]) values[0]).ToByteArray());
                        // char[] dchar = encoder.GetChars((byte[]) this.values[0]);
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
                    bva = new sbyte[((sbyte[]) values[0]).Length];
                    Array.Copy((Array) values[0], 0, bva, 0, bva.Length);
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
                    add(sbytes);
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
        ///     Constructs an attribute with copies of all values of the input
        ///     attribute.
        /// </summary>
        /// <param name="attr">
        ///     An LdapAttribute to use as a template.
        ///     @throws IllegalArgumentException if attr is null
        /// </param>
        public LdapAttribute(LdapAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentException("LdapAttribute class cannot be null");
            }
            // Do a deep copy of the LdapAttribute template
            name = attr.name;
            baseName = attr.baseName;
            if (null != attr.subTypes)
            {
                subTypes = new string[attr.subTypes.Length];
                Array.Copy(attr.subTypes, 0, subTypes, 0, subTypes.Length);
            }
            // OK to just copy attributes, as the app only sees a deep copy of them
            if (null != attr.values)
            {
                values = new object[attr.values.Length];
                Array.Copy(attr.values, 0, values, 0, values.Length);
            }
        }

        /// <summary>
        ///     Constructs an attribute with no values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null
        /// </param>
        public LdapAttribute(string attrName)
        {
            if ((object) attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }
            name = attrName;
            baseName = getBaseName(attrName);
            subTypes = getSubtypes(attrName);
        }

        /// <summary>
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
        public LdapAttribute(string attrName, sbyte[] attrBytes) : this(attrName)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            // Make our own copy of the byte array to prevent app from changing it
            var tmp = new sbyte[attrBytes.Length];
            Array.Copy(attrBytes, 0, tmp, 0, attrBytes.Length);
            add(tmp);
        }

        /// <summary>
        ///     Constructs an attribute with a single string value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     @throws IllegalArgumentException if attrName or attrString is null
        /// </param>
        public LdapAttribute(string attrName, string attrString) : this(attrName)
        {
            if ((object) attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.UTF8;
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                add(sbytes);
            }
            catch (IOException e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
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
        public LdapAttribute(string attrName, string[] attrStrings) : this(attrName)
        {
            if (attrStrings == null)
            {
                throw new ArgumentException("Attribute values array cannot be null");
            }
            for (int i = 0, u = attrStrings.Length; i < u; i++)
            {
                try
                {
                    if ((object) attrStrings[i] == null)
                    {
                        throw new ArgumentException("Attribute value " + "at array index " + i + " cannot be null");
                    }
                    var encoder = Encoding.UTF8;
                    var ibytes = encoder.GetBytes(attrStrings[i]);
                    var sbytes = ibytes.ToSByteArray();
                    add(sbytes);
                    //					this.add(attrStrings[i].getBytes("UTF-8"));
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
                    Array.Copy(values, 0, ((LdapAttribute) newObj).values, 0, values.Length);
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
        public virtual void addValue(string attrString)
        {
            if ((object) attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.UTF8;
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                add(sbytes);
                //				this.add(attrString.getBytes("UTF-8"));
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
        public virtual void addValue(sbyte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(attrBytes);
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
        public virtual void addBase64Value(string attrString)
        {
            if ((object) attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(Convert.FromBase64String(attrString).ToSByteArray());
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
        public virtual void addBase64Value(StringBuilder attrString, int start, int end)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(Convert.FromBase64String(attrString.ToString(start, end)).ToSByteArray());
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
        public virtual void addBase64Value(char[] attrChars)
        {
            if (attrChars == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(Convert.FromBase64CharArray(attrChars, 0, attrChars.Length).ToSByteArray());
        }

        /// <summary>
        ///     Returns the base name of the attribute.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <returns>
        ///     The base name of the attribute.
        /// </returns>
        public virtual string getBaseName()
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
        public static string getBaseName(string attrName)
        {
            if ((object) attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }
            var idx = attrName.IndexOf(';');
            if (-1 == idx)
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
        public virtual string[] getSubtypes()
        {
            return subTypes;
        }

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
        public static string[] getSubtypes(string attrName)
        {
            if ((object) attrName == null)
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
        public virtual bool hasSubtype(string subtype)
        {
            if ((object) subtype == null)
            {
                throw new ArgumentException("subtype cannot be null");
            }
            if (null != subTypes)
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
        public virtual bool hasSubtypes(string[] subtypes)
        {
            if (subtypes == null)
            {
                throw new ArgumentException("subtypes cannot be null");
            }
            for (var i = 0; i < subtypes.Length; i++)
            {
                for (var j = 0; j < subTypes.Length; j++)
                {
                    if ((object) subTypes[j] == null)
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
        public virtual void removeValue(string attrString)
        {
            if (null == (object) attrString)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.UTF8;
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                removeValue(sbytes);
                //				this.removeValue(attrString.getBytes("UTF-8"));
            }
            catch (IOException uee)
            {
                // This should NEVER happen but just in case ...
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
        public virtual void removeValue(sbyte[] attrBytes)
        {
            if (null == attrBytes)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            for (var i = 0; i < values.Length; i++)
            {
                if (equals(attrBytes, (sbyte[]) values[i]))
                {
                    if (0 == i && 1 == values.Length)
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
        public virtual int size()
        {
            return null == values ? 0 : values.Length;
        }

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
            return name.CompareTo(((LdapAttribute) attribute).name);
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
        private void add(sbyte[] bytes)
        {
            if (null == values)
            {
                values = new object[] {bytes};
            }
            else
            {
                // Duplicate attribute values not allowed
                for (var i = 0; i < values.Length; i++)
                {
                    if (equals(bytes, (sbyte[]) values[i]))
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
        private bool equals(sbyte[] e1, sbyte[] e2)
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
        ///     Returns a string representation of this LdapAttribute
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapAttribute
        /// </returns>
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
                        if (((sbyte[]) values[i]).Length == 0)
                        {
                            continue;
                        }
                        var encoder = Encoding.UTF8;
                        //						char[] dchar = encoder.GetChars((byte[]) values[i]);
                        var dchar = encoder.GetChars(((sbyte[]) values[i]).ToByteArray());
                        var sval = new string(dchar);
                        //						System.String sval = new String((sbyte[]) values[i], "UTF-8");
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
        public override int Count
        {
            get { return map.Count; }
        }

        /// <summary>
        ///     This is the underlying data structure for this set.
        ///     HashSet is similar to the functionality of this set.  The difference
        ///     is we use the name of an attribute as keys in the Map and LdapAttributes
        ///     as the values.  We also do not declare the map as transient, making the
        ///     map serializable.
        /// </summary>
        private readonly Hashtable map;

        /// <summary> Constructs an empty set of attributes.</summary>
        public LdapAttributeSet()
        {
            map = new Hashtable();
        }

        // ---  methods not defined in Set ---
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
                    ((LdapAttributeSet) newObj).Add(((LdapAttribute) i.Current).Clone());
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
        public virtual LdapAttribute getAttribute(string attrName)
        {
            return (LdapAttribute) map[attrName.ToUpper()];
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
        public virtual LdapAttribute getAttribute(string attrName, string lang)
        {
            var key = attrName + ";" + lang;
            return (LdapAttribute) map[key.ToUpper()];
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
        public virtual LdapAttributeSet getSubset(string subtype)
        {
            // Create a new tempAttributeSet
            var tempAttributeSet = new LdapAttributeSet();
            var i = GetEnumerator();
            // Cycle throught this.attributeSet
            while (i.MoveNext())
            {
                var attr = (LdapAttribute) i.Current;
                // Does this attribute have the subtype we are looking for. If
                // yes then add it to our AttributeSet, else next attribute
                if (attr.hasSubtype(subtype))
                    tempAttributeSet.Add(attr.Clone());
            }
            return tempAttributeSet;
        }

        // --- methods defined in set ---
        /// <summary>
        ///     Returns an iterator over the attributes in this set.  The attributes
        ///     returned from this iterator are not in any particular order.
        /// </summary>
        /// <returns>
        ///     iterator over the attributes in this set
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
            var attribute = (LdapAttribute) attr;
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
            //We must enforce that attr is an LdapAttribute
            var attribute = (LdapAttribute) attr;
            var name = attribute.Name.ToUpper();
            if (map.ContainsKey(name))
                return false;
            map[name] = attribute;
            return true;
        }

        /// <summary>
        ///     Removes the specified object from this set if it is present.
        ///     If the specified object is of type <code>LdapAttribute</code>, the
        ///     specified attribute will be removed.  If the specified object is of type
        ///     <code>String</code>, the attribute with a name that matches the string will
        ///     be removed.
        /// </summary>
        /// <param name="object">
        ///     LdapAttribute to be removed or <code>String</code> naming
        ///     the attribute to be removed.
        /// </param>
        /// <returns>
        ///     true if the object was removed.
        ///     @throws ClassCastException occurs the specified Object
        ///     is not of type <code>LdapAttribute</code> or of type <code>String</code>.
        /// </returns>
        public override bool Remove(object object_Renamed)
        {
            string attributeName; //the name is the key to object in the HashMap
            if (object_Renamed is string)
            {
                attributeName = (string) object_Renamed;
            }
            else
            {
                attributeName = ((LdapAttribute) object_Renamed).Name;
            }
            if ((object) attributeName == null)
            {
                return false;
            }
            var e = map[attributeName.ToUpper()];
            map.Remove(e);
            return e != null;
        }

        /// <summary> Removes all of the elements from this set.</summary>
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
                var attr = (LdapAttribute) attrs.Current;
                retValue.Append(attr);
            }
            return retValue.ToString();
        }
    }

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
        public virtual string DN
        {
            get { return dn; }
        }

        protected internal string dn;
        protected internal LdapAttributeSet attrs;

        /// <summary> Constructs an empty entry.</summary>
        public LdapEntry() : this(null, null)
        {
        }

        /// <summary>
        ///     Constructs a new entry with the specified distinguished name and with
        ///     an empty attribute set.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry. The
        ///     value is not validated. An invalid distinguished
        ///     name will cause operations using this entry to fail.
        /// </param>
        public LdapEntry(string dn) : this(dn, null)
        {
        }

        /// <summary>
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
            if ((object) dn == null)
            {
                dn = "";
            }
            if (attrs == null)
            {
                attrs = new LdapAttributeSet();
            }

            this.dn = dn;
            this.attrs = attrs;
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
        public virtual LdapAttribute getAttribute(string attrName)
        {
            return attrs.getAttribute(attrName);
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
        public virtual LdapAttributeSet getAttributeSet()
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
        public virtual LdapAttributeSet getAttributeSet(string subtype)
        {
            return attrs.getSubset(subtype);
        }
    }

    /// <summary>
    ///     Encapsulates a single search result that is in response to an asynchronous
    ///     search operation.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    internal class LdapSearchResult : LdapMessage
    {
        /// <summary>
        ///     Returns the entry of a server's search response.
        /// </summary>
        /// <returns>
        ///     The LdapEntry associated with this LdapSearchResult
        /// </returns>
        public virtual LdapEntry Entry
        {
            get
            {
                if (entry == null)
                {
                    var attrs = new LdapAttributeSet();
                    var attrList = ((RfcSearchResultEntry) message.Response).Attributes;
                    var seqArray = attrList.ToArray();
                    for (var i = 0; i < seqArray.Length; i++)
                    {
                        var seq = (Asn1Sequence) seqArray[i];
                        var attr = new LdapAttribute(((Asn1OctetString) seq.Get(0)).StringValue());
                        var Set = (Asn1Set) seq.Get(1);
                        object[] setArray = Set.ToArray();
                        for (var j = 0; j < setArray.Length; j++)
                        {
                            attr.addValue(((Asn1OctetString) setArray[j]).ByteValue());
                        }
                        attrs.Add(attr);
                    }
                    entry = new LdapEntry(((RfcSearchResultEntry) message.Response).ObjectName.StringValue(), attrs);
                }
                return entry;
            }
        }

        private LdapEntry entry;

        /// <summary>
        /// Constructs an LdapSearchResult object.
        /// </summary>
        /// <param name="message">The RfcLdapMessage with a search result.</param>
        internal LdapSearchResult(RfcLdapMessage message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs an LdapSearchResult object from an LdapEntry.
        /// </summary>
        /// <param name="entry">the LdapEntry represented by this search result.</param>
        /// <exception cref="ArgumentException">Argument \"entry\" cannot be null</exception>
        public LdapSearchResult(LdapEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentException("Argument \"entry\" cannot be null");
            }

            this.entry = entry;
        }

        /// <summary>
        ///     Return a String representation of this object.
        /// </summary>
        /// <returns>
        ///     a String representing this object.
        /// </returns>
        public override string ToString()
        {
            return entry == null ? base.ToString() : entry.ToString();
        }
    }

    /// <summary>
    ///     Encapsulates an ID which uniquely identifies a particular extended
    ///     operation, known to a particular server, and the data associated
    ///     with that extended operation.
    /// </summary>
    /// <seealso cref="LdapConnection.ExtendedOperation">
    /// </seealso>
    internal class LdapExtendedOperation
    {
        private string oid;
        private sbyte[] vals;

        /// <summary>
        /// Constructs a new object with the specified object ID and data.
        /// </summary>
        /// <param name="oid">The unique identifier of the operation.</param>
        /// <param name="vals">The operation-specific data of the operation.</param>
        public LdapExtendedOperation(string oid, sbyte[] vals)
        {
            this.oid = oid;
            this.vals = vals;
        }

        /// <summary>
        ///     Returns a clone of this object.
        /// </summary>
        /// <returns>
        ///     clone of this object.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                //				Array.Copy((System.Array)SupportClass.ToByteArray( this.vals), 0, (System.Array)SupportClass.ToByteArray( ((LdapExtendedOperation) newObj).vals), 0, this.vals.Length);
                Array.Copy(vals, 0, ((LdapExtendedOperation) newObj).vals, 0, vals.Length);
                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }

        /// <summary>
        ///     Returns the unique identifier of the operation.
        /// </summary>
        /// <returns>
        ///     The OID (object ID) of the operation.
        /// </returns>
        public virtual string getID()
        {
            return oid;
        }

        /// <summary>
        ///     Returns a reference to the operation-specific data.
        /// </summary>
        /// <returns>
        ///     The operation-specific data.
        /// </returns>
        public virtual sbyte[] getValue()
        {
            return vals;
        }

        /// <summary>
        ///     Sets the value for the operation-specific data.
        /// </summary>
        /// <param name="newVals">
        ///     The byte array of operation-specific data.
        /// </param>
        protected internal virtual void setValue(sbyte[] newVals)
        {
            vals = newVals;
        }

        /// <summary>
        ///     Resets the OID for the operation to a new value
        /// </summary>
        /// <param name="newoid">
        ///     The new OID for the operation
        /// </param>
        protected internal virtual void setID(string newoid)
        {
            oid = newoid;
        }
    }

    /// <summary>
    ///     Represents an Ldap Search Result Reference.
    ///     <pre>
    ///         SearchResultReference ::= [APPLICATION 19] SEQUENCE OF LdapURL
    ///     </pre>
    /// </summary>
    internal class RfcSearchResultReference : Asn1SequenceOf
    {
        // Constructors for SearchResultReference
        /// <summary>
        /// The only time a client will create a SearchResultReference is when it is
        /// decoding it from an InputStream
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="in_Renamed">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcSearchResultReference(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
        }

        // Accessors
        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.SEARCH_RESULT_REFERENCE);
        }
    }

    /// <summary> Represnts an Ldap String.</summary>
    internal class RfcLdapString : Asn1OctetString
    {
        public RfcLdapString(string s) : base(s)
        {
        }

        public RfcLdapString(sbyte[] ba) : base(ba)
        {
        }

        public RfcLdapString(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
        }
    }

    /// <summary>
    ///     Represents an Ldap DN.
    ///     <pre>
    ///         LdapDN ::= LdapString
    ///     </pre>
    /// </summary>
    internal class RfcLdapDN : RfcLdapString
    {
        // Constructors for RfcLdapDN
        public RfcLdapDN(string s)
            : base(s)
        {
        }

        public RfcLdapDN(sbyte[] s)
            : base(s)
        {
        }
    }

    /// <summary>
    ///     Represents an Ldap Extended Response.
    ///     <pre>
    ///         ExtendedResponse ::= [APPLICATION 24] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         responseName     [10] LdapOID OPTIONAL,
    ///         response         [11] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    internal class RfcExtendedResponse : Asn1Sequence, RfcResponse
    {
        public virtual RfcLdapOID ResponseName
        {
            get { return responseNameIndex != 0 ? (RfcLdapOID) Get(responseNameIndex) : null; }
        }

        public virtual Asn1OctetString Response
        {
            get { return responseIndex != 0 ? (Asn1OctetString) Get(responseIndex) : null; }
        }

        /// <summary> Context-specific TAG for optional responseName.</summary>
        public const int RESPONSE_NAME = 10;

        /// <summary> Context-specific TAG for optional response.</summary>
        public const int RESPONSE = 11;

        private readonly int referralIndex;
        private readonly int responseNameIndex;
        private readonly int responseIndex;

        /// <summary>
        /// The only time a client will create a ExtendedResponse is when it is
        /// decoding it from an InputStream
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="in_Renamed">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcExtendedResponse(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            // decode optional tagged elements
            if (Size() > 3)
            {
                for (var i = 3; i < Size(); i++)
                {
                    var obj = (Asn1Tagged) Get(i);
                    var id = obj.GetIdentifier();
                    switch (id.Tag)
                    {
                        case RfcLdapResult.REFERRAL:
                            var content = ((Asn1OctetString) obj.taggedValue()).ByteValue();
                            var bais = new MemoryStream(content.ToByteArray());
                            Set(i, new Asn1SequenceOf(dec, bais, content.Length));
                            referralIndex = i;
                            break;
                        case RESPONSE_NAME:
                            Set(i, new RfcLdapOID(((Asn1OctetString) obj.taggedValue()).ByteValue()));
                            responseNameIndex = i;
                            break;
                        case RESPONSE:
                            Set(i, obj.taggedValue());
                            responseIndex = i;
                            break;
                    }
                }
            }
        }

        // Accessors
        public Asn1Enumerated getResultCode()
        {
            return (Asn1Enumerated) Get(0);
        }

        public RfcLdapDN getMatchedDN()
        {
            return new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue());
        }

        public RfcLdapString getErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue());
        }

        public Asn1SequenceOf getReferral()
        {
            return referralIndex != 0 ? (Asn1SequenceOf) Get(referralIndex) : null;
        }

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.EXTENDED_RESPONSE);
        }
    }

    /// <summary>
    ///     Represents and Ldap Bind Response.
    ///     <pre>
    ///         BindResponse ::= [APPLICATION 1] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         serverSaslCreds    [7] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    internal class RfcBindResponse : Asn1Sequence, RfcResponse
    {
        /// <summary>
        ///     Returns the OPTIONAL serverSaslCreds of a BindResponse if it exists
        ///     otherwise null.
        /// </summary>
        public virtual Asn1OctetString ServerSaslCreds
        {
            get
            {
                if (Size() == 5)
                    return (Asn1OctetString) ((Asn1Tagged) Get(4)).taggedValue();
                if (Size() == 4)
                {
                    // could be referral or serverSaslCreds
                    var obj = Get(3);
                    if (obj is Asn1Tagged)
                        return (Asn1OctetString) ((Asn1Tagged) obj).taggedValue();
                }
                return null;
            }
        }

        // Constructors for BindResponse
        /// <summary>
        /// The only time a client will create a BindResponse is when it is
        /// decoding it from an InputStream
        /// Note: If serverSaslCreds is included in the BindResponse, it does not
        /// need to be decoded since it is already an OCTET STRING.
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="in_Renamed">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcBindResponse(Asn1Decoder dec, Stream in_Renamed, int len)
            : base(dec, in_Renamed, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Size() > 3)
            {
                var obj = (Asn1Tagged) Get(3);
                var id = obj.GetIdentifier();
                if (id.Tag == RfcLdapResult.REFERRAL)
                {
                    var content = ((Asn1OctetString) obj.taggedValue()).ByteValue();
                    var bais = new MemoryStream(content.ToByteArray());
                    Set(3, new Asn1SequenceOf(dec, bais, content.Length));
                }
            }
        }

        // Accessors
        public Asn1Enumerated getResultCode()
        {
            return (Asn1Enumerated) Get(0);
        }

        public RfcLdapDN getMatchedDN()
        {
            return new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue());
        }

        public RfcLdapString getErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue());
        }

        public Asn1SequenceOf getReferral()
        {
            if (Size() > 3)
            {
                var obj = Get(3);
                if (obj is Asn1SequenceOf)
                    return (Asn1SequenceOf) obj;
            }
            return null;
        }

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.BIND_RESPONSE);
        }
    }

    /// <summary>
    ///     Represents an LDAP Intermediate Response.
    ///     IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
    ///     COMPONENTS OF LDAPResult, note: only present on incorrectly
    ///     encoded response from
    ///     pre Falcon-sp1 server
    ///     responseName     [10] LDAPOID OPTIONAL,
    ///     responseValue    [11] OCTET STRING OPTIONAL }
    /// </summary>
    internal class RfcIntermediateResponse : Asn1Sequence, RfcResponse
    {
        /**
         * Context-specific TAG for optional responseName.
         */
        public const int TAG_RESPONSE_NAME = 0;
        /**
         * Context-specific TAG for optional response.
         */
        public const int TAG_RESPONSE = 1;
        private int m_referralIndex;
        private readonly int m_responseNameIndex;
        private readonly int m_responseValueIndex;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcIntermediateResponse"/> class.
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="in_Renamed">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcIntermediateResponse(Asn1Decoder dec, Stream in_Renamed, int len)
            : base(dec, in_Renamed, len)
        {
            //		super(dec, in, len);
            var i = 0;
            m_responseNameIndex = m_responseValueIndex = 0;
            // decode optional tagged elements. The parent class constructor will
            // have decoded these elements as ASN1Tagged objects with the value
            // stored as an ASN1OctectString object.
            if (Size() >= 3) //the incorrectly encoded case, LDAPResult contains 
                i = 3; //at least 3 components
            else
                i = 0; //correctly encoded case, can have zero components
            for (; i < Size(); i++)
            {
                var obj = (Asn1Tagged) Get(i);
                var id = obj.GetIdentifier();
                switch (id.Tag)
                {
                    case TAG_RESPONSE_NAME:
                        Set(i, new RfcLdapOID(
                            ((Asn1OctetString) obj.taggedValue()).ByteValue()));
                        m_responseNameIndex = i;
                        break;
                    case TAG_RESPONSE:
                        Set(i, obj.taggedValue());
                        m_responseValueIndex = i;
                        break;
                }
            }
        }

        public Asn1Enumerated getResultCode()
        {
            if (Size() > 3)
                return (Asn1Enumerated) Get(0);
            return null;
        }

        public RfcLdapDN getMatchedDN()
        {
            if (Size() > 3)
                return new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue());
            return null;
        }

        public RfcLdapString getErrorMessage()
        {
            if (Size() > 3)
                return new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue());
            return null;
        }

        public Asn1SequenceOf getReferral()
        {
            return Size() > 3 ? (Asn1SequenceOf) Get(3) : null;
        }

        public RfcLdapOID getResponseName()
        {
            return m_responseNameIndex >= 0
                ? (RfcLdapOID) Get(m_responseNameIndex)
                : null;
        }

        public Asn1OctetString getResponse()
        {
            return m_responseValueIndex != 0
                ? (Asn1OctetString) Get(m_responseValueIndex)
                : null;
        }

        /// <summary>
        /// Returns the identifier for this Asn1Object as an Asn1Identifier.
        /// This Asn1Identifier object will include the CLASS, FORM and TAG
        /// for this Asn1Object.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true,
                LdapMessage.INTERMEDIATE_RESPONSE);
        }
    }
}

#endif
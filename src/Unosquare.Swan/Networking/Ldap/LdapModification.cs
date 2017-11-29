#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    /// Ldap Modification Operators
    /// </summary>
    public enum LdapModificationOp
    {
        /// <summary>
        /// Adds the listed values to the given attribute, creating
        /// the attribute if it does not already exist.
        /// ADD = 0
        /// </summary>
        Add = 0,

        /// <summary>
        /// Deletes the listed values from the given attribute,
        /// removing the entire attribute (1) if no values are listed or
        /// (2) if all current values of the attribute are listed for
        /// deletion.
        /// DELETE = 1
        /// </summary>
        Delete = 1,

        /// <summary>
        /// Replaces all existing values of the given attribute
        /// with the new values listed, creating the attribute if it
        /// does not already exist.
        /// A replace with no value deletes the entire attribute if it
        /// exists, and is ignored if the attribute does not exist.
        /// REPLACE = 2
        /// </summary>
        Replace = 2
    }

    /// <summary>
    /// A single add, delete, or replace operation to an LdapAttribute.
    /// An LdapModification contains information on the type of modification
    /// being performed, the name of the attribute to be replaced, and the new
    /// value.  Multiple modifications are expressed as an array of modifications,
    /// i.e., <c>LdapModification[]</c>.
    /// An LdapModification or an LdapModification array enable you to modify
    /// an attribute of an Ldap entry. The entire array of modifications must
    /// be performed by the server as a single atomic operation in the order they
    /// are listed. No changes are made to the directory unless all the operations
    /// succeed. If all succeed, a success result is returned to the application.
    /// It should be noted that if the connection fails during a modification,
    /// it is indeterminate whether the modification occurred or not.
    /// There are three types of modification operations: Add, Delete,
    /// and Replace.
    /// <b>Add: </b>Creates the attribute if it doesn't exist, and adds
    /// the specified values. This operation must contain at least one value, and
    /// all values of the attribute must be unique.
    /// <b>Delete: </b>Deletes specified values from the attribute. If no
    /// values are specified, or if all existing values of the attribute are
    /// specified, the attribute is removed. Mandatory attributes cannot be
    /// removed.
    /// <b>Replace: </b>Creates the attribute if necessary, and replaces
    /// all existing values of the attribute with the specified values.
    /// If you wish to keep any existing values of a multi-valued attribute,
    /// you must include these values in the replace operation.
    /// A replace operation with no value will remove the entire attribute if it
    /// exists, and is ignored if the attribute does not exist.
    /// Additional information on Ldap modifications is available in section 4.6
    /// of <a href="http://www.ietf.org/rfc/rfc2251.txt">rfc2251.txt</a>
    /// </summary>
    /// <seealso cref="LdapConnection.Modify"></seealso>
    /// <seealso cref="LdapAttribute"></seealso>
    public sealed class LdapModification : LdapMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapModification" /> class.
        /// Specifies a modification to be made to an attribute.
        /// </summary>
        /// <param name="op">The type of modification to make, which can be
        /// one of the following:
        /// <ul><li>
        /// LdapModification.ADD - The value should be added to
        /// the attribute
        /// </li><li>
        /// LdapModification.DELETE - The value should be removed
        /// from the attribute
        /// </li><li>
        /// LdapModification.REPLACE - The value should replace all
        /// existing values of the
        /// attribute
        /// </li></ul></param>
        /// <param name="attr">The attribute to modify.</param>
        public LdapModification(LdapModificationOp op, LdapAttribute attr)
        {
            Op = op;
            Attribute = attr;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapModification"/> class.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <param name="attrValue">The attribute value.</param>
        public LdapModification(LdapModificationOp op, string attrName, string attrValue)
            : this(op, new LdapAttribute(attrName, attrValue))
        {
            // placeholder
        }

        /// <summary>
        /// Returns the attribute to modify, with any existing values.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public LdapAttribute Attribute { get; }

        /// <summary>
        /// Returns the type of modification specified by this object.
        /// The type is one of the following:
        /// <ul><li>LdapModification.ADD</li><li>LdapModification.DELETE</li><li>LdapModification.REPLACE</li></ul>
        /// </summary>
        /// <value>
        /// The op.
        /// </value>
        public LdapModificationOp Op { get; }
    }
}
#endif
#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    /// Ldap Modification Operators.
    /// </summary>
    public enum LdapModificationOp
    {
        /// <summary>
        /// Adds the listed values to the given attribute, creating
        /// the attribute if it does not already exist.
        /// </summary>
        Add = 0,

        /// <summary>
        /// Deletes the listed values from the given attribute,
        /// removing the entire attribute (1) if no values are listed or
        /// (2) if all current values of the attribute are listed for
        /// deletion.
        /// </summary>
        Delete = 1,

        /// <summary>
        /// Replaces all existing values of the given attribute
        /// with the new values listed, creating the attribute if it
        /// does not already exist.
        /// A replace with no value deletes the entire attribute if it
        /// exists, and is ignored if the attribute does not exist.
        /// </summary>
        Replace = 2,
    }

    /// <summary>
    /// LDAP valid scopes.
    /// </summary>
    public enum LdapScope
    {
        /// <summary>
        /// Used with search to specify that the scope of entrys to search is to
        /// search only the base object.
        /// </summary>
        ScopeBase = 0,

        /// <summary>
        /// Used with search to specify that the scope of entrys to search is to
        /// search only the immediate subordinates of the base object.
        /// </summary>
        ScopeOne = 1,

        /// <summary>
        /// Used with search to specify that the scope of entrys to search is to
        /// search the base object and all entries within its subtree.
        /// </summary>
        ScopeSub = 2,
    }
    
    /// <summary>
    /// Substring Operators.
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
        Final = 2,
    }

    /// <summary>
    /// Filtering Operators.
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
        ExtensibleMatch = 9,
    }
}
#endif
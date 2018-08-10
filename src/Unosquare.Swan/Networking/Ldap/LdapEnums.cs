namespace Unosquare.Swan.Networking.Ldap
{
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
        ScopeSub = 2
    }
}

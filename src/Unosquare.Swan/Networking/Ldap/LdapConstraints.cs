#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    
    /// <summary>
    ///     Defines options controlling Ldap operations on the directory.
    ///     An LdapConstraints object is always associated with an LdapConnection
    ///     object; its values can be changed with LdapConnection.setConstraints, or
    ///     overridden by passing an LdapConstraints object to an operation.
    /// </summary>
    /// <seealso cref="LdapConnection.Constraints">
    /// </seealso>
    public class LdapConstraints
    {
        private int _msLimit;
        private int _hopLimit = 10;
        private bool _doReferrals;
        private LdapControl[] _controls;
        private Hashtable _properties; // Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapConstraints"/> class.
        /// Constructs a new LdapConstraints object that specifies the default
        /// set of constraints.
        /// </summary>
        public LdapConstraints()
        {
            // Get a unique constraints name for debug
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapConstraints" /> class.
        /// Constructs a new LdapConstraints object specifying constraints that
        /// control wait time, and referral handling.
        /// </summary>
        /// <param name="msLimit">The maximum time in milliseconds to wait for results.
        /// The default is 0, which means that there is no
        /// maximum time limit. This limit is enforced for an
        /// operation by the API, not by the server.
        /// The operation will be abandoned and terminated by the
        /// API with a result code of LdapException.Ldap_TIMEOUT
        /// if the operation exceeds the time limit.</param>
        /// <param name="doReferrals">Determines whether to automatically follow
        /// referrals or not. Specify true to follow
        /// referrals automatically, and false to throw
        /// an LdapReferralException if the server responds
        /// with a referral. False is the default value.
        /// The way referrals are followed automatically is
        /// determined by the setting of the handler parameter.
        /// It is ignored for asynchronous operations.</param>
        /// <param name="hopLimit">The maximum number of referrals to follow in a
        /// sequence during automatic referral following.
        /// The default value is 10. A value of 0 means no limit.
        /// The operation will be abandoned and terminated by the
        /// API with a result code of
        /// LdapException.REFERRAL_LIMIT_EXCEEDED if the
        /// number of referrals in a sequence exceeds the limit.
        /// It is ignored for asynchronous operations.</param>
        /// <seealso cref="LdapReferralException"></seealso>
        public LdapConstraints(int msLimit, bool doReferrals, int hopLimit)
        {
            _msLimit = msLimit;
            _doReferrals = doReferrals;
            _hopLimit = hopLimit;
        }

        /// <summary>
        /// Returns the maximum number of referrals to follow during automatic
        /// referral following.  The operation will be abandoned and terminated by
        /// the API with a result code of LdapException.REFERRAL_LIMIT_EXCEEDED
        /// if the number of referrals in a sequence exceeds the limit.
        /// It is ignored for asynchronous operations.
        /// </summary>
        /// <value>
        /// The hop limit.
        /// The maximum number of referrals to follow in a
        /// sequence during automatic referral following.
        /// The default value is 10. A value of 0 means no limit.
        /// The operation will be abandoned and terminated by the
        /// API with a result code of
        /// LdapException.REFERRAL_LIMIT_EXCEEDED if the
        /// number of referrals in a sequence exceeds the limit.
        /// It is ignored for asynchronous operations.
        /// </value>
        public virtual int HopLimit
        {
            get => _hopLimit;

            set => _hopLimit = value;
        }

        /// <summary>
        /// Specified whether or not referrals are followed automatically.
        /// </summary>
        /// <value>
        /// True to follow referrals automatically.
        /// False to throw an LdapReferralException if
        /// the server returns a referral.
        /// </value>
        public virtual bool ReferralFollowing
        {
            get => _doReferrals;

            set => _doReferrals = value;
        }

        /// <summary>
        /// Returns the maximum number of milliseconds to wait for any operation
        /// under these constraints.
        /// If the value is 0, there is no maximum time limit on waiting
        /// for operation results. The actual granularity of the timeout depends
        /// platform.  This limit is enforced the the API on an
        /// operation, not by the server.
        /// The operation will be abandoned and terminated by the
        /// API with a result code of LdapException.Ldap_TIMEOUT if the
        /// operation exceeds the time limit.
        /// </summary>
        /// <value>
        /// The time limit.
        /// </value>
        /// <seealso cref="LdapException.Ldap_TIMEOUT"></seealso>
        public virtual int TimeLimit
        {
            get => _msLimit;

            set => _msLimit = value;
        }

        /// <summary>
        /// Gets all the properties of the constraints object which has been
        /// assigned with {setProperty(String, Object)}.
        /// A value of <code>null</code> is returned if no properties are defined.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        internal virtual Hashtable Properties
        {
            get => _properties;

            set => _properties = (Hashtable)value.Clone();
        }

        /// <summary>
        /// Returns the controls to be sent to the server.
        /// </summary>
        /// <returns>
        /// The controls to be sent to the server, or null if none.
        /// </returns>
        public virtual LdapControl[] GetControls() => _controls;

        /// <summary>
        /// Gets a property of the constraints object which has been
        /// assigned with {setProperty(String, Object)}.
        /// </summary>
        /// <param name="name">Name of the property to be returned.</param>
        /// <returns>
        /// the object associated with the property,
        /// or <code>null</code> if the property is not set.
        /// </returns>
        /// <seealso cref="object"></seealso>
        /// <seealso cref="LdapConnection.GetProperty(string)"></seealso>
        public virtual object GetProperty(string name) => _properties?[name];

        /// <summary>
        /// Sets a single control to be sent to the server.
        /// </summary>
        /// <param name="control">A single control to be sent to the server or
        /// null if none.</param>
        public virtual void SetControls(LdapControl control)
        {
            if (control == null)
            {
                _controls = null;
                return;
            }

            _controls = new LdapControl[1];
            _controls[0] = (LdapControl) control.Clone();
        }

        /// <summary>
        /// Sets controls to be sent to the server.
        /// </summary>
        /// <param name="controls">An array of controls to be sent to the server or
        /// null if none.</param>
        public virtual void SetControls(LdapControl[] controls)
        {
            if (controls == null || controls.Length == 0)
            {
                _controls = null;
                return;
            }

            _controls = new LdapControl[controls.Length];

            for (var i = 0; i < controls.Length; i++)
            {
                _controls[i] = (LdapControl) controls[i].Clone();
            }
        }

        /// <summary>
        /// Sets a property of the constraints object.
        /// No property names have been defined at this time, but the
        /// mechanism is in place in order to support revisional as well as
        /// dynamic and proprietary extensions to operation modifiers.
        /// </summary>
        /// <param name="name">Name of the property to set.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <seealso cref="LdapConnection.GetProperty"></seealso>
        public virtual void SetProperty(string name, object propertyValue)
        {
            if (_properties == null)
            {
                _properties = new Hashtable();
            }

            _properties[name] = propertyValue;
        }
        
        /// <summary>
        ///     Clones an LdapConstraints object.
        /// </summary>
        /// <returns>
        ///     An LdapConstraints object.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                if (_controls != null)
                {
                    ((LdapConstraints)newObj)._controls = new LdapControl[_controls.Length];
                    _controls.CopyTo(((LdapConstraints)newObj)._controls, 0);
                }

                if (_properties != null)
                {
                    ((LdapConstraints)newObj)._properties = (Hashtable)_properties.Clone();
                }

                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }
    }
}

#endif
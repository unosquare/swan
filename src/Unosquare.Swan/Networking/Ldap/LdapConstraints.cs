#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;

    /// <summary>
    /// Shared ancestor to the two types of referral objects - LdapBindHandler and
    /// LdapAuthHandler.
    /// </summary>
    /// <seealso cref="LdapBindHandler"></seealso>
    /// <seealso cref="LdapAuthHandler"></seealso>
    public interface ILdapReferralHandler
    {
    }

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
        static LdapConstraints()
        {
            new object();
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
        /// </value>
        /// <seealso cref="HopLimit"></seealso>
        /// <seealso cref="LdapException.REFERRAL_LIMIT_EXCEEDED"></seealso>
        /// <param name="hop_limit">
        /// The maximum number of referrals to follow in a
        /// sequence during automatic referral following.
        /// The default value is 10. A value of 0 means no limit.
        /// The operation will be abandoned and terminated by the
        /// API with a result code of
        /// LdapException.REFERRAL_LIMIT_EXCEEDED if the
        /// number of referrals in a sequence exceeds the limit.
        /// It is ignored for asynchronous operations.
        /// </param>
        /// <seealso cref="LdapException.REFERRAL_LIMIT_EXCEEDED"></seealso>
        public virtual int HopLimit
        {
            get => hopLimit;

            set => hopLimit = value;
        }

        /// <summary>
        /// Gets all the properties of the constraints object which has been
        /// assigned with {@link #setProperty(String, Object)}.
        /// A value of <code>null</code> is returned if no properties are defined.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        /// <seealso cref="object"></seealso>
        /// <seealso cref="LdapConnection.GetProperty"></seealso>
        /// <param name="props">
        /// the properties represented by the Hashtable object to set.
        /// </param>
        internal virtual Hashtable Properties
        {
            get => properties;

            set => properties = (Hashtable) value.Clone();
        }

        /// <summary>
        /// Specified whether or not referrals are followed automatically.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [referral following]; otherwise, <c>false</c>.
        /// </value>
        /// <param name="doReferrals">
        /// True to follow referrals automatically.
        /// False to throw an LdapReferralException if
        /// the server returns a referral.
        /// </param>
        public virtual bool ReferralFollowing
        {
            get => doReferrals;

            set => doReferrals = value;
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
            get => msLimit;

            set => msLimit = value;
        }

        private int msLimit;
        private int hopLimit = 10;
        private bool doReferrals;
        private ILdapReferralHandler refHandler;
        private LdapControl[] controls;
        private Hashtable properties; // Properties

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
        /// Initializes a new instance of the <see cref="LdapConstraints"/> class.
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
        /// <param name="handler">The custom authentication handler called when
        /// LdapConnection needs to authenticate, typically on
        /// following a referral.  A null may be specified to
        /// indicate default authentication processing, i.e.
        /// referrals are followed with anonymous authentication.
        /// The handler object may be an implemention of either the
        /// LdapBindHandler or LdapAuthHandler interface.
        /// The implementation of these interfaces determines how
        /// authentication is performed when following referrals.
        /// It is ignored for asynchronous operations.</param>
        /// <param name="hopLimit">The maximum number of referrals to follow in a
        /// sequence during automatic referral following.
        /// The default value is 10. A value of 0 means no limit.
        /// The operation will be abandoned and terminated by the
        /// API with a result code of
        /// LdapException.REFERRAL_LIMIT_EXCEEDED if the
        /// number of referrals in a sequence exceeds the limit.
        /// It is ignored for asynchronous operations.</param>
        /// <seealso cref="LdapException.Ldap_TIMEOUT"></seealso>
        /// <seealso cref="LdapException.REFERRAL_LIMIT_EXCEEDED"></seealso>
        /// <seealso cref="LdapException.REFERRAL"></seealso>
        /// <seealso cref="LdapReferralException"></seealso>
        public LdapConstraints(int msLimit, bool doReferrals, ILdapReferralHandler handler, int hopLimit)
        {
            this.msLimit = msLimit;
            this.doReferrals = doReferrals;
            refHandler = handler;
            this.hopLimit = hopLimit;
        }

        /// <summary>
        /// Returns the controls to be sent to the server.
        /// </summary>
        /// <returns>
        /// The controls to be sent to the server, or null if none.
        /// </returns>
        public virtual LdapControl[] GetControls()
        {
            return controls;
        }

        /// <summary>
        /// Gets a property of the constraints object which has been
        /// assigned with {@link #setProperty(String, Object)}.
        /// </summary>
        /// <param name="name">Name of the property to be returned.</param>
        /// <returns>
        /// the object associated with the property,
        /// or <code>null</code> if the property is not set.
        /// </returns>
        /// <seealso cref="object"></seealso>
        /// <seealso cref="LdapConnection.GetProperty(string)"></seealso>
        public virtual object GetProperty(string name)
        {
            return properties?[name];
        }

        /// <summary>
        /// Returns an object that can process authentication for automatic
        /// referral handling.
        /// It may be null.
        /// </summary>
        /// <returns>
        /// An LdapReferralHandler object that can process authentication.
        /// </returns>
        internal virtual ILdapReferralHandler GetReferralHandler() => refHandler;

        /// <summary>
        /// Sets a single control to be sent to the server.
        /// </summary>
        /// <param name="control">A single control to be sent to the server or
        /// null if none.</param>
        public virtual void SetControls(LdapControl control)
        {
            if (control == null)
            {
                controls = null;
                return;
            }

            controls = new LdapControl[1];
            controls[0] = (LdapControl) control.Clone();
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
                this.controls = null;
                return;
            }

            this.controls = new LdapControl[controls.Length];
            for (var i = 0; i < controls.Length; i++)
            {
                this.controls[i] = (LdapControl) controls[i].Clone();
            }
        }

        /// <summary>
        /// Sets a property of the constraints object.
        /// No property names have been defined at this time, but the
        /// mechanism is in place in order to support revisional as well as
        /// dynamic and proprietary extensions to operation modifiers.
        /// </summary>
        /// <param name="name">Name of the property to set.</param>
        /// <param name="value_Renamed">The value renamed.</param>
        /// <seealso cref="LdapConnection.GetProperty"></seealso>
        public virtual void SetProperty(string name, object value_Renamed)
        {
            if (properties == null)
            {
                properties = new Hashtable();
            }

            properties[name] = value_Renamed;
        }

        /// <summary>
        /// Specifies the object that will process authentication requests
        /// during automatic referral following.
        /// The default is null.
        /// </summary>
        /// <param name="handler">An object that implements LdapBindHandler or
        /// LdapAuthHandler</param>
        public virtual void SetReferralHandler(ILdapReferralHandler handler)
        {
            refHandler = handler;
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
                if (controls != null)
                {
                    ((LdapConstraints)newObj).controls = new LdapControl[controls.Length];
                    controls.CopyTo(((LdapConstraints)newObj).controls, 0);
                }

                if (properties != null)
                {
                    ((LdapConstraints)newObj).properties = (Hashtable)properties.Clone();
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
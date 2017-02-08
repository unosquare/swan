using System;

namespace Unosquare.Swan.AspNetCore
{
    /// <summary>
    /// defines a combination of actions in a CRUD pattern
    /// </summary>
    [Flags]
    public enum ActionFlags
    {
        /// <summary>
        /// None action
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Create action
        /// </summary>
        Create = 0x1,
        /// <summary>
        /// Update action
        /// </summary>
        Update = 0x2,
        /// <summary>
        /// Delete Action
        /// </summary>
        Delete = 0x3
    }

    /// <summary>
    /// Decorate methods with this attribute to execute business rules that match the following signature:
    /// public void MethodName(T entity)
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BusinessRuleAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the entity types.
        /// </summary>
        /// <value>
        /// The entity types.
        /// </value>
        public Type[] EntityTypes { get; protected set; }
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public ActionFlags Action { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleAttribute"/> class.
        /// </summary>
        /// <param name="actionFlags">The action flags.</param>
        public BusinessRuleAttribute(ActionFlags actionFlags)
        {
            this.Action = actionFlags;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleAttribute"/> class.
        /// </summary>
        /// <param name="entityTypes">The entity types.</param>
        /// <param name="actionFlags">The action flags.</param>
        public BusinessRuleAttribute(Type[] entityTypes, ActionFlags actionFlags)
        {
            this.EntityTypes = entityTypes;
            this.Action = actionFlags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleAttribute"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="actionFlags">The action flags.</param>
        public BusinessRuleAttribute(Type entityType, ActionFlags actionFlags) 
            : this(new[] { entityType}, actionFlags)
        {
        }
    }
}

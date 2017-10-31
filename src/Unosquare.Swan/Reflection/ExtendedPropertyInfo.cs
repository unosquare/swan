namespace Unosquare.Swan.Reflection
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Attributes;

    /// <summary>
    /// Represents a Property object from a Object Reflection Property with extended values
    /// </summary>
    public class ExtendedPropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedPropertyInfo"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        public ExtendedPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            Property = propertyInfo.Name;
            DataType = propertyInfo.PropertyType.Name;

            foreach (var display in propertyInfo.GetCustomAttributes(true).OfType<PropertyDisplayAttribute>())
            {
                Name = display.Name;
                Description = display.Description;
                GroupName = display.GroupName;
                DefaultValue = display.DefaultValue;
            }
        }

        /// <summary>
        /// Gets or sets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property { get; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public string DataType { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object DefaultValue { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get;  }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; }
    }

    /// <summary>
    /// Represents a Property object from a Object Reflection Property with extended values
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    public class ExtendedPropertyInfo<T> : ExtendedPropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedPropertyInfo{T}"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        public ExtendedPropertyInfo(string property) 
            : base(typeof(T).GetProperty(property))
        {
        }
    }
}
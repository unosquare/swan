﻿namespace Unosquare.Swan
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a Property object from a Object Reflection Property with extended values
    /// </summary>
    public class PropertyDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDto{T}"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        public PropertyDto(PropertyInfo propertyInfo)
        {
            Property = propertyInfo.Name;

            DataType = propertyInfo.PropertyType.Name;

            foreach (var display in propertyInfo.GetCustomAttributes(true).OfType<DisplayAttribute>())
            {
                Name = display.Name;
                Description = display.Description;
                GroupName = display.GroupName;
            }

            foreach (var defaultValue in propertyInfo.GetCustomAttributes(true).OfType<DefaultValueAttribute>())
            {
                DefaultValue = defaultValue.Value;
            }
        }

        /// <summary>
        /// Gets or sets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public string DataType { get; set; }

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
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; set; }
    }

    /// <summary>
    /// Represents a Property object from a Object Reflection Property with extended values
    /// </summary>
    public class PropertyDto<T> : PropertyDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDto{T}"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        public PropertyDto(string property) : base(typeof(T).GetTypeInfo().GetProperty(property))
        {
        }
    }
}
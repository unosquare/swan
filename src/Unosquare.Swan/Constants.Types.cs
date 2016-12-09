namespace Unosquare.Swan
{
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    partial class Constants
    {
        #region Main Dictionary Definition

        static private readonly Dictionary<Type, ExtendedTypeInfo> InternalBasicTypeInfo =
            new Dictionary<Type, ExtendedTypeInfo> {
                // Non-Nullables
                { typeof(DateTime), new ExtendedTypeInfo(typeof(DateTime)) },
                { typeof(byte), new ExtendedTypeInfo(typeof(byte)) },
                { typeof(sbyte), new ExtendedTypeInfo(typeof(sbyte)) },
                { typeof(int), new ExtendedTypeInfo(typeof(int)) },
                { typeof(uint), new ExtendedTypeInfo(typeof(uint)) },
                { typeof(short), new ExtendedTypeInfo(typeof(short)) },
                { typeof(ushort), new ExtendedTypeInfo(typeof(ushort)) },
                { typeof(long), new ExtendedTypeInfo(typeof(long)) },
                { typeof(ulong), new ExtendedTypeInfo(typeof(ulong)) },
                { typeof(float), new ExtendedTypeInfo(typeof(float)) },
                { typeof(double), new ExtendedTypeInfo(typeof(double)) },
                { typeof(char), new ExtendedTypeInfo(typeof(char)) },
                { typeof(bool), new ExtendedTypeInfo(typeof(bool)) },
                { typeof(decimal), new ExtendedTypeInfo(typeof(decimal)) },
                // String is also considered a primitive type
                { typeof(string), new ExtendedTypeInfo(typeof(string)) },
                // Nullables
                { typeof(DateTime?), new ExtendedTypeInfo(typeof(DateTime?)) },
                { typeof(byte?), new ExtendedTypeInfo(typeof(byte?)) },
                { typeof(sbyte?), new ExtendedTypeInfo(typeof(sbyte?)) },
                { typeof(int?), new ExtendedTypeInfo(typeof(int?)) },
                { typeof(uint?), new ExtendedTypeInfo(typeof(uint?)) },
                { typeof(short?), new ExtendedTypeInfo(typeof(short?)) },
                { typeof(ushort?), new ExtendedTypeInfo(typeof(ushort?)) },
                { typeof(long?), new ExtendedTypeInfo(typeof(long?)) },
                { typeof(ulong?), new ExtendedTypeInfo(typeof(ulong?)) },
                { typeof(float?), new ExtendedTypeInfo(typeof(float?)) },
                { typeof(double?), new ExtendedTypeInfo(typeof(double?)) },
                { typeof(char?), new ExtendedTypeInfo(typeof(char?)) },
                { typeof(bool?), new ExtendedTypeInfo(typeof(bool?)) },
                { typeof(decimal?), new ExtendedTypeInfo(typeof(decimal?)) },
            };

        #endregion

        /// <summary>
        /// Provides a queryable dictionary of all the basic types including all primitives, string, DateTime, and all of their nullable counterparts.
        /// </summary>
        static public ReadOnlyDictionary<Type, ExtendedTypeInfo> BasicTypesInfo { get; } =
            new ReadOnlyDictionary<Type, ExtendedTypeInfo>(InternalBasicTypeInfo);

        /// <summary>
        /// Contains all basic types, including string, date time, and all of their nullable counterparts
        /// </summary>
        static public ReadOnlyCollection<Type> AllBasicTypes { get; } = new ReadOnlyCollection<Type>(BasicTypesInfo.Keys.ToArray());

        /// <summary>
        /// Contains all basic value types. i.e. excludes string and nullables
        /// </summary>
        static public ReadOnlyCollection<Type> AllBasicValueTypes { get; } = new ReadOnlyCollection<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsValueType)
                .Select(kvp => kvp.Key).ToArray()
            );

        /// <summary>
        /// Contains all basic value types including the string type. i.e. excludes nullables
        /// </summary>
        static public ReadOnlyCollection<Type> AllBasicValueAndStringTypes { get; } = new ReadOnlyCollection<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsValueType || kvp.Key == typeof(string))
                .Select(kvp => kvp.Key).ToArray()
            );

        /// <summary>
        /// Gets all nullable value types. i.e. excludes string and all basic value types
        /// </summary>
        static public ReadOnlyCollection<Type> AllBasicNullableValueTypes { get; } = new ReadOnlyCollection<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsNullableValueType)
                .Select(kvp => kvp.Key).ToArray()
            );


    }
}

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

        private static readonly Dictionary<Type, ExtendedTypeInfo> InternalBasicTypeInfo =
            new Dictionary<Type, ExtendedTypeInfo> {
                // Non-Nullables
                { typeof(DateTime), new ExtendedTypeInfo<DateTime>() },
                { typeof(byte), new ExtendedTypeInfo<byte>() },
                { typeof(sbyte), new ExtendedTypeInfo<sbyte>() },
                { typeof(int), new ExtendedTypeInfo<int>() },
                { typeof(uint), new ExtendedTypeInfo<uint>() },
                { typeof(short), new ExtendedTypeInfo<short>() },
                { typeof(ushort), new ExtendedTypeInfo<ushort>() },
                { typeof(long), new ExtendedTypeInfo<long>() },
                { typeof(ulong), new ExtendedTypeInfo<ulong>() },
                { typeof(float), new ExtendedTypeInfo<float>() },
                { typeof(double), new ExtendedTypeInfo<double>() },
                { typeof(char), new ExtendedTypeInfo<char>() },
                { typeof(bool), new ExtendedTypeInfo<bool>() },
                { typeof(decimal), new ExtendedTypeInfo<decimal>() },
                { typeof(Guid), new ExtendedTypeInfo<Guid>() },
                // Strings is also considered a basic type (it's the only basic reference type)
                { typeof(string), new ExtendedTypeInfo<string>() },
                // Nullables
                { typeof(DateTime?), new ExtendedTypeInfo<DateTime?>() },
                { typeof(byte?), new ExtendedTypeInfo<byte?>() },
                { typeof(sbyte?), new ExtendedTypeInfo<sbyte?>() },
                { typeof(int?), new ExtendedTypeInfo<int?>() },
                { typeof(uint?), new ExtendedTypeInfo<uint?>() },
                { typeof(short?), new ExtendedTypeInfo<short?>() },
                { typeof(ushort?), new ExtendedTypeInfo<ushort?>() },
                { typeof(long?), new ExtendedTypeInfo<long?>() },
                { typeof(ulong?), new ExtendedTypeInfo<ulong?>() },
                { typeof(float?), new ExtendedTypeInfo<float?>() },
                { typeof(double?), new ExtendedTypeInfo<double?>() },
                { typeof(char?), new ExtendedTypeInfo<char?>() },
                { typeof(bool?), new ExtendedTypeInfo<bool?>() },
                { typeof(decimal?), new ExtendedTypeInfo<decimal?>() },
                { typeof(Guid?), new ExtendedTypeInfo<Guid?>() },
            };

        #endregion

        /// <summary>
        /// Provides a queryable dictionary of all the basic types including all primitives, string, DateTime, and all of their nullable counterparts.
        /// </summary>
        public static ReadOnlyDictionary<Type, ExtendedTypeInfo> BasicTypesInfo { get; } =
            new ReadOnlyDictionary<Type, ExtendedTypeInfo>(InternalBasicTypeInfo);

        /// <summary>
        /// Contains all basic types, including string, date time, and all of their nullable counterparts
        /// </summary>
        public static ReadOnlyCollection<Type> AllBasicTypes { get; } = new ReadOnlyCollection<Type>(BasicTypesInfo.Keys.ToArray());

        /// <summary>
        /// Gets all numeric types including their nullable counterparts. 
        /// Note that Booleans and Guids are not considered numeric types
        /// </summary>
        public static ReadOnlyCollection<Type> AllNumericTypes { get; } = new ReadOnlyCollection<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsNumeric)
                .Select(kvp => kvp.Key).ToArray()
            );

        /// <summary>
        /// Gets all numeric types without their nullable counterparts. 
        /// Note that Booleans and Guids are not considered numeric types
        /// </summary>
        public static ReadOnlyCollection<Type> AllNumericValueTypes { get; } = new ReadOnlyCollection<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsNumeric && kvp.Value.IsNullableValueType == false)
                .Select(kvp => kvp.Key).ToArray()
            );

        /// <summary>
        /// Contains all basic value types. i.e. excludes string and nullables
        /// </summary>
        public static ReadOnlyCollection<Type> AllBasicValueTypes { get; } = new ReadOnlyCollection<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsValueType)
                .Select(kvp => kvp.Key).ToArray()
            );

        /// <summary>
        /// Contains all basic value types including the string type. i.e. excludes nullables
        /// </summary>
        public static ReadOnlyCollection<Type> AllBasicValueAndStringTypes { get; } = new ReadOnlyCollection<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsValueType || kvp.Key == typeof(string))
                .Select(kvp => kvp.Key).ToArray()
            );

        /// <summary>
        /// Gets all nullable value types. i.e. excludes string and all basic value types
        /// </summary>
        public static ReadOnlyCollection<Type> AllBasicNullableValueTypes { get; } = new ReadOnlyCollection<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsNullableValueType)
                .Select(kvp => kvp.Key).ToArray()
            );


    }
}

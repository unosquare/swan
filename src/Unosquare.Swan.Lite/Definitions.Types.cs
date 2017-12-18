namespace Unosquare.Swan
{
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Contains useful constants and definitions
    /// </summary>
    public partial class Definitions
    {
        #region Main Dictionary Definition

        /// <summary>
        /// The basic types information
        /// </summary>
        public static readonly Dictionary<Type, ExtendedTypeInfo> BasicTypesInfo =
            new Dictionary<Type, ExtendedTypeInfo>
            {
                // Non-Nullables
                {typeof(DateTime), new ExtendedTypeInfo<DateTime>()},
                {typeof(byte), new ExtendedTypeInfo<byte>()},
                {typeof(sbyte), new ExtendedTypeInfo<sbyte>()},
                {typeof(int), new ExtendedTypeInfo<int>()},
                {typeof(uint), new ExtendedTypeInfo<uint>()},
                {typeof(short), new ExtendedTypeInfo<short>()},
                {typeof(ushort), new ExtendedTypeInfo<ushort>()},
                {typeof(long), new ExtendedTypeInfo<long>()},
                {typeof(ulong), new ExtendedTypeInfo<ulong>()},
                {typeof(float), new ExtendedTypeInfo<float>()},
                {typeof(double), new ExtendedTypeInfo<double>()},
                {typeof(char), new ExtendedTypeInfo<char>()},
                {typeof(bool), new ExtendedTypeInfo<bool>()},
                {typeof(decimal), new ExtendedTypeInfo<decimal>()},
                {typeof(Guid), new ExtendedTypeInfo<Guid>()},
                
                // Strings is also considered a basic type (it's the only basic reference type)
                {typeof(string), new ExtendedTypeInfo<string>()},
                
                // Nullables
                {typeof(DateTime?), new ExtendedTypeInfo<DateTime?>()},
                {typeof(byte?), new ExtendedTypeInfo<byte?>()},
                {typeof(sbyte?), new ExtendedTypeInfo<sbyte?>()},
                {typeof(int?), new ExtendedTypeInfo<int?>()},
                {typeof(uint?), new ExtendedTypeInfo<uint?>()},
                {typeof(short?), new ExtendedTypeInfo<short?>()},
                {typeof(ushort?), new ExtendedTypeInfo<ushort?>()},
                {typeof(long?), new ExtendedTypeInfo<long?>()},
                {typeof(ulong?), new ExtendedTypeInfo<ulong?>()},
                {typeof(float?), new ExtendedTypeInfo<float?>()},
                {typeof(double?), new ExtendedTypeInfo<double?>()},
                {typeof(char?), new ExtendedTypeInfo<char?>()},
                {typeof(bool?), new ExtendedTypeInfo<bool?>()},
                {typeof(decimal?), new ExtendedTypeInfo<decimal?>()},
                {typeof(Guid?), new ExtendedTypeInfo<Guid?>()},
                
                // Additional Types
                {typeof(TimeSpan), new ExtendedTypeInfo<TimeSpan>()},
                {typeof(TimeSpan?), new ExtendedTypeInfo<TimeSpan?>()},
                {typeof(IPAddress), new ExtendedTypeInfo<IPAddress>()},
            };

        #endregion

        /// <summary>
        /// Contains all basic types, including string, date time, and all of their nullable counterparts
        /// </summary>
        /// <value>
        /// All basic types.
        /// </value>
        public static List<Type> AllBasicTypes { get; } = new List<Type>(BasicTypesInfo.Keys.ToArray());

        /// <summary>
        /// Gets all numeric types including their nullable counterparts.
        /// Note that Booleans and Guids are not considered numeric types
        /// </summary>
        /// <value>
        /// All numeric types.
        /// </value>
        public static List<Type> AllNumericTypes { get; } = new List<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsNumeric)
                .Select(kvp => kvp.Key).ToArray());

        /// <summary>
        /// Gets all numeric types without their nullable counterparts.
        /// Note that Booleans and Guids are not considered numeric types
        /// </summary>
        /// <value>
        /// All numeric value types.
        /// </value>
        public static List<Type> AllNumericValueTypes { get; } = new List<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsNumeric && kvp.Value.IsNullableValueType == false)
                .Select(kvp => kvp.Key).ToArray());

        /// <summary>
        /// Contains all basic value types. i.e. excludes string and nullables
        /// </summary>
        /// <value>
        /// All basic value types.
        /// </value>
        public static List<Type> AllBasicValueTypes { get; } = new List<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsValueType)
                .Select(kvp => kvp.Key).ToArray());

        /// <summary>
        /// Contains all basic value types including the string type. i.e. excludes nullables
        /// </summary>
        /// <value>
        /// All basic value and string types.
        /// </value>
        public static List<Type> AllBasicValueAndStringTypes { get; } = new List<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsValueType || kvp.Key == typeof(string))
                .Select(kvp => kvp.Key).ToArray());

        /// <summary>
        /// Gets all nullable value types. i.e. excludes string and all basic value types
        /// </summary>
        /// <value>
        /// All basic nullable value types.
        /// </value>
        public static List<Type> AllBasicNullableValueTypes { get; } = new List<Type>(
            BasicTypesInfo
                .Where(kvp => kvp.Value.IsNullableValueType)
                .Select(kvp => kvp.Key).ToArray());
    }
}
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Swan.Reflection;

namespace Swan
{
    /// <summary>
    /// Provides various extension methods for value types and structs.
    /// </summary>
    public static class ValueTypeExtensions
    {
        /// <summary>
        /// Clamps the specified value between the minimum and the maximum.
        /// </summary>
        /// <typeparam name="T">The type of value to clamp.</typeparam>
        /// <param name="this">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public static T Clamp<T>(this T @this, T min, T max)
            where T : struct, IComparable
        {
            if (@this.CompareTo(min) < 0) return min;

            return @this.CompareTo(max) > 0 ? max : @this;
        }

        /// <summary>
        /// Clamps the specified value between the minimum and the maximum.
        /// </summary>
        /// <param name="this">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public static int Clamp(this int @this, int min, int max)
            => @this < min ? min : (@this > max ? max : @this);

        /// <summary>
        /// Determines whether the specified value is between a minimum and a maximum value.
        /// </summary>
        /// <typeparam name="T">The type of value to check.</typeparam>
        /// <param name="this">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>
        ///   <c>true</c> if the specified minimum is between; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBetween<T>(this T @this, T min, T max)
            where T : struct, IComparable
        {
            return @this.CompareTo(min) >= 0 && @this.CompareTo(max) <= 0;
        }

        /// <summary>
        /// Converts an array of bytes into the given struct type.
        /// </summary>
        /// <typeparam name="T">The type of structure to convert.</typeparam>
        /// <param name="this">The data.</param>
        /// <returns>a struct type derived from convert an array of bytes ref=ToStruct".</returns>
        public static T ToStruct<T>(this byte[] @this)
            where T : struct
        {
            return @this == null ? throw new ArgumentNullException(nameof(@this)) : ToStruct<T>(@this, 0, @this.Length);
        }

        /// <summary>
        /// Converts an array of bytes into the given struct type.
        /// </summary>
        /// <typeparam name="T">The type of structure to convert.</typeparam>
        /// <param name="this">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// A managed object containing the data pointed to by the ptr parameter.
        /// </returns>
        /// <exception cref="ArgumentNullException">data.</exception>
        public static T ToStruct<T>(this byte[] @this, int offset, int length)
            where T : struct
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var buffer = new byte[length];
            Array.Copy(@this, offset, buffer, 0, buffer.Length);
            var handle = GCHandle.Alloc(GetStructBytes<T>(buffer), GCHandleType.Pinned);

            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Converts a struct to an array of bytes.
        /// </summary>
        /// <typeparam name="T">The type of structure to convert.</typeparam>
        /// <param name="this">The object.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
        public static byte[] ToBytes<T>(this T @this)
            where T : struct
        {
            var data = new byte[Marshal.SizeOf(@this)];
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(@this, handle.AddrOfPinnedObject(), false);
                return GetStructBytes<T>(data);
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Swaps the endianness of an unsigned long to an unsigned integer.
        /// </summary>
        /// <param name="this">The bytes contained in a long.</param>
        /// <returns>
        /// A 32-bit unsigned integer equivalent to the ulong 
        /// contained in longBytes.
        /// </returns>
        public static uint SwapEndianness(this ulong @this)
            => (uint)(((@this & 0x000000ff) << 24) +
                       ((@this & 0x0000ff00) << 8) +
                       ((@this & 0x00ff0000) >> 8) +
                       ((@this & 0xff000000) >> 24));

        private static byte[] GetStructBytes<T>(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var fields = typeof(T).GetTypeInfo()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var endian = AttributeCache.DefaultCache.Value.RetrieveOne<StructEndiannessAttribute, T>();

            foreach (var field in fields)
            {
                if (endian == null && !field.IsDefined(typeof(StructEndiannessAttribute), false))
                    continue;

                var offset = Marshal.OffsetOf<T>(field.Name).ToInt32();
                var length = Marshal.SizeOf(field.FieldType);

                endian ??= AttributeCache.DefaultCache.Value.RetrieveOne<StructEndiannessAttribute>(field);

                if (endian != null && (endian.Endianness == Endianness.Big && BitConverter.IsLittleEndian ||
                                       endian.Endianness == Endianness.Little && !BitConverter.IsLittleEndian))
                {
                    Array.Reverse(data, offset, length);
                }
            }

            return data;
        }
    }
}
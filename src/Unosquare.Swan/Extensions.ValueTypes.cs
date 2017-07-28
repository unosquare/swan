namespace Unosquare.Swan
{
    using System;
    using System.Reflection;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides various extension methods
    /// </summary>
    public partial class Extensions
    {
        /// <summary>
        /// Clamps the specified value between the minimum and the maximum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>A value that indicates the relative order of the objects being compared</returns>
        public static T Clamp<T>(this T value, T min, T max)
            where T : struct, IComparable
        {
            if (value.CompareTo(min) < 0) return min;

            return value.CompareTo(max) > 0 ? max : value;
        }

        /// <summary>
        /// Determines whether the specified value is between a minimum and a maximum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>
        ///   <c>true</c> if the specified minimum is between; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBetween<T>(this T value, T min, T max)
            where T : struct, IComparable
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// Swaps the endianness of an unsigned long to an unsigned integer.
        /// </summary>
        /// <param name="longBytes">The bytes contained in a long.</param>
        /// <returns>method returns a unit</returns>
        internal static uint SwapEndianness(this ulong longBytes)
        {
            return (uint)(((longBytes & 0x000000ff) << 24) +
                           ((longBytes & 0x0000ff00) << 8) +
                           ((longBytes & 0x00ff0000) >> 8) +
                           ((longBytes & 0xff000000) >> 24));
        }

        /// <summary>
        /// Adjusts the endianness of the type represented by the data byte array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters</returns>
        private static byte[] GetStructBytes<T>(byte[] data)
        {
#if !NETSTANDARD1_3 && !UWP
            var fields = typeof(T).GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#else
            var fields = typeof(T).GetTypeInfo().DeclaredFields;
#endif
            StructEndiannessAttribute endian = null;

            if (typeof(T).IsDefined(typeof(StructEndiannessAttribute), false))
            {
                endian = typeof(T).GetCustomAttributes(typeof(StructEndiannessAttribute), false)[0] as StructEndiannessAttribute;
            }

            foreach (var field in fields)
            {
                if (endian == null && !field.IsDefined(typeof(StructEndiannessAttribute), false))
                    continue;
                
                var offset = Marshal.OffsetOf<T>(field.Name).ToInt32();
                var length = Marshal.SizeOf(field.FieldType);

                endian = endian ?? field.GetCustomAttributes(typeof(StructEndiannessAttribute), false).ToArray()[0] as StructEndiannessAttribute;

                if (endian != null && (endian.Endianness == Endianness.Big && BitConverter.IsLittleEndian ||
                                       endian.Endianness == Endianness.Little && !BitConverter.IsLittleEndian))
                {
                    Array.Reverse(data, offset, length);
                }
            }

            return data;
        }

        /// <summary>
        /// Converts an array of bytes into the given struct type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>a struct type derived from convert an array of bytes ref=ToStruct"</returns>
        public static T ToStruct<T>(this byte[] data) 
            where T : struct
        {
            return ToStruct<T>(data, 0, data.Length);
        }

        /// <summary>
        /// Converts an array of bytes into the given struct type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>A managed object containing the data pointed to by the ptr parameter</returns>
        public static T ToStruct<T>(this byte[] data, int offset, int length) 
            where T : struct
        {
            var buffer = new byte[length];
            Array.Copy(data, offset, buffer, 0, buffer.Length);
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
        /// Converts a struct to an array of bytes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters</returns>
        public static byte[] ToBytes<T>(this T obj) 
            where T : struct
        {
            var data = new byte[Marshal.SizeOf(obj)];
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
                return GetStructBytes<T>(data);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}

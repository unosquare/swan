namespace Unosquare.Swan
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides various extension methods
    /// </summary>
    partial class Extensions
    {
        /// <summary>
        /// Clamps the specified value between the minimum and the maximum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
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
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether this instance is collection.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <returns>
        ///   <c>true</c> if the specified property is collection; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCollection(this PropertyInfo prop)
        {
            return prop.PropertyType != typeof(string) &&
                             typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType);
        }

        /// <summary>
        /// Swaps the endianness of an unsigned long to an unsigned integer.
        /// </summary>
        /// <param name="longBytes">The bytes contained in a long.</param>
        /// <returns></returns>
        public static uint SwapEndianness(this ulong longBytes)
        {
            return (uint)(((longBytes & 0x000000ff) << 24) +
                           ((longBytes & 0x0000ff00) << 8) +
                           ((longBytes & 0x00ff0000) >> 8) +
                           ((longBytes & 0xff000000) >> 24));
        }


        /// <summary>
        /// Adjusts the endianness of the type represented by the data byte array.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private static byte[] AdjustEndianness(Type type, byte[] data)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            EndianAttribute endian = null;

            if (type.IsDefined(typeof(EndianAttribute), false))
            {
                endian = type.GetCustomAttributes(typeof(EndianAttribute), false)[0] as EndianAttribute;
            }

            foreach (var field in fields)
            {
                if (endian == null && !field.IsDefined(typeof(EndianAttribute), false))
                {
                    continue;
                }

                int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                int length = Marshal.SizeOf(field.FieldType);
                endian = endian ?? field.GetCustomAttributes(typeof(EndianAttribute), false).ToArray()[0] as EndianAttribute;

                if (endian.Endianness == Endianness.Big && BitConverter.IsLittleEndian ||
                        endian.Endianness == Endianness.Little && !BitConverter.IsLittleEndian)
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
        /// <returns></returns>
        public static T ToStruct<T>(this byte[] data) where T : struct
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
        /// <returns></returns>
        public static T ToStruct<T>(this byte[] data, int offset, int length) where T : struct
        {
            byte[] buffer = new byte[length];
            Array.Copy(data, offset, buffer, 0, buffer.Length);
            var handle = GCHandle.Alloc(AdjustEndianness(typeof(T), buffer), GCHandleType.Pinned);

            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
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
        /// <returns></returns>
        public static byte[] ToBytes<T>(this T obj) where T : struct
        {
            byte[] data = new byte[Marshal.SizeOf(obj)];
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
                return AdjustEndianness(typeof(T), data);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}

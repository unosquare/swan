using System;

namespace Swan.Extensions
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
            if (@this.CompareTo(min) < 0)
                return min;

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
            where T : struct, IComparable =>
            @this.CompareTo(min) >= 0 && @this.CompareTo(max) <= 0;

        
    }
}
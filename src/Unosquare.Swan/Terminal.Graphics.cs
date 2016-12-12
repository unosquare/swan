using System;

namespace Unosquare.Swan
{
    partial class Terminal
    {
        /// <summary>
        /// Represents a Table to print in console
        /// </summary>
        private static class Table
        {
            /// <summary>
            /// Gets or sets the color of the border.
            /// </summary>
            /// <value>
            /// The color of the border.
            /// </value>
            private static ConsoleColor BorderColor { get; } = ConsoleColor.DarkGreen;

            public static void Vertical()
            {
                ((byte)179).Write(BorderColor, 1, false);
            }

            public static void RightTee()
            {
                ((byte)180).Write(BorderColor, 1, false);
            }

            public static void TopRight()
            {
                ((byte)191).Write(BorderColor, 1, false);
            }

            public static void BottomLeft()
            {
                ((byte)192).Write(BorderColor, 1, false);
            }

            public static void BottomTee()
            {
                ((byte)193).Write(BorderColor, 1, false);
            }

            public static void TopTee()
            {
                ((byte)194).Write(BorderColor, 1, false);
            }

            public static void LeftTee()
            {
                ((byte)195).Write(BorderColor, 1, false);
            }

            public static void Horizontal(int length)
            {
                ((byte)196).Write(BorderColor, length, false);
            }

            public static void Tee()
            {
                ((byte)197).Write(BorderColor, 1, false);
            }

            public static void BottomRight()
            {
                ((byte)217).Write(BorderColor, 1, false);
            }

            public static void TopLeft()
            {
                ((byte)218).Write(BorderColor, 1, false);
            }
        }

    }
}

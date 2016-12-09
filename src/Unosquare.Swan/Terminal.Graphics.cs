using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan
{
    partial class Terminal
    {
        /// <summary>
        /// Represents a Table to print in console
        /// </summary>
        static private class Table
        {
            /// <summary>
            /// Gets or sets the color of the border.
            /// </summary>
            /// <value>
            /// The color of the border.
            /// </value>
            static public ConsoleColor BorderColor { get; set; } = ConsoleColor.DarkGreen;

            static public void Vertical()
            {
                ((byte)179).Write(BorderColor, 1, false);
            }

            static public void RightTee()
            {
                ((byte)180).Write(BorderColor, 1, false);
            }

            static public void TopRight()
            {
                ((byte)191).Write(BorderColor, 1, false);
            }

            static public void BottomLeft()
            {
                ((byte)192).Write(BorderColor, 1, false);
            }

            static public void BottomTee()
            {
                ((byte)193).Write(BorderColor, 1, false);
            }

            static public void TopTee()
            {
                ((byte)194).Write(BorderColor, 1, false);
            }

            static public void LeftTee()
            {
                ((byte)195).Write(BorderColor, 1, false);
            }

            static public void Horizontal(int length)
            {
                ((byte)196).Write(BorderColor, length, false);
            }

            static public void Tee()
            {
                ((byte)197).Write(BorderColor, 1, false);
            }

            static public void BottomRight()
            {
                ((byte)179).Write(BorderColor, 217, false);
            }

            static public void TopLeft()
            {
                ((byte)218).Write(BorderColor, 1, false);
            }
        }

    }
}

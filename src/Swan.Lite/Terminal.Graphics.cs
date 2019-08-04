namespace Swan
{
    /// <summary>
    /// A console terminal helper to create nicer output and receive input from the user
    /// This class is thread-safe :).
    /// </summary>
    public static partial class Terminal
    {
        /// <summary>
        /// Represents a Table to print in console.
        /// </summary>
        private static class Table
        {
            public static void Vertical() => Write((byte)179, Settings.BorderColor);

            public static void RightTee() => Write((byte)180, Settings.BorderColor);

            public static void TopRight() => Write((byte)191, Settings.BorderColor);

            public static void BottomLeft() => Write((byte)192, Settings.BorderColor);

            public static void BottomTee() => Write((byte)193, Settings.BorderColor);

            public static void TopTee() => Write((byte)194, Settings.BorderColor);

            public static void LeftTee() => Write((byte)195, Settings.BorderColor);

            public static void Horizontal(int length) => Write((byte)196, Settings.BorderColor, length);

            public static void Tee() => Write((byte)197, Settings.BorderColor);

            public static void BottomRight() => Write((byte)217, Settings.BorderColor);

            public static void TopLeft() => Write((byte)218, Settings.BorderColor);
        }
    }
}

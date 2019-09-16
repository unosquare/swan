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
            public static void Vertical() => Write('\u2502', Settings.BorderColor);

            public static void RightTee() => Write('\u2524', Settings.BorderColor);

            public static void TopRight() => Write('\u2510', Settings.BorderColor);

            public static void BottomLeft() => Write('\u2514', Settings.BorderColor);

            public static void BottomTee() => Write('\u2534', Settings.BorderColor);

            public static void TopTee() => Write('\u252c', Settings.BorderColor);

            public static void LeftTee() => Write('\u251c', Settings.BorderColor);

            public static void Horizontal(int length) => Write(new string('\u2500', length), Settings.BorderColor);

            public static void Tee() => Write('\u253c', Settings.BorderColor);

            public static void BottomRight() => Write('\u2518', Settings.BorderColor);

            public static void TopLeft() => Write('\u250C', Settings.BorderColor);
        }
    }
}

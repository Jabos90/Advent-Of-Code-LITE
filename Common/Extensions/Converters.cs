namespace Common.Extensions;

public static class Converters
{   /// <summary>
    /// Converts the string representation of a number to its 32-bit signed integer equivalent
    /// </summary>
    /// <param name="s">A string containing a number to convert</param>
    /// <returns>A 32-bit signed integer equivalent to the number contained in s</returns>
    public static int ToInt(this string s) => int.Parse(s);
}
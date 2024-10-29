namespace Common.Extensions;

public static class Sequences
{   /// <summary>
    /// Find the smallest value out of a number of values
    /// </summary>
    /// <param name="values">The values to examine</param>
    /// <returns>The smallest value out of the given values</returns>
    public static T Min<T>(params T[] values) => values.Min();
}
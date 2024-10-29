using System.Numerics;

namespace Common.Extensions;

public static class Tuples
{   /// <summary>
    /// Add the values in a tuple together with the values in another tuple
    /// </summary>
    /// <param name="a">The tuple whose values to increase</param>
    /// <param name="b">The tuple with the values to add</param>
    /// <returns>A tuple with the values in <see cref="a"/> added to the values in <see cref="b"/></returns>
    public static (T, T) Add<T>(this (T, T) a, (T, T) b) where T : IAdditionOperators<T, T, T> =>
        (a.Item1 + b.Item1, a.Item2 + b.Item2);
}
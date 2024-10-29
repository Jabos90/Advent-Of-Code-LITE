namespace Common.Extensions;

public static class Dictionaries
{   /// <summary>
    /// Adds the specified keys and value to the dictionary
    /// </summary>
    /// <param name="dictionary">The dictionary to add element to</param>
    /// <param name="keys">A tuple containing the keys of the element to add</param>
    /// <param name="value">The value of the element to add</param>
    public static void Add<TOuter, TInner, TValue>(this Dictionary<TOuter, Dictionary<TInner, TValue>> dictionary, (TOuter Outer, TInner Inner) keys, TValue value) =>
        dictionary.Add(keys.Outer, keys.Inner, value);

    /// <summary>
    /// Adds the specified keys and value to the dictionary
    /// </summary>
    /// <param name="dictionary">The dictionary to add element to</param>
    /// <param name="outerKey">The outer key of the element to add</param>
    /// <param name="innerKey">The inner key of the element to add</param>
    /// <param name="value">The value of the element to add</param>
    public static void Add<TOuter, TInner, TValue>(this Dictionary<TOuter, Dictionary<TInner, TValue>> dictionary, TOuter outerKey, TInner innerKey, TValue value)
    {
        if (dictionary.TryGetValue(outerKey, out var innerDictionary))
            innerDictionary[innerKey] = value;
        else
        {   // Outer key is absent
            innerDictionary = new Dictionary<TInner, TValue> { { innerKey, value } };
            dictionary.Add(outerKey, innerDictionary);
        }
    }

    /// <summary>
    /// Gets the number of values contained in the dictionary
    /// </summary>
    /// <param name="dictionary">The dictionary whose values to count</param>
    /// <returns>The number of values contained in the dictionary</returns>
    public static int Count<TOuter, TInner, TValue>(this Dictionary<TOuter, Dictionary<TInner, TValue>> dictionary)
    {
        var totalCount = 0;
        foreach (var kvp in dictionary)
            totalCount += kvp.Value.Count;
        return totalCount;
    }
}
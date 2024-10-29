namespace Common.Extensions;

public static class Deconstructors
{
    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out IEnumerable<T> rest)
    {
        a = list.GetElement(0);
        rest = list.Skip(1).ToList();
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out rest);
        rest.Deconstruct(out b, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out rest);
        rest.Deconstruct(out c, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out rest);
        rest.Deconstruct(out d, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out rest);
        rest.Deconstruct(out e, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out rest);
        rest.Deconstruct(out f, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out T g, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out f, out rest);
        rest.Deconstruct(out g, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out T g, out T h, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out f, out g, out rest);
        rest.Deconstruct(out h, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out T g, out T h, out T i, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out f, out g, out h, out rest);
        rest.Deconstruct(out i, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out T g, out T h, out T i, out T j, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out f, out g, out h, out i, out rest);
        rest.Deconstruct(out j, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out T g, out T h, out T i, out T j, out T k, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out f, out g, out h, out i, out j, out rest);
        rest.Deconstruct(out k, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out T g, out T h, out T i, out T j, out T k, out T l, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out f, out g, out h, out i, out j, out k, out rest);
        rest.Deconstruct(out l, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out T g, out T h, out T i, out T j, out T k, out T l, out T m, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out f, out g, out h, out i, out j, out k, out l, out rest);
        rest.Deconstruct(out m, out rest);
    }

    public static void Deconstruct<T>(this IEnumerable<T> list, out T a, out T b, out T c, out T d, out T e, out T f, out T g, out T h, out T i, out T j, out T k, out T l, out T m, out T n, out IEnumerable<T> rest)
    {
        list.Deconstruct(out a, out b, out c, out d, out e, out f, out g, out h, out i, out j, out k, out l, out m, out rest);
        rest.Deconstruct(out n, out rest);
    }

    private static T GetElement<T>(this IEnumerable<T> list, int index) => list.Count() > index ? list.ElementAt(index) : default;
}
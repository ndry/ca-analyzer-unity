using System;
using System.Collections.Generic;
using System.Linq;

public static class Ex {
    public static IEnumerable<T> Do<T>(
        this IEnumerable<T> source,
        Action<T> action
    ) => source.Select(x => { action(x); return x; });
    public static IEnumerable<T> Do<T>(
        this IEnumerable<T> source,
        Action<T, int> action
    ) => source.Select((x, i) => { action(x, i); return x; });
    public static void Add<T>(
        this ICollection<T> dest,
        IEnumerable<T> source
    ) => source.Do(dest.Add);
    public static void Remove<T>(
        this ICollection<T> dest,
        IEnumerable<T> source
    ) => source.Do(e => dest.Remove(e));
    public static void AddTo<T>(
        this IEnumerable<T> source,
        ICollection<T> dest
    ) => dest.Add(source);
    public static void RemoveFrom<T>(
        this IEnumerable<T> source,
        ICollection<T> dest
    ) => dest.Remove(source);
    public static HashSet<T> ToSet<T>(this IEnumerable<T> source)
        => new HashSet<T>(source);
    public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        => new Queue<T>(source);
    public static IEnumerable<T> Elements<T>(this T[,] source) {
        foreach (var item in source) {
            yield return item;
        }
    }
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        this IEnumerable<(TKey key, TValue value)> source
    ) => source.ToDictionary(kv => kv.key, kv => kv.value);
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        this IEnumerable<(TKey key, TValue value)> source, 
        IEqualityComparer<TKey> comparer
    ) => source.ToDictionary(kv => kv.Item1, kv => kv.Item2, comparer);
}

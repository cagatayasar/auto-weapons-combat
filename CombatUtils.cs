using System;
using System.Collections;
using System.Collections.Generic;

public static partial class Utils
{
    public static float Rad2Deg => 180f / MathF.PI;

    public static R To<T, R>(this T item, Func<T, R> func)
    {
        return func(item);
    }

    public static int GetDigitCount(this int n) {
        if (n < 0) {
            n = (n == Int32.MinValue) ? Int32.MaxValue : -n;
        }
        if (n < 10) return 1;
        if (n < 100) return 2;
        if (n < 1000) return 3;
        if (n < 10000) return 4;
        if (n < 100000) return 5;
        if (n < 1000000) return 6;
        if (n < 10000000) return 7;
        if (n < 100000000) return 8;
        if (n < 1000000000) return 9;
        return 10;
    }

    public static int[] ToDigitArray(this int n)
    {
        var result = new int[n.GetDigitCount()];
        for (int i = result.Length - 1; i >= 0; i--) {
            result[i] = n % 10;
            n /= 10;
        }
        return result;
    }

    public static int ToMultiplier(this bool value)
    {
        return value ? 1 : -1;
    }

    public static int ToBinary(this bool value)
    {
        return value ? 1 : 0;
    }

    public static void Repeat(this int count, Action action)
    {
        for (int i = 0; i < count; i++) {
            action();
        }
    }

    public static bool IsInBetween<T>(this T num, T lowerBoundary, T upperBoundary) where T : IComparable
    {
        return num.CompareTo(lowerBoundary) >= 0 && num.CompareTo(upperBoundary) <= 0;
    }

    public static bool Contains_<T>(this IList<T> list, T item) where T : class
    {
        var count = list.Count;
        for (int i = 0; i < count; i++) {
            if (list[i] == item)
                return true;
        }
        return false;
    }

    public static bool Any_<T>(this IList<T> list, Func<T, bool> predicate)
    {
        var count = list.Count;
        for (int i = 0; i < count; i++) {
            if (predicate(list[i]))
                return true;
        }
        return false;
    }

    public static List<TResult> SelectWhere_<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector, Func<TSource, bool> predicate)
    {
        var result = new List<TResult>();
        foreach (var item in enumerable) {
            if (predicate(item))
                result.Add(selector(item));
        }

        return result;
    }

    public static CombatWeaponSaved FindCWS(this List<List<CombatWeaponSaved>> rowsList, int matchRosterIndex)
    {
        for (int i = 0; i < rowsList.Count; i++) {
            for (int j = 0; j < rowsList[i].Count; j++) {
                    if (rowsList[i][j].matchRosterIndex == matchRosterIndex) {
                    return rowsList[i][j];
                }
            }
        }
        return null;
    }
}

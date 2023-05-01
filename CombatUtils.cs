using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static partial class Utils
{
    public static NumberFormatInfo decimalSeperator = new NumberFormatInfo();
    public static float Rad2Deg => 180f / MathF.PI;

    public static float NextFloat(this System.Random rnd, float minValue = 0f, float maxValue = 1f)
    {
        var randomDbl = rnd.NextDouble();
        return (float) (randomDbl * ((double) maxValue - (double) minValue) + minValue);
    }

    public static bool NextBool(this System.Random rnd)
    {
        return rnd.Next() > (Int32.MaxValue / 2);
    }

    public static IList<T> Shuffle<T>(this IList<T> list, ref System.Random rnd)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public static string ToString(this List_<List_<CW>> rowsList)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < rowsList.Count; i++)
        {
            sb.Append($"Row {i+1}: ");
            foreach (var cw in rowsList[i])
                sb.Append($"{cw.weapon.weaponType}".PadRight(15, ' ').Substring(0, 15));
            sb.Append($"\n");
        }
        return sb.ToString();
    }

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

    public static string GetWithDecimalZero(float num)
    {
        var str = num.ToString(decimalSeperator);
        return str.Length == 1 ? str + ".0" : str;
    }

    public static int ToMultiplier(this bool value) => value ? 1 : -1;
    public static int ToBinary(this bool value)     => value ? 1 :  0;

    public static void Repeat(this int count, Action action)
    {
        for (int i = 0; i < count; i++) {
            action();
        }
    }

    public static bool Contains_<T>(this List_<T> list, T item) where T : class
    {
        var count = list.Count;
        for (int i = 0; i < count; i++) {
            if (list[i] == item)
                return true;
        }
        return false;
    }

    public static bool Any_<T>(this List_<T> list, Func<T, bool> predicate)
    {
        var count = list.Count;
        for (int i = 0; i < count; i++) {
            if (predicate(list[i]))
                return true;
        }
        return false;
    }

    public static T FirstOrDefault_<T>(this List_<T> list, Func<T, bool> predicate)
    {
        var count = list.Count;
        for (int i = 0; i < count; i++) {
            var item = list[i];
            if (predicate(item))
                return item;
        }
        return default(T);
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

    public static CWSaved FindCWS(this List<List<CWSaved>> rowsList, int matchRosterIndex)
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

    public static string ReplaceVariable(this string desc, object obj, char delimeterBegin, char delimeterEnd, out bool success, Func<string, string> actOnVariableName = null)
    {
        success = false;
        if (string.IsNullOrEmpty(desc)) return desc;

        var startIndex = desc.IndexOf(delimeterBegin, 0);
        if (startIndex == -1) return desc;

        var endIndex = desc.IndexOf(delimeterEnd, startIndex);
        if (endIndex == -1) return desc;

        var sb = new StringBuilder();
        var variableName = desc.Substring(startIndex + 1, endIndex - startIndex - 1);
        if (actOnVariableName != null)
            variableName = actOnVariableName(variableName);

        sb.Append(desc.Substring(0, startIndex));
        sb.Append(obj.GetVariable(variableName));
        sb.Append(desc.Substring(endIndex + 1, desc.Length - endIndex - 1));

        success = true;
        return sb.ToString();
    }

    public static string GetVariable(this object obj, string variable)
    {
        var fieldType = obj.GetType().GetField(variable).FieldType;
        var value = obj.GetType().GetField(variable).GetValue(obj);

        if (fieldType == typeof(float)) {
            return Utils.GetWithDecimalZero((float) value);
        } else {
            return value.ToString();
        }
    }
}

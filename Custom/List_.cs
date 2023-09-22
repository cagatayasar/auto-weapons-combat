using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoWeapons {

public class List_<T> : List<T>
{
    public new int Count;

    public List_() : base() {}
    public List_(int capacity) : base(capacity) {}

    //------------------------------------------------------------------------
    public new void Add(T item)
    {
        base.Add(item);
        Count++;
    }

    //------------------------------------------------------------------------
    public new void AddRange(IEnumerable<T> collection)
    {
        base.AddRange(collection);
        Count = base.Count;
    }

    //------------------------------------------------------------------------
    public new void Insert(int index, T item)
    {
        base.Insert(index, item);
        Count++;
    }

    //------------------------------------------------------------------------
    public new void InsertRange(int index, IEnumerable<T> collection)
    {
        base.InsertRange(index, collection);
        Count = base.Count;
    }

    //------------------------------------------------------------------------
    public new bool Remove(T item)
    {
        if (base.Remove(item)) {
            Count--;
            return true;
        }
        return false;
    }

    //------------------------------------------------------------------------
    public new int RemoveAll(Predicate<T> match)
    {
        var result = base.RemoveAll(match);
        Count = base.Count;
        return result;
    }

    //------------------------------------------------------------------------
    public new void RemoveAt(int index)
    {
        base.RemoveAt(index);
        Count--;
    }

    //------------------------------------------------------------------------
    public new void RemoveRange(int index, int count)
    {
        base.RemoveRange(index, count);
        Count -= count;
    }

    //------------------------------------------------------------------------
    public new void Clear()
    {
        base.Clear();
        Count = 0;
    }
}
}

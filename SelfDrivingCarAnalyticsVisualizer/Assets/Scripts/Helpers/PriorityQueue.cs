using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<T> list;
    private Comparer<T> comparer;
    public int Count { get { return list.Count; } }
    public readonly bool IsDescending;

    public PriorityQueue(Comparer<T> comparer)
    {
        this.comparer = comparer;
        list = new List<T>();
    }

    public PriorityQueue(Comparer<T> comparer, bool isdesc)
        : this(comparer)
    {
        IsDescending = isdesc;
    }

    public PriorityQueue(Comparer<T> comparer, int capacity)
        : this(comparer, capacity, false)
    { }

    public PriorityQueue(Comparer<T> comparer, IEnumerable<T> collection)
        : this(comparer, collection, false)
    { }

    public PriorityQueue(Comparer<T> comparer, int capacity, bool isdesc)
    {
        list = new List<T>(capacity);
        IsDescending = isdesc;
    }

    public PriorityQueue(Comparer<T> comparer, IEnumerable<T> collection, bool isdesc)
        : this(comparer)
    {
        IsDescending = isdesc;
        foreach (var item in collection)
            Enqueue(item);
    }


    public void Enqueue(T x)
    {
        list.Add(x);
        int i = Count - 1;

        while (i > 0)
        {
            int p = (i - 1) / 2;
            if ((IsDescending ? -1 : 1) * comparer.Compare(list[p], x) <= 0) break;

            list[i] = list[p];
            i = p;
        }

        if (Count > 0) list[i] = x;
    }

    public T Dequeue()
    {
        T target = Peek();
        T root = list[Count - 1];
        list.RemoveAt(Count - 1);

        int i = 0;
        while (i * 2 + 1 < Count)
        {
            int a = i * 2 + 1;
            int b = i * 2 + 2;
            int c = b < Count && (IsDescending ? -1 : 1) * comparer.Compare(list[b], list[a]) < 0 ? b : a;

            if ((IsDescending ? -1 : 1) * comparer.Compare(list[c], root) >= 0) break;
            list[i] = list[c];
            i = c;
        }

        if (Count > 0) list[i] = root;
        return target;
    }

    public T Peek()
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty.");
        return list[0];
    }

    public void Clear()
    {
        list.Clear();
    }
}
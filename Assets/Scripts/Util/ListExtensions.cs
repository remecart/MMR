using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListExtensions
{
    public static void InsertSorted<T>(this List<T> list, T item, Comparison<T> comparison)
    {
        var index = list.BinarySearch(item, Comparer<T>.Create(comparison));
        if (index < 0)
        {
            index = ~index;
        }

        list.Insert(index, item);
    }

    public static void InsertAtBeat<T>(this List<T> list, T item, float beat) where T : BeatmapObject
    {
        // Use insertSorted with a comparison that compares the beat of the objects
        list.InsertSorted(item, (x, y) => x.Beat.CompareTo(y.Beat));
    }

    public static bool Active(this List<KeyCode> keyCodes)
    {
        return keyCodes.All(Input.GetKey) && keyCodes.Any(Input.GetKeyDown);
    }
}
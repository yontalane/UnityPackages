using System.Collections.Generic;
using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A static class containing list extension methods.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Shuffles the list in place.
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            T item;
            int index;
            int count = list.Count;
            while (count > 0)
            {
                index = Mathf.FloorToInt(count * Random.value);
                item = list[index];
                list.RemoveAt(index);
                list.Add(item);
                count--;
            }
        }
    }
}

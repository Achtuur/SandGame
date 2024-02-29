using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace ExtensionMethods
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<(T, int)> Enumerate<T>(this IEnumerable<T> enumerable)
        {
            foreach(var item in enumerable.Select((value, index) => new { value, index }))
            {
                yield return (item.value, item.index);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using RandomMemory.Internal;

namespace RandomMemory
{
    public abstract class TransactionBase
    {
        protected TElement[] CloneAndSortBy<TElement, TKey>(IList<TElement> elementData, Func<TElement, TKey> indexSelector, IComparer<TKey> comparer)
        {
            var array = new TElement[elementData.Count];
            var sortSource = new TKey[elementData.Count];
            for (var i = 0; i < elementData.Count; i++)
            {
                array[i] = elementData[i];
                sortSource[i] = indexSelector(elementData[i]);
            }

            Array.Sort(sortSource, array, 0, array.Length, comparer);
            return array;
        }

        protected TElement[] RemoveCore<TElement, TKey>(TElement[] array, TKey key,
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            var index = BinarySearch.FindFirst(array, key, keySelector, comparer);
            if (index >= 0)
            {
                var newArray = new TElement[array.Length - 1];
                Array.Copy(array, 0, newArray, 0, index);
                Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
                return newArray;
            }

            return array;
        }

        protected TElement[] RemoveCore<TElement, TKey>(TElement[] array, TKey[] keys,
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (keys == null || keys.Length == 0)
                return array;

            var dest = RemoveCore(array, keys[0], keySelector, comparer);
            for (var i = 1; i < keys.Length; i++)
            {
                dest = RemoveCore(dest, keys[i], keySelector, comparer);
            }
            return dest;
        }

        protected TElement[] DiffCore<TElement, TKey>(TElement[] array, TElement addOrReplaceData,
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            var dest = new TElement[array.Length];
            Array.Copy(array, dest, dest.Length);

            var insertionIndex =
                BinarySearch.FindFirst(array, keySelector(addOrReplaceData), keySelector, comparer);

            if (insertionIndex >= 0)
            {
                dest[insertionIndex] = addOrReplaceData;
            }
            else
            {
                insertionIndex = ~insertionIndex;
                var newArray = new TElement[array.Length + 1];
                if (insertionIndex == 0)
                {
                    newArray[0] = addOrReplaceData;
                    Array.Copy(dest, 0, newArray, 1, dest.Length);
                }
                else if (insertionIndex == dest.Length)
                {
                    Array.Copy(dest, 0, newArray, 0, dest.Length);
                    newArray[dest.Length] = addOrReplaceData;
                }
                else
                {
                    Array.Copy(dest, 0, newArray, 0, insertionIndex);
                    newArray[insertionIndex] = addOrReplaceData;
                    Array.Copy(dest, insertionIndex, newArray, insertionIndex + 1, dest.Length - insertionIndex);
                }

                dest = newArray;
            }

            return dest;
        }

        protected List<TElement> DiffCore<TElement, TKey>(TElement[] array, TElement[] addOrReplaceData, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            var newList = new List<TElement>(array.Length);
            var replaceIndexes = new Dictionary<int, TElement>();
            foreach (var data in addOrReplaceData)
            {
                var index = BinarySearch.FindFirst(array, keySelector(data), keySelector, comparer);
                if (index != -1)
                {
                    replaceIndexes.Add(index, data);
                }
                else
                {
                    newList.Add(data);
                }
            }

            for (var i = 0; i < array.Length; i++)
            {
                newList.Add(replaceIndexes.TryGetValue(i, out var data) ? data : array[i]);
            }

            return newList;
        }
    }
}

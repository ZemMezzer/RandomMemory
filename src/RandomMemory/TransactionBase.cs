using System;
using System.Collections.Generic;
using System.Linq;
using RandomMemory.Internal;

namespace RandomMemory
{
    public abstract class TransactionBase
    {
        protected List<TElement> CloneAndSortBy<TElement, TKey>(IList<TElement> elementData, Func<TElement, TKey> indexSelector, IComparer<TKey> comparer)
        {
            var array = new TElement[elementData.Count];
            var sortSource = new TKey[elementData.Count];
            for (var i = 0; i < elementData.Count; i++)
            {
                array[i] = elementData[i];
                sortSource[i] = indexSelector(elementData[i]);
            }

            Array.Sort(sortSource, array, 0, array.Length, comparer);
            return array.ToList();
        }

        protected void RemoveCore<TElement, TKey>(
            IList<TElement> list,
            TKey key,
            Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            var index = BinarySearch.FindIndexOrInsertionIndex(
                list,
                key,
                keySelector,
                comparer);

            if (index >= 0)
            {
                list.RemoveAt(index);
            }
        }

        protected void RemoveCore<TElement, TKey>(
            IList<TElement> list,
            TKey[] keys,
            Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            if (keys == null || keys.Length == 0)
                return;

            for (var i = 0; i < keys.Length; i++)
            {
                RemoveCore(list, keys[i], keySelector, comparer);
            }
        }

        protected void DiffCore<TElement, TKey>(
            List<TElement> list,
            TElement addOrReplaceData,
            Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            var key = keySelector(addOrReplaceData);

            var index = BinarySearch.FindIndexOrInsertionIndex(
                list,
                key,
                keySelector,
                comparer);

            if (index >= 0)
            {
                list[index] = addOrReplaceData;
                return;
            }

            list.Insert(~index, addOrReplaceData);
        }

        protected void DiffCore<TElement, TKey>(
            List<TElement> list,
            TElement[] addOrReplaceData,
            Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            foreach (var data in addOrReplaceData)
            {
                var key = keySelector(data);

                var index = BinarySearch.FindIndexOrInsertionIndex(
                    list,
                    key,
                    keySelector,
                    comparer);

                if (index >= 0)
                {
                    list[index] = data;
                }
                else
                {
                    list.Insert(~index, data);
                }
            }
        }
    }
}

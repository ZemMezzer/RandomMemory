using System;
using System.Collections.Generic;

namespace RandomMemory.Internal
{
    internal static class BinarySearch
    {
        public static int FindIndexOrInsertionIndex<T, TKey>(IList<T> source, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            var lo = 0;
            var hi = source.Count - 1;

            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var found = comparer.Compare(selector(source[mid]), key);

                if (found == 0) return mid;

                if (found < 0)
                    lo = mid + 1;
                else
                    hi = mid - 1;
            }

            return ~lo;
        }

        public static int FindFirst<T, TKey>(IList<T> source, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            var lo = 0;
            var hi = source.Count - 1;

            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var found = comparer.Compare(selector(source[mid]), key);

                if (found == 0) return mid;
                if (found < 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return -1;
        }

        public static int FindFirstIntKey<T>(IList<T> source, int key, Func<T, int> selector)
        {
            var lo = 0;
            var hi = source.Count - 1;

            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                // compare inlining
                var selectedValue = selector(source[mid]);
                var found = (selectedValue < key) ? -1 : (selectedValue > key) ? 1 : 0;

                if (found == 0) return mid;
                if (found < 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return -1;
        }

        // lo = 0, hi = Count.
        public static int FindClosest<T, TKey>(IList<T> source, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer, bool selectLower)
        {
            if (source.Count == 0) return -1;

            lo = lo - 1;

            while (hi - lo > 1)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(selector(source[mid]), key);

                if (found == 0)
                {
                    lo = hi = mid;
                    break;
                }
                if (found >= 1)
                {
                    hi = mid;
                }
                else
                {
                    lo = mid;
                }
            }

            return selectLower ? lo : hi;
        }

        // default lo = 0, hi = array.Count
        public static int LowerBound<T, TKey>(IList<T> source, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(key, selector(source[mid]));

                if (found <= 0)
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            var index = lo;
            if (index == -1 || source.Count <= index)
            {
                return -1;
            }

            // check final
            return (comparer.Compare(key, selector(source[index])) == 0)
                ? index
                : -1;
        }

        public static int UpperBound<T, TKey>(IList<T> source, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(key, selector(source[mid]));

                if (found >= 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }

            var index = (lo == 0) ? 0 : lo - 1;
            if (index == -1 || source.Count <= index)
            {
                return -1;
            }

            // check final
            return (comparer.Compare(key, selector(source[index])) == 0)
                ? index
                : -1;
        }


        //... want the lowest index of  Key <= Value
        //... returns 0 if key is <= all values in array
        //... returns array.Length if key is > all values in array

        public static int LowerBoundClosest<T, TKey>(IList<T> source, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(key, selector(source[mid]));

                if (found <= 0)     //... Key is <= value at mid
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;   //... Notice that lo starts at zero and can only increase
                }
            }

            var index = lo;         //... index will always be zero or greater

            if ( source.Count <= index)
            {
               return source.Count;
            }

            // check final
            return (comparer.Compare(key, selector(source[index])) <= 0)
                ? index
                : -1;
        }


        //... want the highest index of  Key >= Value
        //... returns -1 if key is < than all values in array
        //... returns array.Length - 1 if key is >= than all values in array

        public static int UpperBoundClosest<T, TKey>(IList<T> source, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(key, selector(source[mid]));

                if (found >= 0)     //... Key >= value at mid
                {
                    lo = mid + 1;   //... Note lo starts at zero and can only increase
                }
                else
                {
                    hi = mid;
                }
            }

            var index = (lo == 0) ? 0 : lo - 1;   //... index will always be zero or greater

            if ( index >= source.Count )
            {
               return source.Count;
            }

            // check final
            return (comparer.Compare(key, selector(source[index])) >= 0)
                ? index
                : -1;
        }



    }
}

namespace NeoCambion.Collections
{
    using JetBrains.Annotations;
    using System;
    using System.Collections.Generic;

    public class Grid2D<T>
    {
        public List<T> values = new List<T>();
        public List<List<int>> indsPxPy = new List<List<int>>();
        public List<List<int>> indsPxNy = new List<List<int>>();
        public List<List<int>> indsNxPy = new List<List<int>>();
        public List<List<int>> indsNxNy = new List<List<int>>();

        public string sizes
        {
            get
            {
                return "+X+Y: " + indsPxPy.Count + " | +X-Y: " + indsPxNy.Count + " | -X+Y: " + indsNxPy.Count + " | -X-Y: " + indsNxNy.Count;
            }
        }

        public T this[int x, int y]
        {
            get
            {
                int ind = GetOrSetIndex(x, y);
                return ind >= 0 ? values[ind] : default;
            }
            set
            {
                int ind = GetOrSetIndex(x, y);
                if (ind >= 0)
                {
                    values[ind] = value;
                }
                else
                {
                    values.Add(value);
                    GetOrSetIndex(x, y, values.Count - 1);
                }
            }
        }

        private int GetOrSetIndex(int x, int y, int newInd = -1)
        {
            if (x >= 0)
            {
                if (y >= 0)
                {
                    if (indsPxPy.Count <= x)
                        indsPxPy.PadList(x - indsPxPy.Count + 1, null);
                    if (indsPxPy[x] == null)
                        indsPxPy[x] = new List<int>();
                    if (indsPxPy[x].Count <= y)
                        indsPxPy[x].PadList(y - indsPxPy[x].Count + 1, -1);
                    if (newInd >= 0)
                        indsPxPy[x][y] = newInd;

                    return indsPxPy[x][y];
                }
                else
                {
                    int y_ = -y - 1;
                    if (indsPxNy.Count <= x)
                        indsPxNy.PadList(x - indsPxNy.Count + 1, null);
                    if (indsPxNy[x] == null)
                        indsPxNy[x] = new List<int>();
                    if (indsPxNy[x].Count <= y_)
                        indsPxNy[x].PadList(y_ - indsPxNy[x].Count + 1, -1);
                    if (newInd >= 0)
                        indsPxNy[x][y_] = newInd;

                    return indsPxNy[x][y_];
                }
            }
            else
            {
                if (y >= 0)
                {
                    int x_ = -x - 1;
                    if (indsNxPy.Count <= x_)
                        indsNxPy.PadList(x_ - indsNxPy.Count + 1, null);
                    if (indsNxPy[x_] == null)
                        indsNxPy[x_] = new List<int>();
                    if (indsNxPy[x_].Count <= y)
                        indsNxPy[x_].PadList(y - indsNxPy[x_].Count + 1, -1);
                    if (newInd >= 0)
                        indsNxPy[x_][y] = newInd;

                    return indsNxPy[x_][y];
                }
                else
                {
                    int x_ = -x - 1;
                    int y_ = -y - 1;
                    if (indsNxNy.Count <= x_)
                        indsNxNy.PadList(x_ - indsNxNy.Count + 1, null);
                    if (indsNxNy[x_] == null)
                        indsNxNy[x_] = new List<int>();
                    if (indsNxNy[x_].Count <= y_)
                        indsNxNy[x_].PadList(y_ - indsNxNy[x_].Count + 1, -1);
                    if (newInd >= 0)
                        indsNxNy[x_][y_] = newInd;

                    return indsNxNy[x_][y_];
                }
            }
        }

        public List<T> GetValues()
        {
            return values;
        }

        public List<List<int>>[] GetGridArrangement()
        {
            return new List<List<int>>[] { indsPxPy, indsPxNy, indsNxPy, indsNxNy };
        }

        public bool SetData(List<T> values, List<List<int>>[] gridArrangement)
        {
            if (gridArrangement.Length != 4)
            {
                return false;
            }
            else
            {
                this.values = values;
                indsPxPy = gridArrangement[0];
                indsPxNy = gridArrangement[1];
                indsNxPy = gridArrangement[2];
                indsNxNy = gridArrangement[3];
                return true;
            }
        }

        public void Clear()
        {
            values.Clear();
            indsPxPy.Clear();
            indsPxNy.Clear();
            indsNxPy.Clear();
            indsNxNy.Clear();
        }
    }

    public static class Ext_Collections
    {
        #region [ BOUNDS CHECKING / FIXING ]

        public static bool InBounds<T>(this int index, T[] array)
        {
            if (index > -1 && index < array.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool InBounds<T>(this T[] array, int index)
        {
            if (index > -1 && index < array.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool InBounds<T>(this int index, List<T> list)
        {
            if (index > -1 && index < list.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool InBounds<T>(this List<T> list, int index)
        {
            if (index > -1 && index < list.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static T[] PadArray<T>(this T[] array, int padLength = 1, T padValue = default, bool padAtFront = false)
        {
            int paddedLength = array.Length + padLength;
            T[] newArray = new T[paddedLength];
            if (!padAtFront)
            {
                for(int i = 0; i < paddedLength; i++)
                {
                    if (i < array.Length)
                    {
                        newArray[i] = array[i];
                    }
                    else
                    {
                        newArray[i] = padValue;
                    }
                }
            }
            else
            {
                for (int i = 0; i < paddedLength; i++)
                {
                    if (i < padLength)
                    {
                        newArray[i] = padValue;
                    }
                    else
                    {
                        newArray[i] = array[i - padLength];
                    }
                }
            }
            array = newArray;
            return newArray;
        }
            
        public static List<T> PadList<T>(this List<T> list, int padCount = 1, T padValue = default, bool padAtFront = false)
        {

            if (!padAtFront)
            {
                for(int i = 0; i < padCount; i++)
                {
                    list.Add(padValue);
                }
            }
            else
            {
                for (int i = 0; i < padCount; i++)
                {
                    list.Insert(0, padValue);
                }
            }
            return list;
        }

        public static T[] TrimArray<T>(this T[] array, int trimTo)
        {
            if (array.Length > trimTo)
            {
                T[] temp = new T[trimTo];
                for (int i = 0; i < trimTo; i++)
                {
                    temp[i] = array[i];
                }
                array = temp;
            }
            return array;
        }

        #endregion

        #region [ TYPE CONVERSION ]

        public static List<T> ToList<T>(this T[] array)
        {
            List<T> listOut = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                listOut.Add(array[i]);
            }
            return listOut;
        }

        public static T[] ToArray<T>(this List<T> list)
        {
            T[] arrayOut = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                arrayOut[i] = list[i];
            }
            return arrayOut;
        }

        #endregion

        #region [ DATA COPYING ]

        public static void CopyListData<T>(List<T> source, List<T> destination)
        {
            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
        }

        public static T[] CopyTo<T>(this T[] source, T[] destination)
        {
            for (int i = 0; i < source.Length && i < destination.Length; i++)
            {
                destination[i] = source[i];
            }
            return destination;
        }

        public static List<T> CopyTo<T>(this List<T> source, List<T> destination)
        {
            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
            return destination;
        }

        public static T[] CopyFrom<T>(this T[] destination, T[] source)
        {
            for (int i = 0; i < source.Length && i < destination.Length; i++)
            {
                destination[i] = source[i];
            }
            return destination;
        }

        public static List<T> CopyFrom<T>(this List<T> destination, List<T> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
            return destination;
        }

        #endregion

        #region [ SEARCHING ]

        public static bool SubListContains<T>(this List<List<T>> listOfLists, T item)
        {
            foreach (List<T> subList in listOfLists)
            {
                if (subList.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static int IndexOf<T>(this T[] array, T item, int searchFromIndex = 0)
        {
            if (searchFromIndex < 0 || searchFromIndex >= array.Length)
                searchFromIndex = 0;
            for (int i = searchFromIndex; i < array.Length; i++)
            {
                if (array[i].Equals(item))
                    return i;
            }
            return -1;
        }

        public static List<int> IndicesOf<T>(this List<T> list, T item)
        {
            List<int> output = new List<int>();
            int lastIndex = -1;
            bool contSearch = true;
            while (contSearch)
            {
                int n = list.IndexOf(item, lastIndex + 1);
                if (n == -1)
                {
                    contSearch = false;

                    break;
                }
                else
                {
                    output.Add(n);
                    lastIndex = n;
                }
            }
            return output;
        }

        public static List<int[]> IndicesOf<T>(this List<List<T>> listOfLists, T item)
        {
            List<int[]> output = new List<int[]>();

            for (int i = 0; i < listOfLists.Count; i++)
            {
                List<int> indices = listOfLists[i].IndicesOf(item);
                if (indices.Count > 0)
                {
                    for (int j = 0; j < indices.Count; j++)
                    {
                        output.Add(new int[] { i, indices[j] });
                    }
                }
            }

            return output;
        }

        public static T[] WrappedAscendingRange<T>(this T[] array, int startInclusive, int endInclusive)
        {
            if (startInclusive >= array.Length)
                startInclusive = array.Length - 1;
            if (endInclusive >= array.Length)
                endInclusive = array.Length - 1;
            if (startInclusive < 0)
                startInclusive = 0;
            if (endInclusive < 0)
                endInclusive = 0;

            T[] arrayOut = new T[startInclusive > endInclusive ? array.Length - startInclusive + endInclusive + 1 : endInclusive - startInclusive + 1];

            int n = 0;
            if (startInclusive == endInclusive)
            {
                arrayOut[n] = array[startInclusive];
            }
            else if (startInclusive < endInclusive)
            {
                for (int i = startInclusive; i <= endInclusive; i++)
                {
                    arrayOut[n] = array[i];
                    n++;
                }
            }
            else
            {
                for (int i = startInclusive; i < array.Length; i++)
                {
                    arrayOut[n] = array[i];
                    n++;
                }
                for (int i = 0; i <= endInclusive; i++)
                {
                    arrayOut[n] = array[i];
                    n++;
                }
            }

            return arrayOut;
        }

        public static List<T> WrappedAscendingRange<T>(this List<T> list, int startInclusive, int endInclusive)
        {
            if (startInclusive >= list.Count)
                startInclusive = list.Count - 1;
            if (endInclusive >= list.Count)
                endInclusive = list.Count - 1;
            if (startInclusive < 0)
                startInclusive = 0;
            if (endInclusive < 0)
                endInclusive = 0;

            List<T> listOut = new List<T>();

            if (startInclusive == endInclusive)
            {
                listOut.Add(list[startInclusive]);
            }
            else if (startInclusive < endInclusive)
            {
                for (int i = startInclusive; i <= endInclusive; i++)
                {
                    listOut.Add(list[i]);
                }
            }
            else
            {
                for (int i = startInclusive; i < list.Count; i++)
                {
                    listOut.Add(list[i]);
                }
                for (int i = 0; i <= endInclusive; i++)
                {
                    listOut.Add(list[i]);
                }
            }

            return listOut;
        }

        #endregion

        #region [ RANDOMISATION ]

        public static T Pick<T>(this List<T> itemList)
        {
            Random rand = new Random();
            if (itemList.Count > 0)
            {
                int n = rand.Next(0, itemList.Count);
                return itemList[n];
            }
            else
            {
                return default;
            }
        }

        public static T[] Shuffle<T>(this T[] array)
        {
            Random rand = new Random();
            int r;
            for (int i = array.Length - 1; i > 0; i--)
            {
                r = rand.Next(0, i + 1);
                (array[i], array[r]) = (array[r], array[i]);
            }
            return array;
        }

        public static List<T> Shuffle<T>(this List<T> list)
        {
            Random rand = new Random();
            int r;
            for (int i = list.Count - 1; i > 0; i--)
            {
                r = rand.Next(0, i + 1);
                (list[i], list[r]) = (list[r], list[i]);
            }
            return list;
        }

        #endregion

        #region [ FIRST / LAST ]

        public static T First<T>(this T[] array)
        {
            if (array.Length > 0)
            {
                return array[0];
            }
            else
            {
                return default;
            }
        }
            
        public static T First<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return default;
            }
        }
            
        public static T Last<T>(this T[] array)
        {
            if (array.Length > 0)
            {
                return array[array.Length - 1];
            }
            else
            {
                return default;
            }
        }
            
        public static T Last<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                return list[list.Count - 1];
            }
            else
            {
                return default;
            }
        }

        #endregion

        #region [ INDEX OSCILLATION ]

        public static int OscillateIndexOutward(this int collectionSize, int indexIn)
        {
            if (indexIn >= 0 && indexIn < collectionSize)
            {
                int indStart;
                if (collectionSize % 2 == 0)
                {
                    indStart = collectionSize / 2 - 1;
                }
                else
                {
                    indStart = (collectionSize - 1) / 2;
                }

                indexIn++;
                int mod = (indexIn - (indexIn % 2)) / 2;

                return indStart + (mod * (indexIn % 2 == 0 ? 1 : -1));
            }
            else
            {
                throw new IndexOutOfRangeException("Unable to convert index outside the range of the collection!");
            }
        }
            
        public static int OscillateIndexOutward<T>(this T[] array, int indexIn)
        {
            if (array.InBounds(indexIn))
            {
                int indStart;
                if (array.Length % 2 == 0)
                {
                    indStart = array.Length / 2 - 1;
                }
                else
                {
                    indStart = (array.Length - 1) / 2;
                }

                indexIn++;
                int mod = (indexIn - (indexIn % 2)) / 2;

                return indStart + (mod * (indexIn % 2 == 0 ? 1 : -1));
            }
            else
            {
                throw new IndexOutOfRangeException("Unable to convert index outside the range of the collection!");
            }
        }

        public static int OscillateIndexOutward<T>(this List<T> list, int indexIn)
        {
            if (list.InBounds(indexIn))
            {
                int indStart;
                if (list.Count % 2 == 0)
                {
                    indStart = list.Count / 2 - 1;
                }
                else
                {
                    indStart = (list.Count - 1) / 2;
                }

                indexIn++;
                int mod = (indexIn - (indexIn % 2)) / 2;

                return indStart + (mod * (indexIn % 2 == 0 ? 1 : -1));
            }
            else
            {
                throw new IndexOutOfRangeException("Unable to convert index outside the range of the collection!");
            }
        }

        public static int OscillateIndexInward(this int collectionSize, int indexIn)
        {
            return collectionSize - OscillateIndexOutward(collectionSize, indexIn) - 1;
        }
            
        public static int OscillateIndexInward<T>(this T[] array, int indexIn)
        {
            return array.Length - OscillateIndexOutward(array, indexIn) - 1;
        }

        public static int OscillateIndexInward<T>(this List<T> list, int indexIn)
        {
            return list.Count - OscillateIndexOutward(list, indexIn) - 1;
        }

        #endregion

        public static void Transfer<T>(this List<T> source, int sourceIndex, List<T> destination, int destinationIndex = -1)
        {
            if (destinationIndex < 0)
            {
                destination.Add(source[sourceIndex]);
            }
            else
            {
                destination.Insert(destinationIndex, source[sourceIndex]);
            }
            source.RemoveAt(sourceIndex);
        }

        public static void Clear<T>(this T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = default(T);
            }
        }

        public static void Clear<T>(this T[,] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array.Length; j++)
                {
                    array[i,j] = default(T);
                }
            }
        }

        public static void Clear<T>(this T[,,] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array.Length; j++)
                {
                    for (int k = 0; k < array.Length; k++)
                    {
                        array[i,j,k] = default(T);
                    }
                }
            }
        }

        public static int Count<T> (this List<T>[] array)
        {
            int n = 0;
            foreach (List<T> list in array)
            {
                n += list.Count;
            }
            return n;
        }
    }

    public static class Ext_FloatCollection
    {
        public static int IndexOfSmallest(this float[] arr)
        {
            float s = float.MaxValue;
            int sInd = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < s)
                {
                    s = arr[i];
                    sInd = i;
                }
            }
            return sInd;
        }

        public static int IndexOfSmallest(this List<float> list)
        {
            float s = float.MaxValue;
            int sInd = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < s)
                {
                    s = list[i];
                    sInd = i;
                }
            }
            return sInd;
        }

        public static int IndexOfLargest(this float[] arr)
        {
            float l = float.MinValue;
            int lInd = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > l)
                {
                    l = arr[i];
                    lInd = i;
                }
            }
            return lInd;
        }

        public static int IndexOfLargest(this List<float> list)
        {
            float l = float.MinValue;
            int lInd = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > l)
                {
                    l = list[i];
                    lInd = i;
                }
            }
            return lInd;
        }

        public static int FirstLessThan(this float[] arr, float value, bool allowEqualTo = false)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < value || (allowEqualTo && arr[i] == value))
                    return i;
            }
            return -1;
        }

        public static int FirstLessThan(this List<float> list, float value, bool allowEqualTo = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < value || (allowEqualTo && list[i] == value))
                    return i;
            }
            return -1;
        }

        public static int FirstGreaterThan(this float[] arr, float value, bool allowEqualTo = false)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > value || (allowEqualTo && arr[i] == value))
                    return i;
            }
            return -1;
        }

        public static int FirstGreaterThan(this List<float> list, float value, bool allowEqualTo = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > value || (allowEqualTo && list[i] == value))
                    return i;
            }
            return -1;
        }
    }

    public static class Ext_IntCollection
    {
        public static int[] IncrementalPopulate(this int[] arr, int startValue = 0, int step = 1)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = startValue + step * i;

            return arr;
        }

        public static List<int> IncrementalPopulate(this List<int> list, int startValue = 0, int step = 1, int count = 8, bool overwrite = true)
        {
            if (overwrite)
            {
                if (list.Count > 0)
                    count = list.Count;
                else if (count < 0)
                    count = 8;

                list.Clear();
            }
            else
            {
                if (count < 0)
                    count = 8;
            }

            for (int i = 0; i < count; i++)
                list.Add(startValue + step * i);

            return list;
        }
        
        public static int[] IncrementalShuffle(this int[] arr, int startValue = 0, int step = 1)
        {
            return arr.IncrementalPopulate(startValue, step).Shuffle();
        }
        
        public static List<int> IncrementalShuffle(this List<int> list, int startValue = 0, int step = 1, int count = 8, bool overwrite = true)
        {
            return list.IncrementalPopulate(startValue, step, count, overwrite).Shuffle();
        }

        public static int IndexOfSmallest(this int[] arr)
        {
            int s = int.MaxValue;
            int sInd = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < s)
                {
                    s = arr[i];
                    sInd = i;
                }
            }
            return sInd;
        }

        public static int IndexOfSmallest(this List<int> list)
        {
            int s = int.MaxValue;
            int sInd = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < s)
                {
                    s = list[i];
                    sInd = i;
                }
            }
            return sInd;
        }

        public static int IndexOfLargest(this int[] arr)
        {
            int l = int.MinValue;
            int lInd = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > l)
                {
                    l = arr[i];
                    lInd = i;
                }
            }
            return lInd;
        }

        public static int IndexOfLargest(this List<int> list)
        {
            int l = int.MinValue;
            int lInd = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > l)
                {
                    l = list[i];
                    lInd = i;
                }
            }
            return lInd;
        }

        public static int FirstLessThan(this int[] arr, int value, bool allowEqualTo = false)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < value || (allowEqualTo && arr[i] == value))
                    return i;
            }
            return -1;
        }

        public static int FirstLessThan(this List<int> list, int value, bool allowEqualTo = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < value || (allowEqualTo && list[i] == value))
                    return i;
            }
            return -1;
        }

        public static int FirstGreaterThan(this int[] arr, int value, bool allowEqualTo = false)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > value || (allowEqualTo && arr[i] == value))
                    return i;
            }
            return -1;
        }

        public static int FirstGreaterThan(this List<int> list, int value, bool allowEqualTo = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > value || (allowEqualTo && list[i] == value))
                    return i;
            }
            return -1;
        }
    }

    public static class Ext_StringCollection
    {
        public static List<bool> Contains(this List<string> strings, string value)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Contains(value));
            }
            return output;
        }
            
        public static List<bool> EndsWith(this List<string> strings, string value, StringComparison comparisonType)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].EndsWith(value, comparisonType));
            }
            return output;
        }
            
        public static List<bool> EndsWith(this List<string> strings, string value, bool ignoreCase, System.Globalization.CultureInfo culture)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].EndsWith(value, ignoreCase, culture));
            }
            return output;
        }
            
        public static List<bool> EndsWith(this List<string> strings, string value)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].EndsWith(value));
            }
            return output;
        }

        public static List<int> IndexOf(this List<string> strings, string value, int startIndex, StringComparison comparisonType)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value, startIndex, comparisonType));
            }
            return output;
        }
            
        public static List<int> IndexOf(this List<string> strings, string value, StringComparison comparisonType)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value, comparisonType));
            }
            return output;
        }
            
        public static List<int> IndexOf(this List<string> strings, string value, int startIndex, int count)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value, startIndex, count));
            }
            return output;
        }
            
        public static List<int> IndexOf(this List<string> strings, string value)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value));
            }
            return output;
        }
            
        public static List<int> IndexOf(this List<string> strings, char value, int startIndex, int count)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value, startIndex, count));
            }
            return output;
        }
            
        public static List<int> IndexOf(this List<string> strings, char value, int startIndex)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value, startIndex));
            }
            return output;
        }
            
        public static List<int> IndexOf(this List<string> strings, char value)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value));
            }
            return output;
        }
            
        public static List<int> IndexOf(this List<string> strings, string value, int startIndex, int count, StringComparison comparisonType)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value, startIndex, count, comparisonType));
            }
            return output;
        }
            
        public static List<int> IndexOf(this List<string> strings, string value, int startIndex)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOf(value, startIndex));
            }
            return output;
        }

        public static List<int> IndexOfAny(this List<string> strings, char[] anyOf)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOfAny(anyOf));
            }
            return output;
        }
            
        public static List<int> IndexOfAny(this List<string> strings, char[] anyOf, int startIndex, int count)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOfAny(anyOf, startIndex, count));
            }
            return output;
        }
            
        public static List<int> IndexOfAny(this List<string> strings, char[] anyOf, int startIndex)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IndexOfAny(anyOf, startIndex));
            }
            return output;
        }

        public static List<string> Insert(this List<string> strings, int startIndex, string value)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Insert(startIndex, value));
            }
            return output;
        }

        public static List<bool> IsNormalized(this List<string> strings)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IsNormalized());
            }
            return output;
        }
            
        public static List<bool> IsNormalized(this List<string> strings, System.Text.NormalizationForm normalizationForm)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].IsNormalized(normalizationForm));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, string value, int startIndex, StringComparison comparisonType)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value, startIndex, comparisonType));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, string value, StringComparison comparisonType)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value, comparisonType));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, string value, int startIndex, int count)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value, startIndex, count));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, string value)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, char value, int startIndex, int count)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value, startIndex, count));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, char value, int startIndex)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value, startIndex));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, char value)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, string value, int startIndex, int count, StringComparison comparisonType)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value, startIndex, count, comparisonType));
            }
            return output;
        }

        public static List<int> LastIndexOf(this List<string> strings, string value, int startIndex)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOf(value, startIndex));
            }
            return output;
        }

        public static List<int> LastIndexOfAny(this List<string> strings, char[] anyOf)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOfAny(anyOf));
            }
            return output;
        }

        public static List<int> LastIndexOfAny(this List<string> strings, char[] anyOf, int startIndex, int count)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOfAny(anyOf, startIndex, count));
            }
            return output;
        }

        public static List<int> LastIndexOfAny(this List<string> strings, char[] anyOf, int startIndex)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].LastIndexOfAny(anyOf, startIndex));
            }
            return output;
        }

        public static List<string> Normalize(this List<string> strings)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Normalize());
            }
            return output;
        }

        public static List<string> Normalize(this List<string> strings, System.Text.NormalizationForm normalizationForm)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Normalize(normalizationForm));
            }
            return output;
        }

        public static List<string> PadLeft(this List<string> strings, int totalWidth)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].PadLeft(totalWidth));
            }
            return output;
        }
            
        public static List<string> PadLeft(this List<string> strings, int totalWidth, char paddingChar)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].PadLeft(totalWidth, paddingChar));
            }
            return output;
        }
            
        public static List<string> PadRight(this List<string> strings, int totalWidth)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].PadRight(totalWidth));
            }
            return output;
        }
            
        public static List<string> PadRight(this List<string> strings, int totalWidth, char paddingChar)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].PadRight(totalWidth, paddingChar));
            }
            return output;
        }
            
        public static List<string> Remove(this List<string> strings, int startIndex)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Remove(startIndex));
            }
            return output;
        }
            
        public static List<string> Remove(this List<string> strings, int startIndex, int count)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Remove(startIndex, count));
            }
            return output;
        }
            
        public static List<string> Replace(this List<string> strings, string oldValue, string newValue)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Replace(oldValue, newValue));
            }
            return output;
        }
            
        public static List<string> Replace(this List<string> strings, char oldChar, char newChar)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Replace(oldChar, newChar));
            }
            return output;
        }

        public static List<string[]> Split(this List<string> strings, string[] separator, int count, StringSplitOptions option)
        {
            List<string[]> output = new List<string[]>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Split(separator, count, option));
            }
            return output;
        }
            
        public static List<string[]> Split(this List<string> strings, params char[] separator)
        {
            List<string[]> output = new List<string[]>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Split(separator));
            }
            return output;
        }
            
        public static List<string[]> Split(this List<string> strings, char[] separator, int count)
        {
            List<string[]> output = new List<string[]>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Split(separator, count));
            }
            return output;
        }
            
        public static List<string[]> Split(this List<string> strings, char[] separator, int count, StringSplitOptions options)
        {
            List<string[]> output = new List<string[]>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Split(separator, count, options));
            }
            return output;
        }
            
        public static List<string[]> Split(this List<string> strings, char[] separator, StringSplitOptions options)
        {
            List<string[]> output = new List<string[]>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Split(separator, options));
            }
            return output;
        }
            
        public static List<string[]> Split(this List<string> strings, string[] separator, StringSplitOptions options)
        {
            List<string[]> output = new List<string[]>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Split(separator, options));
            }
            return output;
        }

        public static List<bool> StartsWith(this List<string> strings, string value, StringComparison comparisonType)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].StartsWith(value, comparisonType));
            }
            return output;
        }

        public static List<bool> StartsWith(this List<string> strings, string value, bool ignoreCase, System.Globalization.CultureInfo culture)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].StartsWith(value, ignoreCase, culture));
            }
            return output;
        }

        public static List<bool> StartsWith(this List<string> strings, string value)
        {
            List<bool> output = new List<bool>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].StartsWith(value));
            }
            return output;
        }

        public static List<string> Substring(this List<string> strings, int startIndex)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Substring(startIndex));
            }
            return output;
        }

        public static List<string> Substring(this List<string> strings, int startIndex, int length)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Substring(startIndex, length));
            }
            return output;
        }

        public static List<char[]> ToCharArray(this List<string> strings, int startIndex, int length)
        {
            List<char[]> output = new List<char[]>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].ToCharArray(startIndex, length));
            }
            return output;
        }
            
        public static List<char[]> ToCharArray(this List<string> strings)
        {
            List<char[]> output = new List<char[]>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].ToCharArray());
            }
            return output;
        }

        public static List<string> ToLower(this List<string> strings)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].ToLower());
            }
            return output;
        }

        public static List<string> ToLower(this List<string> strings, System.Globalization.CultureInfo culture)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].ToLower(culture));
            }
            return output;
        }

        public static List<string> ToLowerInvariant(this List<string> strings)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].ToLowerInvariant());
            }
            return output;
        }
            
        public static List<string> ToUpper(this List<string> strings)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].ToUpper());
            }
            return output;
        }

        public static List<string> ToUpper(this List<string> strings, System.Globalization.CultureInfo culture)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].ToUpper(culture));
            }
            return output;
        }

        public static List<string> ToUpperInvariant(this List<string> strings)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].ToUpperInvariant());
            }
            return output;
        }

        public static List<string> Trim(this List<string> strings)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Trim());
            }
            return output;
        }

        public static List<string> Trim(this List<string> strings, params char[] trimChars)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].Trim(trimChars));
            }
            return output;
        }
            
        public static List<string> TrimEnd(this List<string> strings, params char[] trimChars)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].TrimEnd(trimChars));
            }
            return output;
        }
            
        public static List<string> TrimStart(this List<string> strings, params char[] trimChars)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < strings.Count; i++)
            {
                output.Add(strings[i].TrimStart(trimChars));
            }
            return output;
        }
    }

    namespace Unity
    {
        using UnityEngine;

        public static class Ext_RectCollection
        {
            public static bool Contains(this Rect[] array, Vector2 point)
            {
                foreach (Rect rect in array)
                {
                    if (rect.Contains(point))
                        return true;
                }
                return false;
            }

            public static bool Contains(this List<Rect> list, Vector2 point)
            {
                foreach (Rect rect in list)
                {
                    if (rect.Contains(point))
                        return true;
                }
                return false;
            }
        }

        public static class Ext_Vector2Collection
        {
            public static Vector2[] Offset(this Vector2[] array, Vector2 offset)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] += offset;
                }
                return array;
            }

            public static List<Vector2> Offset(this List<Vector2> list, Vector2 offset)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] += offset;
                }
                return list;
            }
        }

        public static class Ext_Vector3Collection
        {
            public static Vector3[] Offset(this Vector3[] array, Vector3 offset)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] += offset;
                }
                return array;
            }

            public static List<Vector3> Offset(this List<Vector3> list, Vector3 offset)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] += offset;
                }
                return list;
            }
        }
    }
}
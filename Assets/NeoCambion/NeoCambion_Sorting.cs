namespace NeoCambion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    namespace Sorting
    {
        public static class Ext_Collections
        {
            public static int[] InsertionSort(this int[] array, bool ascending = true)
            {
                int n = array.Length;
                int temp;

                if (ascending)
                {
                    for (int i = 1; i < n; i++)
                    {
                        int j = i;
                        temp = array[j];
                        while (j > 0 && array[j - 1] > temp)
                        {
                            array[j] = array[j - 1];
                            j--;
                        }
                        array[j] = temp;
                    }
                }
                else
                {
                    for (int i = n - 2; i >= 0; i--)
                    {
                        int j = i;
                        temp = array[j];
                        while (j < n - 1 && array[j + 1] > temp)
                        {
                            array[j] = array[j + 1];
                            j++;
                        }
                        array[j] = temp;
                    }
                }

                return array;
            }
            
            public static int[,] InsertionSort(this int[,] array2D, bool ascending = true)
            {
                int n = array2D.GetLength(0);
                int[] temp = new int[array2D.GetLength(1)];
                int rowBytes = sizeof(int) * array2D.GetLength(1);

                if (ascending)
                {
                    for (int i = 1; i < n; i++)
                    {
                        int j = i;
                        Buffer.BlockCopy(array2D, rowBytes * i, temp, 0, rowBytes); ;
                        while (j > 0 && array2D[j - 1, 0] > temp[0])
                        {
                            Buffer.BlockCopy(array2D, rowBytes * (j - 1), array2D, rowBytes * j, rowBytes);
                            j--;
                        }
                        Buffer.BlockCopy(temp, 0, array2D, rowBytes * j, rowBytes);
                    }
                }
                else
                {
                    for (int i = n - 2; i >= 0; i--)
                    {
                        int j = i;
                        Buffer.BlockCopy(array2D, rowBytes * i, temp, 0, rowBytes); ;
                        while (j > 0 && array2D[j + 1, 0] > temp[0])
                        {
                            Buffer.BlockCopy(array2D, rowBytes * (j + 1), array2D, rowBytes * j, rowBytes);
                            j++;
                        }
                        Buffer.BlockCopy(temp, 0, array2D, rowBytes * j, rowBytes);
                    }
                }

                return array2D;
            }

            public static List<int> InsertionSort(this List<int> list, bool ascending = true)
            {
                int n = list.Count;
                int temp;

                if (ascending)
                {
                    for (int i = 1; i < n; i++)
                    {
                        int j = i;
                        temp = list[j];
                        while (j > 0 && list[j - 1] > temp)
                        {
                            list[j] = list[j - 1];
                            j--;
                        }
                        list[j] = temp;
                    }
                }
                else
                {
                    for (int i = n - 2; i >= 0; i--)
                    {
                        int j = i;
                        temp = list[j];
                        while (j < n - 1 && list[j + 1] > temp)
                        {
                            list[j] = list[j + 1];
                            j++;
                        }
                        list[j] = temp;
                    }
                }

                return list;
            }

            public static int[] ShellSort(this int[] array/*, bool ascending = true*/)
            {
                int n = array.Length;
                int k = 0, i, j, interv, target;

                // Papernov & Stasevich, 1965
                // https://en.wikipedia.org/wiki/Shellsort
                int interval(int kVal)
                {
                    return 2 * (int)(n / Mathf.Pow(2, kVal + 1)) + 1;
                }

                do
                {
                    interv = interval(k++);
                    for (i = 1; i < n; i++)
                    {
                        // Insertion sort of sub-arrays
                        j = i;
                        target = array[i];
                        while (j >= interv && array[j - interv] > target)
                        {
                            array[j] = array[j - interv];
                            j -= interv;
                        }
                        array[j] = target;
                    }
                } while (interv > 1);

                return array;
            }

            public static int[,] ShellSort(this int[,] array2D/*, bool ascending = true*/)
            {
                int n = array2D.GetLength(0);
                int k = 0, i, j, interv;
                int rowBytes = sizeof(int) * array2D.GetLength(1);
                int[] target = new int[array2D.GetLength(1)];

                // Papernov & Stasevich, 1965
                // https://en.wikipedia.org/wiki/Shellsort
                int interval(int kVal)
                {
                    return 2 * (int)(n / Mathf.Pow(2, kVal + 1)) + 1;
                }

                do
                {
                    interv = interval(k++);
                    for (i = 1; i < n; i++)
                    {
                        // Insertion sort of sub-arrays
                        j = i;
                        Buffer.BlockCopy(array2D, rowBytes * i , target, 0, rowBytes);
                        while (j >= interv && array2D[j - interv, 0] > target[0])
                        {
                            Buffer.BlockCopy(array2D, rowBytes * (j - interv), array2D, rowBytes * j, rowBytes);
                            j -= interv;
                        }
                        Buffer.BlockCopy(target, 0, array2D, rowBytes * j, rowBytes);
                    }
                } while (interv > 1);

                return array2D;
            }

            public static List<int> ShellSort(this List<int> list/*, bool ascending = true*/)
            {
                int n = list.Count;
                int k = 0, i, j, interv, target;

                // Papernov & Stasevich, 1965
                // https://en.wikipedia.org/wiki/Shellsort
                int interval(int kVal)
                {
                    return 2 * (int)(n / Mathf.Pow(2, kVal + 1)) + 1;
                }

                do
                {
                    interv = interval(k++);
                    for (i = 1; i < n; i++)
                    {
                        // Insertion sort of sub-arrays
                        j = i;
                        target = list[i];
                        while (j >= interv && list[j - interv] > target)
                        {
                            list[j] = list[j - interv];
                            j -= interv;
                        }
                        list[j] = target;
                    }
                } while (interv > 1);

                return list;
            }
        }
    }
}
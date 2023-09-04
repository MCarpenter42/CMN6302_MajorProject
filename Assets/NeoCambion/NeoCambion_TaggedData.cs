namespace NeoCambion
{

    namespace TaggedData
    {
        using System;
        using System.Text;
        using System.Collections;
        using System.Collections.Generic;

        public static class TaggedDataUtility
        {
            public static byte[] SetLength(this byte[] arrIn, int length)
            {
                byte[] arrOut = new byte[length];
                if (arrIn.Length > length)
                {
                    for (int i = 0; i < length; i++)
                        arrOut[i] = arrIn[i];
                }
                else if (arrIn.Length < length)
                {
                    for (int i = 0; i < arrIn.Length; i++)
                        arrOut[i] = arrIn[i];
                }
                else
                {
                    arrOut = arrIn;
                }
                return arrOut;
            }

            public static string ToText(this long tag)
            {
                return Encoding.ASCII.GetString(tag.ToBytes());
            }

            #region [ VALUE COLLECTION RETRIEVAL ]

            public static T[] GetValues<T>(this Data_LongTag<T>[] array)
            {
                T[] arrayOut = new T[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    arrayOut[i] = array[i].value;
                }
                return arrayOut;
            }

            public static List<T> GetValues<T>(this List<Data_LongTag<T>> list)
            {
                List<T> listOut = new List<T>();
                for (int i = 0; i < list.Count; i++)
                {
                    listOut.Add(list[i].value);
                }
                return listOut;
            }
            
            public static T[] GetValues<T>(this Data_IntTag<T>[] array)
            {
                T[] arrayOut = new T[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    arrayOut[i] = array[i].value;
                }
                return arrayOut;
            }

            public static List<T> GetValues<T>(this List<Data_IntTag<T>> list)
            {
                List<T> listOut = new List<T>();
                for (int i = 0; i < list.Count; i++)
                {
                    listOut.Add(list[i].value);
                }
                return listOut;
            }
            
            public static T[] GetValues<T>(this Data_StringTag<T>[] array)
            {
                T[] arrayOut = new T[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    arrayOut[i] = array[i].value;
                }
                return arrayOut;
            }

            public static List<T> GetValues<T>(this List<Data_StringTag<T>> list)
            {
                List<T> listOut = new List<T>();
                for (int i = 0; i < list.Count; i++)
                {
                    listOut.Add(list[i].value);
                }
                return listOut;
            }

            #endregion
        }

        [Serializable]
        public struct Data_LongTag<T>
        {
            public long tag;
            public T value;
            public override string ToString() => $"(#{tag}: {value})";

            public Data_LongTag(long tag, T value)
            {
                this.tag = tag;
                this.value = value;
            }
        }

        [Serializable]
        public struct Data_IntTag<T>
        {
            public int tag;
            public T value;
            public override string ToString() => $"(#{tag}: {value})";

            public Data_IntTag(int tag, T value)
            {
                this.tag = tag;
                this.value = value;
            }
        }

        [Serializable]
        public struct Data_StringTag<T>
        {
            private Data_LongTag<T> data;
            public string tagText { get { return data.tag.ToText(); } }
            public T value { get { return data.value; } }
            public override string ToString() => $"(#{tagText}: {value})";

            public Data_StringTag(string tag, T value)
            {
                if (tag.Length > 8)
                    tag = tag.Substring(0, 8);
                byte[] tagBytes = Encoding.ASCII.GetBytes(tag);
                data = new Data_LongTag<T>(tagBytes.SetLength(8).ToLong(), value);
            }
        }

        namespace Unity
        {
            using UnityEngine;

            public static class TaggedDataUtility_Unity
            {
                #region [ VALUE COLLECTION RETRIEVAL ]

                public static T[] GetValues<T>(this Data_Vector2Tag<T>[] array)
                {
                    T[] arrayOut = new T[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        arrayOut[i] = array[i].value;
                    }
                    return arrayOut;
                }

                public static List<T> GetValues<T>(this List<Data_Vector2Tag<T>> list)
                {
                    List<T> listOut = new List<T>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        listOut.Add(list[i].value);
                    }
                    return listOut;
                }

                public static T[] GetValues<T>(this Data_Vector3Tag<T>[] array)
                {
                    T[] arrayOut = new T[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        arrayOut[i] = array[i].value;
                    }
                    return arrayOut;
                }

                public static List<T> GetValues<T>(this List<Data_Vector3Tag<T>> list)
                {
                    List<T> listOut = new List<T>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        listOut.Add(list[i].value);
                    }
                    return listOut;
                }

                #endregion
            }

            [Serializable]
            public struct Data_Vector2Tag<T>
            {
                public Vector2 tag;
                public T value;
                public override string ToString() => $"(#{tag}: {value})";

                public Data_Vector2Tag(Vector2 tag, T value)
                {
                    this.tag = tag;
                    this.value = value;
                }
            }

            [Serializable]
            public struct Data_Vector3Tag<T>
            {
                public Vector3 tag;
                public T value;
                public override string ToString() => $"(#{tag}: {value})";

                public Data_Vector3Tag(Vector3 tag, T value)
                {
                    this.tag = tag;
                    this.value = value;
                }
            }
        }
    }
}
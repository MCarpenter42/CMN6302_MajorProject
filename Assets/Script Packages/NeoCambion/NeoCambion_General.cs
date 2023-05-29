namespace NeoCambion
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #region [ ENUMERATION TYPES ]

    public enum Axis { X, Y, Z };
    public enum DualAxis { XY, XZ, YZ };
    public enum CompassBearing_Simple { North, East, South, West };
    public enum CompassBearing_Precision1
    {
        North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
    };
    public enum CompassBearing_Precision2
    {
        North, NorthNorthEast, NorthEast, EastNorthEast,
        East, EastSouthEast, SouthEast, SouthSouthEast,
        South, SouthSouthWest, SouthWest, WestSouthWest,
        West, WestNorthWest, NorthWest, NorthNorthWest
    };
    public enum RotDirection { Clockwise, CounterClockwise };

    public enum Condition_Number { Never, LessThan, LessThanOrEqualTo, EqualTo, GreaterThanOrEqualTo, GreaterThan, Always };
    public enum Condition_String { Never, Matches, DoesNotMatch, Contains, DoesNotContain, IsSubstring, IsNotSubstring, Always };

    public enum RectProperty { X, Y, Width, Height };

    #endregion

    public struct HexByte
    {
        public readonly byte byteValue;
        public readonly char[] charValues { get { return byteValue.ParseToHexChars(); } }
        public readonly string stringValue { get { return byteValue.ParseToHexString(); } }

        public HexByte(byte byteValue)
        {
            this.byteValue = byteValue;
        }

        public HexByte(char hex1, char hex0)
        {
            byteValue = Ext_Char.ParseHexToByte(hex1, hex0);
        }
    }

    public static class Ext_Byte
    {
        public static int ToInt(this byte[] bytes)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToInt32(bytes, 0) : BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);
        }

        public static long ToLong(this byte[] bytes)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToInt64(bytes, 0) : BitConverter.ToInt64(bytes.Reverse().ToArray(), 0);
        }

        public static short ToShort(this byte[] bytes)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToInt16(bytes, 0) : BitConverter.ToInt16(bytes.Reverse().ToArray(), 0);
        }

        public static uint ToUInt(this byte[] bytes)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToUInt32(bytes, 0) : BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
        }

        public static ulong ToULong(this byte[] bytes)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToUInt64(bytes, 0) : BitConverter.ToUInt64(bytes.Reverse().ToArray(), 0);
        }

        public static ushort ToUShort(this byte[] bytes)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToUInt16(bytes, 0) : BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
        }

        public static string ParseToString(this byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            string str = "";
            for (int i = 0; i < bytes.Length - (bytes.Length % 2); i += 2)
            {
                str += BitConverter.ToChar(new byte[] { bytes[i], bytes[i + 1] });
            }
            return str;
            //return BitConverter.IsLittleEndian ? BitConverter.ToString(bytes, 0) : BitConverter.ToString(bytes.Reverse().ToArray(), 0);
        }

        public static char[] ParseToHexChars(this byte byteVal)
        {
            byte hex0 = (byte)(byteVal % 0x10);
            byte hex1 = (byte)((byteVal - (byteVal % 0x10)) / 0x10);
            return new char[] { Ext_Char.Hexidecimal[hex1], Ext_Char.Hexidecimal[hex0] };
        }

        public static char[] ParseToHexChars(this byte[] bytes)
        {
            int n = bytes.Length - (bytes.Length % 2);
            char[] charsOut = new char[n];
            for (int i = 0; i < n; i += 2)
            {
                char[] chars = bytes[i].ParseToHexChars();
                charsOut[i] = chars[0];
                charsOut[i + 1] = chars[1];
            }
            return charsOut;
        }

        public static string ParseToHexString(this byte byteVal)
        {
            char[] chars = ParseToHexChars(byteVal);
            return "" + chars[0] + chars[1];
        }

        public static string ParseToHexString(this byte[] bytes)
        {
            int n = bytes.Length - (bytes.Length % 2);
            string strOut = "";
            for (int i = 0; i < n; i++)
            {
                strOut += bytes[i].ParseToHexString();
            }
            return strOut;
        }
    }

    public static class Ext_Char
    {
        public static char[] Hexidecimal = new char[]
        {
            '0', '1', '2', '3',
            '4', '5', '6', '7',
            '8', '9', 'A', 'B',
            'C', 'D', 'E', 'F',
        };

        public static char[] AlphaNumeric = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static char[] AlphaNumUnderscore = new char[]
        {
            '_', '-',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static byte[] ToBytes(this char charVal)
        {
            return BitConverter.GetBytes(charVal);
        }

        public static char ToUpper(this char charVal)
        {
            switch (charVal)
            {
                default:
                    return charVal;
                case 'a':
                    return 'A';
                case 'b':
                    return 'B';
                case 'c':
                    return 'C';
                case 'd':
                    return 'D';
                case 'e':
                    return 'E';
                case 'f':
                    return 'F';
                case 'g':
                    return 'G';
                case 'h':
                    return 'H';
                case 'i':
                    return 'I';
                case 'j':
                    return 'J';
                case 'k':
                    return 'K';
                case 'l':
                    return 'L';
                case 'm':
                    return 'M';
                case 'n':
                    return 'N';
                case 'o':
                    return 'O';
                case 'p':
                    return 'P';
                case 'q':
                    return 'Q';
                case 'r':
                    return 'R';
                case 's':
                    return 'S';
                case 't':
                    return 'T';
                case 'u':
                    return 'U';
                case 'v':
                    return 'V';
                case 'w':
                    return 'W';
                case 'x':
                    return 'X';
                case 'y':
                    return 'Y';
                case 'z':
                    return 'Z';
            }
        }

        /*public static byte[] ToBytesSingle(this char charVal)
        {
            return BitConverter.GetBytes(charVal);
        }*/

        public static bool IsAlphanumeric(this char charVal, bool allowDashOrUnderscore = false, bool allowSpace = false)
        {
            return (allowSpace && charVal == ' ') || (allowDashOrUnderscore ? AlphaNumUnderscore.Contains(charVal) : AlphaNumeric.Contains(charVal));
        }

        public static bool IsHexidecimal(this char charVal)
        {
            return Hexidecimal.Contains(charVal.ToUpper());
        }

        public static byte ParseHexToByte(char hex1, char hex0)
        {
            int i1 = 0, i0 = 0;
            for (int i = 0; i < Hexidecimal.Length; i++)
            {
                if (hex1 == Hexidecimal[i])
                    i1 = i;
                if (hex0 == Hexidecimal[i])
                    i0 = i;
            }
            return (byte)(i1 * 0x10 + i0);
        }

        public static byte ParseHexToByte(this char[] values)
        {
            values = new char[] { values.Length > 0 ? values[0] : '0', values.Length >= 1 ? values[1] : '0' };
            int i1 = 0, i0 = 0;
            for (int i = 0; i < Hexidecimal.Length; i++)
            {
                if (values[0] == Hexidecimal[i])
                    i1 = i;
                if (values[1] == Hexidecimal[i])
                    i0 = i;
            }
            return (byte)(i1 * 0x10 + i0);
        }
    }

    public static class Ext_Float
    {
        public static byte[] ToBytes(this float floatVal)
        {
            return BitConverter.GetBytes(floatVal);
        }

        public static bool ApproximatelyEquals(this float f, float comparison, float marginOfError = 0.05f)
        {
            float diff = f - comparison >= 0.0f ? f - comparison : comparison - f;
            if (diff / f <= marginOfError)
            {
                return true;
            }
            return false;
        }
    }

    public static class Ext_Int
    {
        public static byte[] ToBytes(this int intVal)
        {
            return BitConverter.GetBytes(intVal);
        }
    }
    
    public static class Ext_Long
    {
        public static byte[] ToBytes(this long longVal)
        {
            return BitConverter.GetBytes(longVal);
        }

        public static string ParseToString(this long longVal)
        {
            return longVal.ToBytes().ToString();
        }
    }
    
    public static class Ext_Object
    {
        public static PropertyInfo GetProperty<T>(this object obj, string propertyName)
        {
            return typeof(T).GetProperty(propertyName);
        }

        public static object GetPropertyValue<T>(this T obj, string propertyName)
        {
            return typeof(T).GetProperty(propertyName).GetValue(obj);
        }
    }

    public static class Ext_String
    {
        public static byte[] ToBytes(this string str)
        {
            char[] chars = str.ToCharArray();
            byte[] bytes = new byte[chars.Length * 2];
            byte[] cBytes;
            for (int i = 0; i < chars.Length; i++)
            {
                cBytes = chars[i].ToBytes();
                bytes[i * 2] = cBytes[0];
                bytes[i * 2 + 1] = cBytes[1];
            }
            return bytes;
        }

        public static long ToLong(this string str)
        {
            if (str.Length > 8)
                str = str.Substring(0, 8);
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            return bytes.ToLong();
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        public static bool IsEmptyOrNullOrWhiteSpace(this string text)
        {
            return text.IsNullOrEmpty() || text.IsNullOrWhiteSpace();
        }

        public static bool IsAlphanumeric(this string text, bool allowDashOrUnderscore = false, bool allowSpace = false)
        {
            if (text.IsEmptyOrNullOrWhiteSpace())
            {
                return false;
            }
            foreach (char charVal in text)
            {
                if (!charVal.IsAlphanumeric(allowDashOrUnderscore, allowSpace))
                    return false;
            }
            return true;
        }

        public static bool IsHexidecimal(this string text)
        {
            if (text.IsEmptyOrNullOrWhiteSpace())
                return false;
            foreach (char charVal in text)
            {
                if (!charVal.IsHexidecimal())
                    return false;
            }
            return true;
        }

        public static bool ValidateString(this string text, char[] validChars)
        {
            return text.ValidateString(validChars, false);
        }
        
        public static bool ValidateString(this string text, char[] validChars, bool emptyInvalid)
        {
            bool textValid = true;
            if (emptyInvalid && IsEmptyOrNullOrWhiteSpace(text))
            {
                textValid = false;
            }

            int n = text.Length;

            for (int i = 0; i < n; i++)
            {
                char toCheck = char.Parse(text.Substring(i, 1));
                if (!validChars.Contains(toCheck))
                {
                    textValid = false;
                    break;
                }
            }

            return textValid;
        }

        public static string RandomString(int length = 0)
        {
            return RandomString(Ext_Char.AlphaNumUnderscore, length);
        }

        public static string RandomString(char[] charSet, int length = 0)
        {
            System.Random rand = new System.Random((int)DateTime.Now.Ticks);

            if (length <= 0)
                length = rand.Next(5, 20);
            string output = "";
            for (int i = 0; i < length; i++)
            {
                int n = rand.Next(0, charSet.Length - 1);
                output += charSet[n].ToString();
            }
            return output;
        }

        public static bool ValidTypeName(this string typeName)
        {
            return Type.GetType(typeName) != null;
        }

        public static Type TypeFromName(this string typeName)
        {
            if (typeName.ValidTypeName())
            {
                return Type.GetType(typeName);
            }
            else
            {
                return null;
            }
        }
    }

    public static class Ext_Short
    {
        public static byte[] ToBytes(this short shortVal)
        {
            return BitConverter.GetBytes(shortVal);
        }
    }

    public static class Ext_UInt
    {
        public static byte[] ToBytes(this uint uintVal)
        {
            return BitConverter.GetBytes(uintVal);
        }
    }

    public static class Ext_ULong
    {
        public static byte[] ToBytes(this ulong ulongVal)
        {
            return BitConverter.GetBytes(ulongVal);
        }

        public static string ParseToString(this ulong ulongVal)
        {
            return ulongVal.ToBytes().ToString();
        }
    }

    public static class Ext_UShort
    {
        public static byte[] ToBytes(this ushort ushortVal)
        {
            return BitConverter.GetBytes(ushortVal);
        }
    }

    namespace Unity
    {
        using UnityEngine;
        using UnityEditor;

        public static class UnityExt_Float
        {
            public static string[] StopwatchTime(this float time)
            {
                int seconds = (int)Mathf.FloorToInt(time);
                int subSeconds = (int)Mathf.Floor((time - seconds) * 100.0f);

                int tMinutes = seconds - seconds % 60;
                int tSeconds = seconds % 60;

                string strMinutes = tMinutes.ToString();
                string strSeconds = tSeconds.ToString();
                string strSubSecs = subSeconds.ToString();

                if (strSeconds.Length < 2)
                {
                    strSeconds = "0" + strSeconds;
                }
                if (strSubSecs.Length < 2)
                {
                    strSubSecs = "0" + strSubSecs;
                }

                return new string[] { strMinutes, strSeconds, strSubSecs };
            }
        }

        public static class UnityExt_GameObject
        {
            public static void DestroyThis(this GameObject obj, float t = 0.0f)
            {
                GameObject.Destroy(obj, t);
            }
            
            public static void DestroyThisImmediate(this GameObject obj, bool allowDestroyingAssets = false)
            {
                GameObject.DestroyImmediate(obj, allowDestroyingAssets);
            }

            public static bool Exists(this GameObject obj)
            {
                return obj != null;
            }

            public static bool HasComponent<T>(this GameObject obj) where T : Component
            {
                return obj.GetComponent<T>() != null;
            }
            
            public static bool HasComponent(this GameObject obj, System.Type T)
            {
                return obj.GetComponent(T) != null;
            }

            public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
            {
                if (obj.GetComponent<T>() != null)
                {
                    return obj.GetComponent<T>();
                }
                else
                {
                    return obj.AddComponent<T>();
                }
            }

            public static List<T> GetComponents<T>(this GameObject[] objects)
            {
                List<T> componentsInObjects = new List<T>();
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i].TryGetComponent(out T objComponent))
                    {
                        componentsInObjects.Add(objComponent);
                    }
                }
                return componentsInObjects;
            }
            
            public static List<T> GetComponents<T>(this List<GameObject> objects)
            {
                List<T> componentsInObjects = new List<T>();
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i].TryGetComponent(out T objComponent))
                    {
                        componentsInObjects.Add(objComponent);
                    }
                }
                return componentsInObjects;
            }

            public static List<GameObject> GetObjectsWithComponent<T>(this GameObject[] objects)
            {
                List<GameObject> itemsWithComponent = new List<GameObject>();
                if (objects.Length > 0)
                {
                    for (int i = 0; i < objects.Length; i++)
                    {
                        GameObject item = objects[i];
                        T itemComponent = item.GetComponent<T>();
                        if (!itemComponent.Equals(null))
                        {
                            itemsWithComponent.Add(item);
                        }
                    }
                }

                return itemsWithComponent;
            }
            
            public static List<GameObject> GetObjectsWithComponent<T>(this List<GameObject> objects)
            {
                List<GameObject> itemsWithComponent = new List<GameObject>();
                if (objects.Count > 0)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        GameObject item = objects[i];
                        T itemComponent = item.GetComponent<T>();
                        if (!itemComponent.Equals(null))
                        {
                            itemsWithComponent.Add(item);
                        }
                    }
                }

                return itemsWithComponent;
            }

            public static List<GameObject> GetObjectsWithTag(this GameObject[] objects, string tag)
            {
                List<GameObject> itemsWithTag = new List<GameObject>();
                if (objects.Length > 0)
                {
                    for (int i = 0; i < objects.Length; i++)
                    {
                        GameObject item = objects[0];
                        if (item.CompareTag(tag))
                        {
                            itemsWithTag.Add(item);
                        }
                    }
                }

                return itemsWithTag;
            }

            public static List<GameObject> GetObjectsWithTag(this List<GameObject> objects, string tag)
            {
                List<GameObject> itemsWithTag = new List<GameObject>();
                if (objects.Count > 0)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        GameObject item = objects[0];
                        if (item.CompareTag(tag))
                        {
                            itemsWithTag.Add(item);
                        }
                    }
                }

                return itemsWithTag;
            }

            public static List<T> GetComponentsInChildren<T>(GameObject parentObj)
            {
                List<T> componentsInChildren = new List<T>();
                if (parentObj.transform.childCount > 0)
                {
                    for (int i = 0; i < parentObj.transform.childCount; i++)
                    {
                        GameObject child = parentObj.transform.GetChild(i).gameObject;
                        if (child.TryGetComponent<T>(out T childComponent))
                        {
                            componentsInChildren.Add(childComponent);
                        }
                    }
                }
                return componentsInChildren;
            }

            public static List<GameObject> GetChildrenWithComponent<T>(this GameObject parentObj)
            {
                List<GameObject> childrenWithComponent = new List<GameObject>();
                if (parentObj.transform.childCount > 0)
                {
                    for (int i = 0; i < parentObj.transform.childCount; i++)
                    {
                        GameObject child = parentObj.transform.GetChild(i).gameObject;
                        T childComponent;
                        if (child.TryGetComponent<T>(out childComponent))
                        {
                            childrenWithComponent.Add(child);
                        }
                    }
                }
                return childrenWithComponent;
            }

            public static List<GameObject> GetChildrenWithTag(this GameObject parentObj, string tag)
            {
                List<GameObject> childrenWithTag = new List<GameObject>();
                if (parentObj.transform.childCount > 0)
                {
                    for (int i = 0; i < parentObj.transform.childCount; i++)
                    {
                        GameObject child = parentObj.transform.GetChild(i).gameObject;
                        if (child.CompareTag(tag))
                        {
                            childrenWithTag.Add(child);
                        }
                    }
                }
                return childrenWithTag;
            }
        }

        public static class UnityExt_Transform
        {
            public static Transform[] GetChildren(this Transform trn)
            {
                Transform[] children = new Transform[trn.childCount];
                for (int i = 0; i < trn.childCount; i++)
                {
                    children[i] = trn.GetChild(i);
                }
                return children;
            }

            public static void MoveTowards(this Transform trn, Vector3 target)
            {
                trn.MoveTowards(target, 1.0f, false);
            }

            public static void MoveTowards(this Transform trn, Vector3 target, float speed)
            {
                trn.MoveTowards(target, speed, false);
            }

            public static void MoveTowards(this Transform trn, Vector3 target, bool fixedDeltaTime)
            {
                trn.MoveTowards(target, 1.0f, fixedDeltaTime);
            }

            public static void MoveTowards(this Transform trn, Vector3 target, float speed, bool fixedDeltaTime)
            {
                Vector3 direction = (target - trn.position).normalized;
                if (fixedDeltaTime)
                {
                    trn.position += direction * speed * Time.fixedDeltaTime;
                }
                else
                {
                    trn.position += direction * speed * Time.deltaTime;
                }
            }
            
            public static void MoveTowards(this Transform trn, Vector3 target, Vector3 tether, float range)
            {
                trn.MoveTowards(target, tether, range, 1.0f, false);
            }

            public static void MoveTowards(this Transform trn, Vector3 target, Vector3 tether, float range, float speed)
            {
                trn.MoveTowards(target, tether, range, speed, false);
            }

            public static void MoveTowards(this Transform trn, Vector3 target, Vector3 tether, float range, bool fixedDeltaTime)
            {
                trn.MoveTowards(target, tether, range, 1.0f, fixedDeltaTime);
            }

            public static void MoveTowards(this Transform trn, Vector3 target, Vector3 tether, float range, float speed, bool fixedDeltaTime)
            {
                Vector3 direction = (target - trn.position).normalized;
                Vector3 newPos;
                if (fixedDeltaTime)
                {
                    newPos = trn.position + direction * speed * Time.fixedDeltaTime;
                }
                else
                {
                    newPos = trn.position + direction * speed * Time.deltaTime;
                }
                Vector3 disp = newPos - tether;
                if (disp.magnitude > range)
                {
                    newPos = (disp).normalized * range;
                }
                trn.position = newPos;
            }
            
            public static void MoveTowards(this Transform trn, Transform target)
            {
                trn.MoveTowards(target, 1.0f, false);
            }

            public static void MoveTowards(this Transform trn, Transform target, float speed)
            {
                trn.MoveTowards(target, speed, false);
            }

            public static void MoveTowards(this Transform trn, Transform target, bool fixedDeltaTime)
            {
                trn.MoveTowards(target, 1.0f, fixedDeltaTime);
            }

            public static void MoveTowards(this Transform trn, Transform target, float speed, bool fixedDeltaTime)
            {
                trn.MoveTowards(target.position, speed, fixedDeltaTime);
            }

            public static void MoveTo(this Transform trn, Vector3 target)
            {
                trn.MoveTo(target, false);
            }

            public static void MoveTo(this Transform trn, Vector3 target, Axis ignoreAxis)
            {
                trn.MoveTo(target, false, ignoreAxis);
            }

            public static void MoveTo(this Transform trn, Vector3 target, DualAxis ignoreAxes)
            {
                trn.MoveTo(target, false, ignoreAxes);
            }

            public static void MoveTo(this Transform trn, Vector3 target, Vector3 posMin, Vector3 posMax)
            {
                trn.MoveTo(target, false, posMin, posMax);
            }

            public static void MoveTo(this Transform trn, Vector3 target, Vector3 posMin, Vector3 posMax, Axis ignoreAxis)
            {
                trn.MoveTo(target, false, posMin, posMax, ignoreAxis);
            }

            public static void MoveTo(this Transform trn, Vector3 target, Vector3 posMin, Vector3 posMax, DualAxis ignoreAxes)
            {
                trn.MoveTo(target, false, posMin, posMax, ignoreAxes);
            }

            public static void MoveTo(this Transform trn, Vector3 target, bool localPos)
            {
                if (localPos)
                {
                    Vector3 localTarget = trn.InverseTransformPoint(target);
                    trn.localPosition = localTarget;
                }
                else
                {
                    trn.position = target;
                }
            }

            public static void MoveTo(this Transform trn, Vector3 target, bool localPos, Axis ignoreAxis)
            {
                if (localPos)
                {
                    Vector3 localTarget = trn.InverseTransformPoint(target);
                    Vector3 lockedTarget = localTarget;
                    switch (ignoreAxis)
                    {
                        default:
                        case Axis.X:
                            lockedTarget.x = trn.localPosition.x;
                            break;

                        case Axis.Y:
                            lockedTarget.y = trn.localPosition.y;
                            break;

                        case Axis.Z:
                            lockedTarget.z = trn.localPosition.z;
                            break;
                    }
                    trn.localPosition = lockedTarget;
                }
                else
                {
                    Vector3 lockedTarget = target;
                    switch (ignoreAxis)
                    {
                        default:
                        case Axis.X:
                            lockedTarget.x = trn.position.x;
                            break;

                        case Axis.Y:
                            lockedTarget.y = trn.position.y;
                            break;

                        case Axis.Z:
                            lockedTarget.z = trn.position.z;
                            break;
                    }
                    trn.position = lockedTarget;
                }
            }

            public static void MoveTo(this Transform trn, Vector3 target, bool localPos, DualAxis ignoreAxes)
            {
                if (localPos)
                {
                    Vector3 localTarget = trn.InverseTransformPoint(target);
                    Vector3 lockedTarget = localTarget;
                    switch (ignoreAxes)
                    {
                        default:
                        case DualAxis.XY:
                            lockedTarget.x = trn.localPosition.x;
                            lockedTarget.y = trn.localPosition.y;
                            break;

                        case DualAxis.XZ:
                            lockedTarget.x = trn.localPosition.x;
                            lockedTarget.z = trn.localPosition.z;
                            break;

                        case DualAxis.YZ:
                            lockedTarget.y = trn.localPosition.y;
                            lockedTarget.z = trn.localPosition.z;
                            break;
                    }
                    trn.localPosition = lockedTarget;
                }
                else
                {
                    Vector3 lockedTarget = target;
                    switch (ignoreAxes)
                    {
                        default:
                        case DualAxis.XY:
                            lockedTarget.x = trn.position.x;
                            lockedTarget.y = trn.position.y;
                            break;

                        case DualAxis.XZ:
                            lockedTarget.x = trn.position.x;
                            lockedTarget.z = trn.position.z;
                            break;

                        case DualAxis.YZ:
                            lockedTarget.y = trn.position.y;
                            lockedTarget.z = trn.position.z;
                            break;
                    }
                    trn.position = lockedTarget;
                }
            }

            public static void MoveTo(this Transform trn, Vector3 target, bool localPos, Vector3 posMin, Vector3 posMax)
            {
                if (localPos)
                {
                    Vector3 localTarget = trn.InverseTransformPoint(target);
                    Vector3 lockedTarget = localTarget;
                    lockedTarget.x = Mathf.Clamp(localTarget.x, posMin.x, posMax.x);
                    lockedTarget.y = Mathf.Clamp(localTarget.y, posMin.y, posMax.y);
                    lockedTarget.z = Mathf.Clamp(localTarget.z, posMin.z, posMax.z);
                    trn.localPosition = lockedTarget;
                }
                else
                {
                    Vector3 lockedTarget = trn.InverseTransformPoint(target);
                    lockedTarget.x = Mathf.Clamp(lockedTarget.x, posMin.x, posMax.x);
                    lockedTarget.y = Mathf.Clamp(lockedTarget.y, posMin.y, posMax.y);
                    lockedTarget.z = Mathf.Clamp(lockedTarget.z, posMin.z, posMax.z);
                    trn.position = lockedTarget;
                }
            }

            public static void MoveTo(this Transform trn, Vector3 target, bool localPos, Vector3 posMin, Vector3 posMax, Axis ignoreAxis)
            {
                if (localPos)
                {
                    Vector3 localTarget = trn.InverseTransformPoint(target);
                    Vector3 lockedTarget = localTarget;
                    switch (ignoreAxis)
                    {
                        default:
                        case Axis.X:
                            lockedTarget.x = trn.localPosition.x;
                            break;

                        case Axis.Y:
                            lockedTarget.y = trn.localPosition.y;
                            break;

                        case Axis.Z:
                            lockedTarget.z = trn.localPosition.z;
                            break;
                    }
                    lockedTarget.x = Mathf.Clamp(lockedTarget.x, posMin.x, posMax.x);
                    lockedTarget.y = Mathf.Clamp(lockedTarget.y, posMin.y, posMax.y);
                    lockedTarget.z = Mathf.Clamp(lockedTarget.z, posMin.z, posMax.z);
                    trn.localPosition = lockedTarget;
                }
                else
                {
                    Vector3 lockedTarget = target;
                    switch (ignoreAxis)
                    {
                        default:
                        case Axis.X:
                            lockedTarget.x = trn.position.x;
                            break;

                        case Axis.Y:
                            lockedTarget.y = trn.position.y;
                            break;

                        case Axis.Z:
                            lockedTarget.z = trn.position.z;
                            break;
                    }
                    lockedTarget.x = Mathf.Clamp(lockedTarget.x, posMin.x, posMax.x);
                    lockedTarget.y = Mathf.Clamp(lockedTarget.y, posMin.y, posMax.y);
                    lockedTarget.z = Mathf.Clamp(lockedTarget.z, posMin.z, posMax.z);
                    trn.position = lockedTarget;
                }
            }

            public static void MoveTo(this Transform trn, Vector3 target, bool localPos, Vector3 posMin, Vector3 posMax, DualAxis ignoreAxes)
            {
                if (localPos)
                {
                    Vector3 localTarget = trn.InverseTransformPoint(target);
                    Vector3 lockedTarget = localTarget;
                    switch (ignoreAxes)
                    {
                        default:
                        case DualAxis.XY:
                            lockedTarget.x = trn.localPosition.x;
                            lockedTarget.y = trn.localPosition.y;
                            break;

                        case DualAxis.XZ:
                            lockedTarget.x = trn.localPosition.x;
                            lockedTarget.z = trn.localPosition.z;
                            break;

                        case DualAxis.YZ:
                            lockedTarget.y = trn.localPosition.y;
                            lockedTarget.z = trn.localPosition.z;
                            break;
                    }
                    lockedTarget.x = Mathf.Clamp(lockedTarget.x, posMin.x, posMax.x);
                    lockedTarget.y = Mathf.Clamp(lockedTarget.y, posMin.y, posMax.y);
                    lockedTarget.z = Mathf.Clamp(lockedTarget.z, posMin.z, posMax.z);
                    trn.localPosition = lockedTarget;
                }
                else
                {
                    Vector3 lockedTarget = target;
                    switch (ignoreAxes)
                    {
                        default:
                        case DualAxis.XY:
                            lockedTarget.x = trn.position.x;
                            lockedTarget.y = trn.position.y;
                            break;

                        case DualAxis.XZ:
                            lockedTarget.x = trn.position.x;
                            lockedTarget.z = trn.position.z;
                            break;

                        case DualAxis.YZ:
                            lockedTarget.y = trn.position.y;
                            lockedTarget.z = trn.position.z;
                            break;
                    }
                    lockedTarget.x = Mathf.Clamp(lockedTarget.x, posMin.x, posMax.x);
                    lockedTarget.y = Mathf.Clamp(lockedTarget.y, posMin.y, posMax.y);
                    lockedTarget.z = Mathf.Clamp(lockedTarget.z, posMin.z, posMax.z);
                    trn.position = lockedTarget;
                }
            }

            public static void MoveTo(this Transform trn, Vector3 target, Vector3 tether, float range)
            {
                float dist = (target - tether).magnitude;
                if (dist <= range)
                {
                    trn.MoveTo(target, false);
                }
                else
                {
                    Vector3 dir = (target - tether).normalized;
                    Vector3 tetheredTarget = tether + dir * range;
                    trn.MoveTo(tetheredTarget, false);
                }
            }

            public static void MoveTo(this Transform trn, Transform target)
            {
                trn.MoveTo(target.position, false);
            }

            public static void MoveTo(this Transform trn, Transform target, Axis ignoreAxis)
            {
                trn.MoveTo(target.position, false, ignoreAxis);
            }

            public static void MoveTo(this Transform trn, Transform target, DualAxis ignoreAxes)
            {
                trn.MoveTo(target.position, false, ignoreAxes);
            }

            public static void MoveTo(this Transform trn, Transform target, Vector3 posMin, Vector3 posMax)
            {
                trn.MoveTo(target.position, false, posMin, posMax);
            }

            public static void MoveTo(this Transform trn, Transform target, Vector3 posMin, Vector3 posMax, Axis ignoreAxis)
            {
                trn.MoveTo(target.position, false, posMin, posMax, ignoreAxis);
            }

            public static void MoveTo(this Transform trn, Transform target, Vector3 posMin, Vector3 posMax, DualAxis ignoreAxes)
            {
                trn.MoveTo(target.position, false, posMin, posMax, ignoreAxes);
            }

            public static void MoveTo(this Transform trn, Transform target, bool localPos)
            {
                trn.MoveTo(target.position, localPos);
            }

            public static void MoveTo(this Transform trn, Transform target, bool localPos, Axis ignoreAxis)
            {
                trn.MoveTo(target.position, localPos, ignoreAxis);
            }

            public static void MoveTo(this Transform trn, Transform target, bool localPos, DualAxis ignoreAxes)
            {
                trn.MoveTo(target.position, localPos, ignoreAxes);
            }

            public static void MoveTo(this Transform trn, Transform target, bool localPos, Vector3 posMin, Vector3 posMax)
            {
                trn.MoveTo(target.position, localPos, posMin, posMax);
            }

            public static void MoveTo(this Transform trn, Transform target, bool localPos, Vector3 posMin, Vector3 posMax, Axis ignoreAxis)
            {
                trn.MoveTo(target.position, localPos, posMin, posMax, ignoreAxis);
            }

            public static void MoveTo(this Transform trn, Transform target, bool localPos, Vector3 posMin, Vector3 posMax, DualAxis ignoreAxes)
            {
                trn.MoveTo(target.position, localPos, posMin, posMax, ignoreAxes);
            }

            public static void MoveTo(this Transform trn, Transform target, Vector3 tether, float range)
            {
                trn.MoveTo(target.position, tether, range);
            }
        }

        public static class UnityExt_Rect
        {
            public static Rect SetProperty(this Rect rect, RectProperty property, float value)
            {
                switch (property)
                {
                    default:
                    case RectProperty.X:
                        rect.Set(value, rect.y, rect.width, rect.height);
                        return rect;

                    case RectProperty.Y:
                        rect.Set(rect.x, value, rect.width, rect.height);
                        return rect;

                    case RectProperty.Width:
                        rect.Set(rect.x, rect.y, value, rect.height);
                        return rect;

                    case RectProperty.Height:
                        rect.Set(rect.x, rect.y, rect.width, value);
                        return rect;
                }
            }

            public static Rect ModifyProperty(this Rect rect, RectProperty property, float adjustment)
            {
                switch (property)
                {
                    default:
                    case RectProperty.X:
                        rect.Set(rect.x + adjustment, rect.y, rect.width, rect.height);
                        return rect;

                    case RectProperty.Y:
                        rect.Set(rect.x, rect.y + adjustment, rect.width, rect.height);
                        return rect;

                    case RectProperty.Width:
                        rect.Set(rect.x, rect.y, rect.width + adjustment, rect.height);
                        return rect;

                    case RectProperty.Height:
                        rect.Set(rect.x, rect.y, rect.width, rect.height + adjustment);
                        return rect;
                }
            }
        }

        public static class UnityExt_Vector2
        {
            public static Vector2 Closest(this Vector2 origin, Vector2[] points)
            {
                return points[ClosestIndex(origin, points)];
            }

            public static Vector2 Closest(this Vector2 origin, List<Vector2> points)
            {
                return points[ClosestIndex(origin, points)];
            }

            public static int ClosestIndex(this Vector2 origin, Vector2[] points)
            {
                int closest = -1;
                float closestDist = float.MaxValue;

                for (int i = 0; i < points.Length; i++)
                {
                    if ((origin - points[i]).magnitude < closestDist)
                    {
                        closest = i;
                        closestDist = (origin - points[i]).magnitude;
                    }
                }

                return closest;
            }

            public static int ClosestIndex(this Vector2 origin, List<Vector2> points)
            {
                int closest = -1;
                float closestDist = float.MaxValue;

                for (int i = 0; i < points.Count; i++)
                {
                    if ((origin - points[i]).magnitude < closestDist)
                    {
                        closest = i;
                        closestDist = (origin - points[i]).magnitude;
                    }
                }

                return closest;
            }

            public static Vector2 Furthest(this Vector2 origin, Vector2[] points)
            {
                return points[FurthestIndex(origin, points)];
            }

            public static Vector2 Furthest(this Vector2 origin, List<Vector2> points)
            {
                return points[FurthestIndex(origin, points)];
            }

            public static int FurthestIndex(this Vector2 origin, Vector2[] points)
            {
                int furthest = -1;
                float furthestDist = -1.0f;

                for (int i = 0; i < points.Length; i++)
                {
                    if ((origin - points[i]).magnitude > furthestDist)
                    {
                        furthest = i;
                        furthestDist = (origin - points[i]).magnitude;
                    }
                }

                return furthest;
            }

            public static int FurthestIndex(this Vector2 origin, List<Vector2> points)
            {
                int furthest = -1;
                float furthestDist = -1.0f;

                for (int i = 0; i < points.Count; i++)
                {
                    if ((origin - points[i]).magnitude > furthestDist)
                    {
                        furthest = i;
                        furthestDist = (origin - points[i]).magnitude;
                    }
                }

                return furthest;
            }

            public static Vector2 SetAxis(this Vector2 vect, Axis axis, float value)
            {
                switch (axis)
                {
                    default:
                    case Axis.X:
                        vect.x = value;
                        break;

                    case Axis.Y:
                        vect.y = value;
                        break;

                    case Axis.Z:
                        throw new Exception("Vector2 objects have no Z value to set!");
                }
                return vect;
            }

            public static Vector2 AddToAxis(this Vector2 vect, Axis axis, float value)
            {
                switch (axis)
                {
                    default:
                    case Axis.X:
                        vect.x += value;
                        break;

                    case Axis.Y:
                        vect.y += value;
                        break;

                    case Axis.Z:
                        throw new Exception("Vector2 objects have no Z value to modify!");
                }
                return vect;
            }

            // DEPRECATED VERSION
            /*public static float UIAngle(this Vector2 vect)
            {
                float angle = 0.0f;
                float angle = Mathf.Acos(((vect.x + vect.y) / vect.magnitude) * Mathf.PI / 180.0f);
                angle = angle * 180.0f / Mathf.PI;
                if (vect.x > 0.0f)
                {
                    angle = 360.0f - angle;
                }
                return angle;
            }*/

            public static float UIAngle(this Vector2 vect)
            {
                Vector2 nVect = vect.normalized;
                float angle;
                if (vect.x > 0.0f)
                {
                    if (vect.y > 0.0f)
                    {
                        angle = 270.0f + Mathf.Atan(Mathf.Abs(nVect.y / nVect.x)) * 180.0f / Mathf.PI;
                    }
                    else if (vect.y < 0.0f)
                    {
                        angle = 270.0f - Mathf.Atan(Mathf.Abs(nVect.y / nVect.x)) * 180.0f / Mathf.PI;
                    }
                    else
                    {
                        angle = 270.0f;
                    }
                }
                else if (vect.x < 0.0f)
                {
                    if (vect.y > 0.0f)
                    {
                        angle = 90.0f - Mathf.Atan(Mathf.Abs(nVect.y / nVect.x)) * 180.0f / Mathf.PI;
                    }
                    else if (vect.y < 0.0f)
                    {
                        angle = 90.0f + Mathf.Atan(Mathf.Abs(nVect.y / nVect.x)) * 180.0f / Mathf.PI;
                    }
                    else
                    {
                        angle = 270.0f;
                    }
                }
                else
                {
                    if (vect.y < 0.0f)
                    {
                        angle = 180.0f;
                    }
                    else
                    {
                        angle = 0.0f;
                    }
                }
                return angle;
            }

            public static Vector3 AsVec3(this Vector2 vec2)
            {
                return new Vector3(vec2.x, vec2.y, 0.0f);
            }

            public static Vector2 Midpoint(this Vector2 startVect, Vector2 endVect)
            {
                return startVect + (endVect - startVect) / 2.0f;
            }

            public static Axis LargestComponent(this Vector2 vect)
            {
                if (Mathf.Abs(vect.x) > MathF.Abs(vect.y))
                    return Axis.X;
                else
                    return Axis.Y;
            }

            public static float LargestValue(this Vector2 vect)
            {
                if (Mathf.Abs(vect.x) > MathF.Abs(vect.y))
                    return vect.x;
                else
                    return vect.y;
            }

            public static Vector2 AveragePoint(Vector2[] points)
            {
                int n = points.Length;
                float xSum = 0.0f;
                float ySum = 0.0f;
                for (int i = 0; i < n; i++)
                {
                    xSum += points[i].x;
                    ySum += points[i].y;
                }
                return new Vector2(xSum / (float)n, ySum / (float)n);
            }

            public static Vector2 AveragePoint(List<Vector2> points)
            {
                int n = points.Count;
                float xSum = 0.0f;
                float ySum = 0.0f;
                for (int i = 0; i < n; i++)
                {
                    xSum += points[i].x;
                    ySum += points[i].y;
                }
                return new Vector2(xSum / (float)n, ySum / (float)n);
            }
        }

        public static class UnityExt_Vector3
        {
            public static Vector3 RestrictRotVector(this Vector3 rotVect)
            {
                if (rotVect.x > 180.0f)
                {
                    rotVect.x -= 360.0f;
                }
                else if (rotVect.x < -180.0f)
                {
                    rotVect.x += 360.0f;
                }

                if (rotVect.y > 180.0f)
                {
                    rotVect.y -= 360.0f;
                }
                else if (rotVect.y < -180.0f)
                {
                    rotVect.y += 360.0f;
                }

                if (rotVect.z > 180.0f)
                {
                    rotVect.z -= 360.0f;
                }
                else if (rotVect.z < -180.0f)
                {
                    rotVect.z += 360.0f;
                }

                return rotVect;
            }

            public static Vector3 Closest(this Vector3 origin, Vector3[] points)
            {
                return points[ClosestIndex(origin, points)];
            }
            
            public static Vector3 Closest(this Vector3 origin, List<Vector3> points)
            {
                return points[ClosestIndex(origin, points)];
            }
            
            public static int ClosestIndex(this Vector3 origin, Vector3[] points)
            {
                int closest = -1;
                float closestDist = float.MaxValue;

                for (int i = 0; i < points.Length; i++)
                {
                    if ((points[i] - origin).magnitude < closestDist)
                    {
                        closest = i;
                        closestDist = (origin - points[i]).magnitude;
                    }
                }

                return closest;
            }
            
            public static int ClosestIndex(this Vector3 origin, List<Vector3> points)
            {
                int closest = -1;
                float closestDist = float.MaxValue;

                for (int i = 0; i < points.Count; i++)
                {
                    if ((points[i] - origin).magnitude < closestDist)
                    {
                        closest = i;
                        closestDist = (origin - points[i]).magnitude;
                    }
                }

                return closest;
            }
            
            public static Vector3 Furthest(this Vector3 origin, Vector3[] points)
            {
                return points[FurthestIndex(origin, points)];
            }
            
            public static Vector3 Furthest(this Vector3 origin, List<Vector3> points)
            {
                return points[FurthestIndex(origin, points)];
            }
            
            public static int FurthestIndex(this Vector3 origin, Vector3[] points)
            {
                int furthest = -1;
                float furthestDist = -1.0f;

                for (int i = 0; i < points.Length; i++)
                {
                    if ((origin - points[i]).magnitude > furthestDist)
                    {
                        furthest = i;
                        furthestDist = (origin - points[i]).magnitude;
                    }
                }

                return furthest;
            }
            
            public static int FurthestIndex(this Vector3 origin, List<Vector3> points)
            {
                int furthest = -1;
                float furthestDist = -1.0f;

                for (int i = 0; i < points.Count; i++)
                {
                    if ((origin - points[i]).magnitude > furthestDist)
                    {
                        furthest = i;
                        furthestDist = (origin - points[i]).magnitude;
                    }
                }

                return furthest;
            }

            public static Vector3 SetAxis(this Vector3 vect, Axis axis, float value)
            {
                switch (axis)
                {
                    default:
                    case Axis.X:
                        vect.x = value;
                        break;

                    case Axis.Y:
                        vect.y = value;
                        break;

                    case Axis.Z:
                        vect.z = value;
                        break;
                }
                return vect;
            }

            public static Vector3 AddToAxis(this Vector3 vect, Axis axis, float value)
            {
                switch (axis)
                {
                    default:
                    case Axis.X:
                        vect.x += value;
                        break;

                    case Axis.Y:
                        vect.y += value;
                        break;

                    case Axis.Z:
                        vect.z += value;
                        break;
                }
                return vect;
            }

            public static Vector2 AsVec2(this Vector3 vec3)
            {
                return new Vector2(vec3.x, vec3.y);
            }

            public static Vector3 Midpoint(this Vector3 startVect, Vector3 endVect)
            {
                return startVect + (endVect - startVect) / 2.0f;
            }

            public static Axis LargestComponent(this Vector3 vect)
            {
                if (Mathf.Abs(vect.x) > MathF.Abs(vect.y) && Mathf.Abs(vect.x) > MathF.Abs(vect.z))
                    return Axis.X;
                else if (Mathf.Abs(vect.y) > MathF.Abs(vect.z))
                    return Axis.Y;
                else
                    return Axis.Z;
            }

            public static float LargestValue(this Vector3 vect)
            {
                if (Mathf.Abs(vect.x) > MathF.Abs(vect.y) && Mathf.Abs(vect.x) > MathF.Abs(vect.z))
                    return vect.x;
                else if (Mathf.Abs(vect.y) > MathF.Abs(vect.z))
                    return vect.y;
                else
                    return vect.z;
            }

            public static Vector3 AveragePoint(Vector3[] points)
            {
                int n = points.Length;
                float xSum = 0.0f;
                float ySum = 0.0f;
                float zSum = 0.0f;
                for (int i = 0; i < n; i++)
                {
                    xSum += points[i].x;
                    ySum += points[i].y;
                    zSum += points[i].z;
                }
                return new Vector3(xSum / (float)n, ySum / (float)n, zSum / (float)n);
            }

            public static Vector3 AveragePoint(List<Vector3> points)
            {
                int n = points.Count;
                float xSum = 0.0f;
                float ySum = 0.0f;
                float zSum = 0.0f;
                for (int i = 0; i < n; i++)
                {
                    xSum += points[i].x;
                    ySum += points[i].y;
                    zSum += points[i].z;
                }
                return new Vector3(xSum / (float)n, ySum / (float)n, zSum / (float)n);
            }

            public static Vector3 Flatten(this Vector3 vect, Axis axis = Axis.Y)
            {
                float m = vect.magnitude;
                switch (axis)
                {
                    case Axis.X:
                        vect.x = 0.0f;
                        break;

                    default:
                    case Axis.Y:
                        vect.y = 0.0f;
                        break;

                    case Axis.Z:
                        vect.z = 0.0f;
                        break;
                }
                return vect = vect.normalized * m;
            }
        }

        public static class UnityExt_Coroutine
        {
            public static void Stop(this Coroutine routine)
            {
                if (routine != null)
                {
                    GameObject.FindObjectOfType<MonoBehaviour>().StopCoroutine(routine);
                }
            }

            public static void StopAndClear(this Coroutine routine)
            {
                routine.Stop();
                routine = null;
            }
        }

        public static class UnityExt_Mesh
        {
            public static Mesh CalculateDefaultUVs(this Mesh mesh)
            {
                Vector3[] verts = mesh.vertices;
                int[] tris = mesh.triangles;

                float a = 22.5f * Mathf.PI / 180.0f;
                Vector2[] uvs = new Vector2[verts.Length];
                /*Vector2[] uvVerts = new Vector2[]
                {
                    Vector2.zero,
                    new Vector2(1.0f, Mathf.Tan(15.0f.ToRad())),
                    new Vector2(Mathf.Tan(15.0f.ToRad()), 1.0f)
                    
                };*/
                Vector2[] uvVerts = new Vector2[]
                {
                    
                    (Vector2.one + new Vector2(Mathf.Sin(00.0f*a), Mathf.Cos(00.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(01.0f*a), Mathf.Cos(01.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(02.0f*a), Mathf.Cos(02.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(03.0f*a), Mathf.Cos(03.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(04.0f*a), Mathf.Cos(04.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(05.0f*a), Mathf.Cos(05.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(06.0f*a), Mathf.Cos(06.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(07.0f*a), Mathf.Cos(07.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(08.0f*a), Mathf.Cos(08.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(09.0f*a), Mathf.Cos(09.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(10.0f*a), Mathf.Cos(10.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(11.0f*a), Mathf.Cos(11.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(12.0f*a), Mathf.Cos(12.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(13.0f*a), Mathf.Cos(13.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(14.0f*a), Mathf.Cos(14.0f*a))) / 2.0f,
                    (Vector2.one + new Vector2(Mathf.Sin(15.0f*a), Mathf.Cos(15.0f*a))) / 2.0f
                };
                /*Vector2[] uvVerts = new Vector2[]
                {
                    
                };*/
                for (int i = 0; i < uvs.Length; i++)
                {
                    uvs[i] = uvVerts[i % uvVerts.Length];
                }

                /*Vector2[] uvs = new Vector2[tris.Length];
                for (int i = 0; i < tris.Length / 3; i++)
                {
                    int n = 3 * i;
                    Vector3[] tri = new Vector3[] { verts[tris[n]], verts[tris[n + 1]], verts[tris[n + 2]] };
                    int[] triInds = new int[3];
                    if ((tri[0] - tri[1]).magnitude > (tri[1] - tri[2]).magnitude)
                    {
                        if ((tri[0] - tri[1]).magnitude > (tri[2] - tri[0]).magnitude)
                            triInds = new int[] { 0, 1, 2 };
                        else
                            triInds = new int[] { 1, 2, 0 };
                    }
                    else
                    {
                        if ((tri[1] - tri[2]).magnitude > (tri[2] - tri[0]).magnitude)
                            triInds = new int[] { 1, 2, 0 };
                        else
                            triInds = new int[] { 2, 0, 1 };
                    }

                    //Vector2[] triUVs = new Vector2[3];
                    float[] angles = new float[3]
                    {
                        Vector3.Angle(tri[triInds[0]] - tri[triInds[1]], tri[triInds[0]] - tri[triInds[1]]),
                        Vector3.Angle(tri[triInds[1]] - tri[triInds[0]], tri[triInds[1]] - tri[triInds[2]]),
                        Vector3.Angle(tri[triInds[2]] - tri[triInds[0]], tri[triInds[2]] - tri[triInds[1]])
                    };
                    if (angles[0] < 45.0f && angles[1] < 45.0f)
                    {
                        uvs[n + triInds[0]] = Vector2.zero;
                        uvs[n + triInds[1]] = Vector2.one;

                        float m = (tri[triInds[2]] - tri[triInds[0]]).magnitude / (tri[triInds[1]] - tri[triInds[0]]).magnitude;
                        uvs[n + triInds[2]] = new Vector2(Mathf.Cos((45.0f + angles[0]).ToRad()), Mathf.Sin((45.0f + angles[0]).ToRad())) * m;
                    }
                    else if (angles[0] <= 60.0f)
                    {
                        uvs[n + triInds[0]] = Vector2.zero;

                        float ratio = (tri[triInds[2]] - tri[triInds[0]]).magnitude / (tri[triInds[1]] - tri[triInds[0]]).magnitude;
                        float sumAB = (90.0f - angles[0]).ToRad();
                        float angleA = Mathf.Atan((ratio - Mathf.Cos(sumAB))/Mathf.Sin(sumAB));
                        float angleB = sumAB - angleA;

                        uvs[n + triInds[1]] = new Vector2(1.0f, Mathf.Tan(angleB));
                        uvs[n + triInds[2]] = new Vector2(Mathf.Tan(angleA), 1.0f);
                    }
                }*/

                mesh.SetUVs(0, uvs);

                return mesh;
            }
        }

        public static class UnityExt_Color
        {
            public static bool ApproximatelyEquals(this Color clr, Color comparison, float marginOfError = 0.05f)
            {
                if (clr.r.ApproximatelyEquals(comparison.r, marginOfError) && clr.g.ApproximatelyEquals(comparison.g, marginOfError) && clr.b.ApproximatelyEquals(comparison.b, marginOfError))
                    return true;
                else
                    return false;
            }
        }

        public static class UnityExt_Object
        {
            public static bool IsInResourcesFolder(this Object assetObj)
            {
                string path = AssetDatabase.GetAssetPath(assetObj);
                string required = "Assets/Resources/";
                if (path.IsNullOrEmpty() || path.Length <= required.Length || path.Substring(0, required.Length) != required)
                    return false;
                else
                    return true;
            }
        }

        public static class UnityExt_Material
        {
            public static Material DefaultDiffuse()
            {
                return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
            }
        }
    }
}
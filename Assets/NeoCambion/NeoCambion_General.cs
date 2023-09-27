namespace NeoCambion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;

    #region [ ENUMERATION TYPES ]

    public enum Axis { X, Y, Z }
    public enum DualAxis { XY, XZ, YZ }
    public enum CompassBearing_Precision0 { North, East, South, West }
    public enum CompassBearing_Precision1
    {
        North, NorthEast,
        East, SouthEast,
        South, SouthWest,
        West, NorthWest
    }
    public enum CompassBearing_Precision2
    {
        North, NorthNorthEast, NorthEast, EastNorthEast,
        East, EastSouthEast, SouthEast, SouthSouthEast,
        South, SouthSouthWest, SouthWest, WestSouthWest,
        West, WestNorthWest, NorthWest, NorthNorthWest
    }
    public enum RotDirection { Clockwise, CounterClockwise }

    public enum Condition_Number { Never, LessThan, LessThanOrEqualTo, EqualTo, GreaterThanOrEqualTo, GreaterThan, Always }
    public enum Condition_String { Never, Matches, DoesNotMatch, Contains, DoesNotContain, IsSubstring, IsNotSubstring, Always }

    public enum RectProperty { X, Y, Width, Height }

    public enum CaseSesitivity { None, Exact, Lower, Upper }

    public enum AsciiBracketing
    {
        None,
        BracketRound, BracketSquare, BracketCurly, BracketAngle,
        QuoteSingle, QuoteSingleAsym, QuoteSingleInvert, QuoteSingleAngled,
        QuoteDouble, QuoteDoubleAsym, QuoteDoubleInvert, QuoteDoubleAngled,
        ExclaimMark, ExclaimMarkInvert, QuestionMark, QuestionMarkInvert,
        Colon, Semicolon, Asterisk, Hyphen, EnDash, EmDash,
        Slash, BackSlash, Bar, Hash,
    }

    #endregion

    public static class Ext_Byte
    {
        public static bool Bit(this byte value, int index)
        {
            if (index < 0 || index > 7)
                return false;
            return (value & (1u << (7 - index))) != 0;
        }

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
            return new char[] { Ext_Char.Hexadecimal[hex1], Ext_Char.Hexadecimal[hex0] };
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

        public static bool Overflow(this byte byteVal, ulong multiplier)
        {
            float mVal = (float)byteVal * multiplier;
            return mVal > byte.MaxValue;
        }

        public static byte Pow(this byte byteVal, ushort power)
        {
            if (power == 0 || byteVal == 1)
                return 1;
            else if (power == 1 || byteVal == 0)
                return byteVal;
            byte valOut = byteVal;
            for (int i = 0; i < power; i++)
            {
                if (valOut.Overflow(byteVal))
                    return byte.MaxValue;
                else
                    valOut *= byteVal;
            }
            return valOut;
        }
    }

    public static class Ext_Char
    {
        public static readonly char[] Decimal = new char[]
        {
            '0', '1', '2', '3', '4',
            '5', '6', '7', '8', '9',
            '.', ',',
        };
        
        public static readonly char[] DecimalInt = new char[]
        {
            '0', '1', '2', '3', '4',
            '5', '6', '7', '8', '9',
        };
        
        public static readonly char[] Hexadecimal = new char[]
        {
            '0', '1', '2', '3',
            '4', '5', '6', '7',
            '8', '9', 'A', 'B',
            'C', 'D', 'E', 'F',
        };
        
        public static readonly char[] AlphaNumeric = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static readonly char[] AlphaNumUnderscore = new char[]
        {
            '_', '-',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static readonly char[] LatinBasicLowercase = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        public static readonly char[] LatinBasicUppercase = new char[]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static readonly char[] Base64 = new char[]
        {
        '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'a', 'b', 'c', 'd', 'e', 'f',
        'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
        'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
        'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D',
        'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L',
        'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
        'U', 'V', 'W', 'X', 'Y', 'Z', '_', '-',
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

        public static bool IsHexadecimal(this char charVal)
        {
            return Hexadecimal.Contains(charVal.ToUpper());
        }
        
        public static bool IsHexadecimal(this char[] values)
        {
            foreach (char val in values)
            {
                if (!val.IsHexadecimal())
                    return false;
            }
            return true;
        }
        
        public static byte ParseHexToByte(char hex)
        {
            if (hex.IsHexadecimal())
                return (byte)Array.IndexOf(Hexadecimal, hex.ToUpper());
            return 0;
        }
        
        public static byte ParseHexToByte(char hex1, char hex0)
        {
            int i1 = Array.IndexOf(Hexadecimal, hex1.ToUpper());
            int i0 = Array.IndexOf(Hexadecimal, hex0.ToUpper());
            return (byte)((i1 > -1 ? i1 : 0) * 0x10 + (i0 > -1 ? i0 : 0));
        }

        public static byte ParseHexToByte(this char[] values, bool useRightmost = true)
        {
            if (values.Length == 2)
                return ParseHexToByte(values[0], values[1]);
            else if (values.Length == 0)
                return 0;
            else if (values.Length == 1)
                return ParseHexToByte(values[0]);
            else
            {
                if (useRightmost)
                    return ParseHexToByte(values[values.Length - 2], values[values.Length - 1]);
                else
                    return ParseHexToByte(values[0], values[1]);
            }
        }

        public static byte[] ParseHexToBytes(this char[] values, bool useRightmost = true, bool trim = true)
        {
            byte[] bytes = new byte[0];
            if (values.Length % 2 == 1)
            {
                bytes = new byte[(trim ? values.Length - 1 : values.Length - 1 + 1) / 2];
                for (int i = 0, n = bytes.Length - 1, ind1 = 0, ind0 = 0; i <= n; i++, ind1 += 2, ind0 += 2)
                {
                    if (i == 0)
                    {
                        if (useRightmost)
                            ind1 += 1;
                        if (!trim)
                            ind1 -= 2;
                        ind0 = ind1 + 1;
                        bytes[i] = ind1 < 0 ? ParseHexToByte(values[ind0]) : ParseHexToByte(values[ind1], values[ind0]);
                    }
                    else if (i == n)
                    {
                        if (useRightmost)
                            bytes[i] = ParseHexToByte(values[ind1], values[ind0]);
                        else
                            bytes[i] = ParseHexToByte(values[ind1], '0');
                    }
                    else
                    {
                        bytes[i] = ParseHexToByte(values[ind1], values[ind0]);
                    }
                }
            }
            else
            {
                bytes = new byte[values.Length / 2];
                for (int i = 0, ind1 = 0, ind0 = 1; i < bytes.Length; i++, ind1 += 2, ind0 += 2)
                {
                    bytes[i] = ParseHexToByte(values[ind1], values[ind0]);
                }
            }
            return bytes;
        }

        public static ulong ParseHex(this char[] values, bool useRightmost = true)
        {
            char[] chars = new char[16];
            ulong valOut = 0;
            int index;
            for (int i = 0, ind = values.Length - 1; i < 16 && ind >= 0; i++, ind--)
            {
                index = Array.IndexOf(Hexadecimal, values[i].ToUpper());
                if (index == -1)
                    return 0;
                valOut += (ulong)index << 4 * i;
            }
            return valOut;
        }
    }

    // https://stackoverflow.com/questions/16960555/how-do-i-cast-a-generic-enum-to-int
    public static class EnumToInt
    {
        public static Func<T, int> Converter<T>()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidCastException("Argument is not an enumerated type!");
            ParameterExpression inputParameter = Expression.Parameter(typeof(T));
            UnaryExpression body = Expression.Convert(Expression.Parameter(typeof(T)), typeof(int));
            Expression<Func<T, int>> lambda = Expression.Lambda<Func<T, int>>(body, inputParameter);
            return lambda.Compile();
        }
    }

    public static class Ext_Enum
    {
        private static InvalidCastException ArgIsNotEnum => new InvalidCastException("Argument is not an enumerated type!");

        public static int GetCount(this Type enumType)
        {
            if (enumType.IsEnum)
                return Enum.GetNames(enumType).Length;
            else
                throw ArgIsNotEnum;
        }

        public static string[] GetNames(this Type enumType)
        {
            if (enumType.IsEnum)
                return Enum.GetNames(enumType);
            else
                throw ArgIsNotEnum;
        }

        /*public static bool CheckFlags<T>(this T value, T flags) where T : struct, Enum
        {
            Func<T, int> converter = EnumToInt.Converter<T>();
            if (typeof(T).IsEnum)
            {
                int vInt = converter(value), fInt = converter(flags);
                return (vInt & fInt) == fInt;
            }
            else
                throw ArgIsNotEnum;
        }*/

        public static bool HasAnyFlag<T>(this T enumValue, params T[] flags) where T : struct, Enum
        {
            if (flags.Length > 0)
            {
                foreach (T flag in flags)
                {
                    if (enumValue.HasFlag(flag))
                        return true;
                }
            }
            return false;
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

        public static float Round(this float f, ushort decimalPlaces)
        {
            uint d2 = decimalPlaces + 1u;
            float rem = f % (float)Math.Pow(10.0f, -decimalPlaces);
            f -= rem;
            if (rem >= 5 * (float)Math.Pow(10.0f, -d2))
            {
                f += (float)Math.Pow(10.0f, -decimalPlaces);
            }
            return f;
        }
    }

    public static class Ext_Int
    {
        public static int WrapToPositive(this int value)
        {
            return value & int.MaxValue;
        }

        public static bool Bit(this int value, int index)
        {
            if (index < 0 || index > 31)
                return false;
            return (value & (1 << (31 - index))) != 0;
        }

        public static string BitString(this int value)
        {
            BitArray bitArr = new BitArray(new int[] { value });
            string str = "";
            for (int i = 0; i < bitArr.Length; i++)
            {
                str = (bitArr[i] ? '1' : '0') + str;
            }
            return str;
        }

        public static byte[] ToBytes(this int intVal)
        {
            return BitConverter.GetBytes(intVal);
        }

        public static string ParseToHexString(this int intVal)
        {
            string str = "";
            int x;
            for (int i = 3; i >= 0; i--)
            {
                x = intVal;
                x = (x >> (8 * i)) & byte.MaxValue;
                str += ((byte)x).ParseToHexString();
            }
            return str;
        }

        public static bool Overflow(this int intVal, long multiplier)
        {
            float mVal = (float)intVal * multiplier;
            if (mVal < 0)
                return mVal < int.MinValue;
            else
                return mVal > int.MaxValue;
        }
        
        public static bool Overflow(this int intVal, ulong multiplier)
        {
            float mVal = (float)intVal * multiplier;
            if (intVal < 0)
                return mVal < int.MinValue;
            else
                return mVal > int.MaxValue;
        }

        public static int Pow(this int intVal, ushort power)
        {
            if (power == 0 || intVal == 1)
                return 1;
            else if (power == 1 || intVal == 0)
                return intVal;

            int valOut = intVal;
            for (int i = 1; i < power; i++)
            {
                if (valOut.Overflow(intVal))
                    return intVal < 0 ? (power % 2 == 0 ? int.MaxValue : int.MinValue) : int.MaxValue;
                else
                    valOut *= intVal;
            }
            return valOut;
        }

        public static int MinSets(this int setSize, int itemsToFit)
        {
            if (setSize < 1)
                throw new ArgumentException("Set size must be greater than 0!");
            if (itemsToFit < 1)
                return 0;
            if (setSize > itemsToFit)
                return 1;
            int rem = itemsToFit % setSize;
            return (itemsToFit - rem) / setSize + (rem > 0 ? 1 : 0);
        }
    }
    
    public static class Ext_Long
    {
        public static long WrapToPositive(this long value)
        {
            return value & long.MaxValue;
        }

        public static bool Bit(this long value, int index)
        {
            if (index < 0 || index > 63)
                return false;
            return (value & (1l << (63 - index))) != 0;
        }

        private const ulong right32 = 4294967295;
        public static string BitString(this long value)
        {
            bool sign = value < 0;
            int _a = (int)((((ulong)value) >> 32) & right32);
            int _b = (int)(((ulong)value) & right32);

            BitArray bitArr = new BitArray(new int[] { _b, _a });
            string str = "";
            for (int i = 0; i < 64; i++)
            {
                str = (bitArr[i] ? '1' : '0') + str;
            }
            return str;
        }

        public static byte[] ToBytes(this long longVal)
        {
            return BitConverter.GetBytes(longVal);
        }

        public static string ParseToString(this long longVal)
        {
            return longVal.ToBytes().ToString();
        }

        public static bool Overflow(this long longVal, long multiplier)
        {
            float mVal = (float)longVal * multiplier;
            if (mVal < 0)
                return mVal < long.MinValue;
            else
                return mVal > long.MaxValue;
        }

        public static bool Overflow(this long longVal, ulong multiplier)
        {
            float mVal = (float)longVal * multiplier;
            if (longVal < 0)
                return mVal < long.MinValue;
            else
                return mVal > long.MaxValue;
        }

        public static long Pow(this long longVal, ushort power)
        {
            if (power == 0 || longVal == 1)
                return 1;
            else if (power == 1 || longVal == 0)
                return longVal;
            long valOut = longVal;
            for (int i = 0; i < power; i++)
            {
                if (valOut.Overflow(longVal))
                    return longVal < 0 ? (power % 2 == 0 ? long.MaxValue : long.MinValue) : long.MaxValue;
                else
                    valOut *= longVal;
            }
            return valOut;
        }
    }
    
    public static class Ext_Object
    {
        public static bool IsTypeNullable<T>(T obj)
        {
            if (obj == null)
                return true;
            Type t = obj.GetType();
            if (!t.IsValueType)
                return true;
            return t.IsNullable();
        }

        public static PropertyInfo GetProperty<T>(string propertyName) => typeof(T).GetProperty(propertyName);
        public static PropertyInfo GetProperty(this Type T, string propertyName) => T.GetProperty(propertyName);
        public static PropertyInfo GetProperty<T>(this object obj, string propertyName) => typeof(T).GetProperty(propertyName);

        public static object GetPropertyValue<T>(this T obj, string propertyName) => typeof(T).GetProperty(propertyName).GetValue(obj);

        // https://stackoverflow.com/a/45605314
        public static object Copy(this object copyTo, object copyFrom)
        {
            if (copyFrom == null)
            {
                copyTo = null;
            }
            else
            {
                Type toType = copyTo.GetType(), fromType = copyFrom.GetType();
                if (toType == fromType || toType.IsSubclassOf(fromType))
                {
                    BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                    FieldInfo[] copyFields = fromType.GetFields(flags);
                    for (int i = 0; i < copyFields.Length; i++)
                    {
                        BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                        FieldInfo fromfield = fromType.GetField(copyFields[i].Name, bindFlags);
                        FieldInfo toField = toType.GetField(copyFields[i].Name, bindFlags);
                        if (fromfield != null)
                            toField.SetValue(copyTo, fromfield.GetValue(copyFrom));
                    }
                }
            }
            return copyTo;
        }
    }

    public static class Ext_String
    {
        public static string ToSingleString(this string[] strArr, string delimiter = null)
        {
            string str = strArr.Length > 0 ? strArr[0] : "";
            if (delimiter == null)
                delimiter = "";
            for (int i = 1; i < strArr.Length; i++)
            {
                str += delimiter + strArr[i];
            }
            return str;
        }

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
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(str);
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

        public static bool IsDecimal(this string str)
        {
            bool pointFound = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (Ext_Char.Decimal.Contains(str[i]))
                {
                    if (str[i] == '.' || str[i] == ',')
                    {
                        if (pointFound)
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        
        public static bool IsDecimalInt(this string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!Ext_Char.DecimalInt.Contains(str[i]))
                {
                    return false;
                }
            }
            return true;
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

        public static bool IsHexadecimal(this string text)
        {
            if (text.IsEmptyOrNullOrWhiteSpace())
                return false;
            foreach (char charVal in text)
            {
                if (!charVal.IsHexadecimal())
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

        public static int HexStringToInt(this string hexString, bool adjustFromLeft = true)
        {
            if (hexString.IsHexadecimal())
            {
                int l = hexString.Length;
                int valOut = 0;
                byte[] bytes = new byte[4];
                if (adjustFromLeft)
                {
                    if (l > 8)
                    {
                        hexString = hexString.Substring(l - 9);
                    }
                    else if (l < 8)
                    {
                        hexString = hexString.PadLeft(8, '0');
                    }
                }
                else
                {
                    if (l > 8)
                    {
                        hexString = hexString.Substring(0, 8);
                    }
                    else if (l < 8)
                    {
                        hexString = hexString.PadRight(8, '0');
                    }
                }
                bytes = Ext_Char.ParseHexToBytes(hexString.ToCharArray());
                for (int i = 0; i < 4; i++)
                {
                    valOut += bytes[i] << (8 * (3 - i));
                }
                return valOut;
            }
            else
            {
                return 0;
            }
        }

        public static uint HexStringToUInt(this string hexString, bool adjustFromLeft = true)
        {
            if (hexString.IsHexadecimal())
            {
                int l = hexString.Length;
                uint valOut = 0;
                byte[] bytes = new byte[4];
                if (adjustFromLeft)
                {
                    if (l > 8)
                    {
                        hexString = hexString.Substring(l - 9);
                    }
                    else if (l < 8)
                    {
                        hexString = hexString.PadLeft(8, '0');
                    }
                }
                else
                {
                    if (l > 8)
                    {
                        hexString = hexString.Substring(0, 8);
                    }
                    else if (l < 8)
                    {
                        hexString = hexString.PadRight(8, '0');
                    }
                }
                bytes = Ext_Char.ParseHexToBytes(hexString.ToCharArray());
                for (int i = 0; i < 4; i++)
                {
                    valOut += (uint)bytes[i] << (8 * (3 - i));
                }
                return valOut;
            }
            else
            {
                return uint.MinValue;
            }
        }

        public static string TrimFileExtension(this string filepath)
        {
            int ind = filepath.LastIndexOf('.');
            if (filepath.Length > 3 && ind > 0)
            {
                return filepath.Substring(0, ind);
            }
            else
            {
                return filepath;
            }
        }

        public static bool MatchesLatinBasic(this string str, CaseSesitivity caseSesitivity)
        {
            switch (caseSesitivity)
            {
                default:
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (!(Ext_Char.LatinBasicLowercase.Contains(str[i]) || Ext_Char.LatinBasicUppercase.Contains(str[i])))
                            return false;
                    }
                    break;

                case CaseSesitivity.Lower:
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (Ext_Char.LatinBasicLowercase.Contains(str[i]))
                            return false;
                    }
                    break;

                case CaseSesitivity.Upper:
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (Ext_Char.LatinBasicUppercase.Contains(str[i]))
                            return false;
                    }
                    break;
            }
            return true;
        }

        public static bool LatinBasicLowercase(this string str, int charInd)
        {
            if (charInd > -1 && charInd < str.Length)
            {
                return Ext_Char.LatinBasicLowercase.Contains(str[charInd]);
            }
            return false;
        }

        public static bool LatinBasicUppercase(this string str, int charInd)
        {
            if (charInd > -1 && charInd < str.Length)
            {
                return Ext_Char.LatinBasicUppercase.Contains(str[charInd]);
            }
            return false;
        }

        /// <summary>
        /// Adds a bracketing character from a set selection to the start and end of a string.
        /// </summary>
        /// <param name="str">The string to be modified.</param>
        /// <param name="bracketing">Selector for the characters to add to either end.</param>
        /// <returns>The modified string.</returns>
        public static string Bracket(this string str, AsciiBracketing bracketing = AsciiBracketing.BracketRound)
        {
            switch (bracketing)
            {
                default:
                    return str;
                case AsciiBracketing.BracketRound:
                    return '(' + str + ')';
                case AsciiBracketing.BracketSquare:
                    return '[' + str + ']';
                case AsciiBracketing.BracketCurly:
                    return '{' + str + '}';
                case AsciiBracketing.BracketAngle:
                    return '<' + str + '>';
                case AsciiBracketing.QuoteSingle:
                    return '\'' + str + '\'';
                case AsciiBracketing.QuoteSingleAsym:
                    return '‘' + str + '’';
                case AsciiBracketing.QuoteSingleInvert:
                    return '‘' + str + '‚';
                case AsciiBracketing.QuoteSingleAngled:
                    return '‹' + str + '›';
                case AsciiBracketing.QuoteDouble:
                    return '"' + str + '"';
                case AsciiBracketing.QuoteDoubleAsym:
                    return '“' + str + '”';
                case AsciiBracketing.QuoteDoubleInvert:
                    return '“' + str + '„';
                case AsciiBracketing.QuoteDoubleAngled:
                    return '«' + str + '»';
                case AsciiBracketing.ExclaimMark:
                    return '!' + str + '!';
                case AsciiBracketing.ExclaimMarkInvert:
                    return '¡' + str + '!';
                case AsciiBracketing.QuestionMark:
                    return '?' + str + '?';
                case AsciiBracketing.QuestionMarkInvert:
                    return '¿' + str + '?';
                case AsciiBracketing.Colon:
                    return ':' + str + ':';
                case AsciiBracketing.Semicolon:
                    return ';' + str + ';';
                case AsciiBracketing.Asterisk:
                    return '*' + str + '*';
                case AsciiBracketing.Hyphen:
                    return '-' + str + '-';
                case AsciiBracketing.EnDash:
                    return '–' + str + '–';
                case AsciiBracketing.EmDash:
                    return '—' + str + '—';
                case AsciiBracketing.Slash:
                    return '/' + str + '/';
                case AsciiBracketing.BackSlash:
                    return '\\' + str + '\\';
                case AsciiBracketing.Bar:
                    return '|' + str + '|';
                case AsciiBracketing.Hash:
                    return '#' + str + '#';
            }
        }

        /// <summary>
        /// Adds a selected bracketing character to the start and end of a string, repeated a specified number of times.
        /// </summary>
        /// <param name="str">The string to be modified.</param>
        /// <param name="bracketing">Selector for the characters to add to either end.</param>
        /// <param name="repeat">The number of additional times to apply the bracketing characters.</param>
        /// <remarks>Any "repeats" are in addition to the initial bracketing. To only bracket once, use the other overload of this method.</remarks>
        /// <returns>The modified string.</returns>
        public static string Bracket(this string str, AsciiBracketing bracketing, int repeat)
        {
            if (bracketing != AsciiBracketing.None)
            {
                string strOut = str.Bracket(bracketing);
                for (int i = 0; i < repeat; i++)
                {
                    strOut = strOut.Bracket(bracketing);
                }
                return strOut;
            }
            return str;
        }

        public static (int, int) IndicesOfPair(this string str, char boundingChar)
        {
            int indStart = str.IndexOf(boundingChar);
            if (indStart > -1 && indStart < str.Length - 1)
            {
                int indEnd = str.IndexOf(boundingChar, indStart + 1);
                if (indEnd > indStart)
                {
                    return (indStart, indEnd);
                }
            }
            return (indStart, -1);
        }
        public static (int, int) IndicesOfPair(this string str, char startChar, char endChar)
        {
            int indStart = str.IndexOf(startChar);
            if (indStart > -1 && indStart < str.Length - 1)
            {
                int indEnd = str.IndexOf(endChar, indStart + 1);
                if (indEnd > indStart)
                {
                    return (indStart, indEnd);
                }
            }
            return (indStart, -1);
        }

        public static float RangeToFloat(this string str, int indexStart)
        {
            if (indexStart < 0 || indexStart >= str.Length)
                throw new IndexOutOfRangeException("Maximum index must be non-negative and less than to the size of the string!");

            try
            {
                return float.Parse(str[indexStart..^0]);
            }
            catch
            {
                throw new FormatException("String not valid for float conversion: " + indexStart + "-END --> " + str[indexStart..^0]);
            }
        }
        public static float RangeToFloat(this string str, int indexMinInclusive, int indexMaxExclusive)
        {
            if (indexMinInclusive > indexMaxExclusive)
                (indexMinInclusive, indexMaxExclusive) = (indexMaxExclusive, indexMinInclusive);

            if (indexMinInclusive < 0)
                throw new IndexOutOfRangeException("Minimum index must be non-negative and less than the size of the string! (Value passed: " + indexMinInclusive + " | String size: " + str.Length + ")");
            else if (indexMaxExclusive >= str.Length)
                throw new IndexOutOfRangeException("Maximum index must be non-negative and less than or equal to the size of the string! (Value passed: " + indexMaxExclusive + " | String size: " + str.Length + ")");

            try
            {
                return float.Parse(str[indexMinInclusive..indexMaxExclusive]);
            }
            catch
            {
                throw new FormatException("String not valid for float conversion: " + indexMinInclusive + "-" + indexMaxExclusive + " --> " + str[indexMinInclusive..indexMaxExclusive]);
            }
        }

        public static int RangeToInt(this string str, int indexStart)
        {
            if (indexStart < 0 || indexStart >= str.Length)
                throw new IndexOutOfRangeException("Maximum index must be non-negative and less than to the size of the string!");

            try
            {
                return int.Parse(str[indexStart..^0]);
            }
            catch
            {
                throw new FormatException("String not valid for int conversion: " + indexStart + "-END --> " + str[indexStart..^0]);
            }
        }
        public static int RangeToInt(this string str, int indexMinInclusive, int indexMaxExclusive)
        {
            if (indexMinInclusive > indexMaxExclusive)
                (indexMinInclusive, indexMaxExclusive) = (indexMaxExclusive, indexMinInclusive);

            if (indexMinInclusive < 0)
                throw new IndexOutOfRangeException("Minimum index must be non-negative and less than the size of the string! Input: " + indexMinInclusive + " / String size: " + str.Length);
            else if (indexMaxExclusive >= str.Length)
                throw new IndexOutOfRangeException("Maximum index must be non-negative and less than or equal to the size of the string! Input: " + indexMaxExclusive + " / String size: " + str.Length);

            try
            {
                return int.Parse(str[indexMinInclusive..indexMaxExclusive]);
            }
            catch
            {
                throw new FormatException("String not valid for int conversion: " + indexMinInclusive + "-" + indexMaxExclusive + " --> " + str[indexMinInclusive..indexMaxExclusive]);
            }
        }
    }

    public static class Ext_Short
    {
        public static short WrapToPositive(this short value)
        {
            return (short)(value & short.MaxValue);
        }

        public static bool Bit(this short value, int index)
        {
            if (index < 0 || index > 15)
                return false;
            return (value & (1 << (15 - index))) != 0;
        }

        public static string BitString(this short value)
        {
            BitArray bitArr = new BitArray(new int[] { (int)value });
            bitArr[8] = bitArr[0];
            string str = "";
            for (int i = 16; i < 32; i++)
            {
                str = (bitArr[i] ? '1' : '0') + str;
            }
            return str;
        }

        public static byte[] ToBytes(this short shortVal)
        {
            return BitConverter.GetBytes(shortVal);
        }

        public static bool Overflow(this short shortVal, long multiplier)
        {
            float mVal = (float)shortVal * multiplier;
            if (mVal < 0)
                return mVal < short.MinValue;
            else
                return mVal > short.MaxValue;
        }

        public static bool Overflow(this short shortVal, ulong multiplier)
        {
            float mVal = (float)shortVal * multiplier;
            if (shortVal < 0)
                return mVal < short.MinValue;
            else
                return mVal > short.MaxValue;
        }

        public static short Pow(this short shortVal, ushort power)
        {
            if (power == 0 || shortVal == 1)
                return 1;
            else if (power == 1 || shortVal == 0)
                return shortVal;
            short valOut = shortVal;
            for (int i = 0; i < power; i++)
            {
                if (valOut.Overflow(shortVal))
                    return shortVal < 0 ? (power % 2 == 0 ? short.MaxValue : short.MinValue) : short.MaxValue;
                else
                    valOut *= shortVal;
            }
            return valOut;
        }
    }

    public static class Ext_Type
    {
        public static readonly Type[] SystemNumeric = new Type[]
        {
            typeof(sbyte),  typeof(byte),
            typeof(short),  typeof(ushort),
            typeof(int),    typeof(uint),
            typeof(long),   typeof(ulong),
            typeof(nint),   typeof(nuint),
            typeof(float),  typeof(double), typeof(decimal)
        };
        public static bool IsNumeric<T>() => SystemNumeric.Contains(typeof(T));
        public static bool IsNumeric(this Type T) => SystemNumeric.Contains(T);

        public static readonly Type[] SystemIntegral = new Type[]
        {
            typeof(sbyte),  typeof(byte),
            typeof(short),  typeof(ushort),
            typeof(int),    typeof(uint),
            typeof(long),   typeof(ulong),
            typeof(nint),   typeof(nuint),
        };
        public static bool IsIntegral<T>() => SystemIntegral.Contains(typeof(T));
        public static bool IsIntegral(this Type T) => SystemIntegral.Contains(T);

        public static readonly Type[] SystemFloatingPoint = new Type[]
        {
            typeof(float),  typeof(double), typeof(decimal)
        };
        public static bool IsFloatingPoint<T>() => SystemFloatingPoint.Contains(typeof(T));
        public static bool IsFloatingPoint(this Type T) => SystemFloatingPoint.Contains(T);

        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

        public static bool HasDefaultConstructor(this Type type) => type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
    }

    public static class Ext_UInt
    {
        public static bool Bit(this uint value, int index)
        {
            if (index < 0 || index > 31)
                return false;
            return (value & (1u << (31 - index))) != 0;
        }

        public static byte[] ToBytes(this uint uintVal)
        {
            return BitConverter.GetBytes(uintVal);
        }

        public static string BitString(this uint value)
        {
            BitArray bitArr = new BitArray(new int[] { (int)value });
            string str = "";
            for (int i = 0; i < bitArr.Length; i++)
            {
                str = (bitArr[i] ? '1' : '0') + str;
            }
            return str;
        }

        public static string ParseToHexString(this uint uintVal)
        {
            string str = "";
            uint x;
            for (int i = 3; i >= 0; i--)
            {
                x = uintVal;
                x = (x >> (8 * i)) & byte.MaxValue;
                str += ((byte)x).ParseToHexString();
            }
            return str;
        }

        public static bool Overflow(this uint uintVal, ulong multiplier)
        {
            float mVal = (float)uintVal * multiplier;
            return mVal > uint.MaxValue;
        }

        public static uint Pow(this uint uintVal, ushort power)
        {
            if (power == 0 || uintVal == 1)
                return 1;
            else if (power == 1 || uintVal == 0)
                return uintVal;
            uint valOut = uintVal;
            for (int i = 0; i < power; i++)
            {
                if (valOut.Overflow(uintVal))
                    return uint.MaxValue;
                else
                    valOut *= uintVal;
            }
            return valOut;
        }
    }

    public static class Ext_ULong
    {
        public static bool Bit(this ulong value, int index)
        {
            if (index < 0 || index > 63)
                return false;
            return (value & (1ul << (63 - index))) != 0;
        }

        private static ulong right32 = 4294967295;
        public static string BitString(this ulong value)
        {
            bool sign = value < 0;
            int _a = (int)((value >> 32) & right32);
            int _b = (int)(value & right32);

            BitArray bitArr = new BitArray(new int[] { _b, _a });
            string str = "";
            for (int i = 0; i < 64; i++)
            {
                str = (bitArr[i] ? '1' : '0') + str;
            }
            return str;
        }

        public static byte[] ToBytes(this ulong ulongVal)
        {
            return BitConverter.GetBytes(ulongVal);
        }

        public static string ParseToString(this ulong ulongVal)
        {
            return ulongVal.ToBytes().ToString();
        }

        public static bool Overflow(this ulong ulongVal, ulong multiplier)
        {
            float mVal = (float)ulongVal * multiplier;
            return mVal > ulong.MaxValue;
        }

        public static ulong Pow(this ulong ulongVal, ushort power)
        {
            if (power == 0 || ulongVal == 1)
                return 1;
            else if (power == 1 || ulongVal == 0)
                return ulongVal;
            ulong valOut = ulongVal;
            for (int i = 0; i < power; i++)
            {
                if (valOut.Overflow(ulongVal))
                    return ulong.MaxValue;
                else
                    valOut *= ulongVal;
            }
            return valOut;
        }
    }

    public static class Ext_UShort
    {
        public static bool Bit(this ushort value, int index)
        {
            if (index < 0 || index > 15)
                return false;
            return (value & (1u << (15 - index))) != 0;
        }

        public static string BitString(this ushort value)
        {
            BitArray bitArr = new BitArray(new int[] { (int)value });
            bitArr[8] = bitArr[0];
            string str = "";
            for (int i = 16; i < 32; i++)
            {
                str = (bitArr[i] ? '1' : '0') + str;
            }
            return str;
        }

        public static byte[] ToBytes(this ushort ushortVal)
        {
            return BitConverter.GetBytes(ushortVal);
        }

        public static bool Overflow(this ushort ushortVal, ulong multiplier)
        {
            float mVal = (float)ushortVal * multiplier;
            return mVal > ushort.MaxValue;
        }

        public static ushort Pow(this ushort ushortVal, ushort power)
        {
            if (power == 0 || ushortVal == 1)
                return 1;
            else if (power == 1 || ushortVal == 0)
                return ushortVal;
            ushort valOut = ushortVal;
            for (int i = 0; i < power; i++)
            {
                if (valOut.Overflow(ushortVal))
                    return ushort.MaxValue;
                else
                    valOut *= ushortVal;
            }
            return valOut;
        }
    }

    // https://stackoverflow.com/a/16162475
    public static class New<T>
    {
        public static readonly Func<T> Instance = Create();
        public static readonly Func<T, T> InstanceCopy = template => (T)Instance().Copy(template);

        private static Func<T> Create()
        {
            Type type = typeof(T);

            if (type == typeof(string))
                return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();
            else if (type.HasDefaultConstructor())
                return Expression.Lambda<Func<T>>(Expression.New(type)).Compile();
            else
                return () => (T)FormatterServices.GetUninitializedObject(type);
        }
    }

    namespace Unity
    {
        using UnityEngine;
        using UnityEditor;

        public static class NewGameObject
        {
            public static GameObject Get(Vector3 position, params Type[] components)
            {
                GameObject obj = new GameObject(null, components);
                obj.transform.parent = null;
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.identity;
                return obj;
            }
            public static GameObject Get(Quaternion rotation, params Type[] components)
            {
                GameObject obj = new GameObject(null, components);
                obj.transform.parent = null;
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = rotation;
                return obj;
            }
            public static GameObject Get(Vector3 position, Quaternion rotation, params Type[] components)
            {
                GameObject obj = new GameObject(null, components);
                obj.transform.parent = null;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj;
            }
            public static GameObject Get(Transform parent, params Type[] components)
            {
                GameObject obj = new GameObject(null, components);
                obj.transform.parent = parent;
                obj.transform.position = Vector3.zero;
                return obj;
            }
            public static GameObject Get(Transform parent, Vector3 position, params Type[] components)
            {
                GameObject obj = new GameObject(null, components);
                obj.transform.parent = parent;
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.identity;
                return obj;
            }
            public static GameObject Get(Transform parent, Quaternion rotation, params Type[] components)
            {
                GameObject obj = new GameObject(null, components);
                obj.transform.parent = parent;
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = rotation;
                return obj;
            }
            public static GameObject Get(Transform parent, Vector3 position, Quaternion rotation, params Type[] components)
            {
                GameObject obj = new GameObject(null, components);
                obj.transform.parent = parent;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj;
            }
            public static GameObject Get(string name, Vector3 position, params Type[] components)
            {
                GameObject obj = new GameObject(name, components);
                obj.transform.parent = null;
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.identity;
                return obj;
            }
            public static GameObject Get(string name, Quaternion rotation, params Type[] components)
            {
                GameObject obj = new GameObject(name, components);
                obj.transform.parent = null;
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = rotation;
                return obj;
            }
            public static GameObject Get(string name, Vector3 position, Quaternion rotation, params Type[] components)
            {
                GameObject obj = new GameObject(name, components);
                obj.transform.parent = null;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj;
            }
            public static GameObject Get(string name, Transform parent, params Type[] components)
            {
                GameObject obj = new GameObject(name, components);
                obj.transform.parent = parent;
                obj.transform.position = Vector3.zero;
                return obj;
            }
            public static GameObject Get(string name, Transform parent, Vector3 position, params Type[] components)
            {
                GameObject obj = new GameObject(name, components);
                obj.transform.parent = parent;
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.identity;
                return obj;
            }
            public static GameObject Get(string name, Transform parent, Quaternion rotation, params Type[] components)
            {
                GameObject obj = new GameObject(name, components);
                obj.transform.parent = parent;
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = rotation;
                return obj;
            }
            public static GameObject Get(string name, Transform parent, Vector3 position, Quaternion rotation, params Type[] components)
            {
                GameObject obj = new GameObject(name, components);
                obj.transform.parent = parent;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj;
            }
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static class UExt_Float
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

        public static class UExt_String
        {
            public static void CopyToClipboard(this string str) => GUIUtility.systemCopyBuffer = str;
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static class Ext_Component
        {
            public static bool HasComponent<T>(this Component obj) where T : Component => obj.GetComponent<T>() != null;
            public static bool HasComponent(this Component obj, Type T) => obj.GetComponent(T) != null;

            public static List<T> GetComponents<T>(this Component[] objects)
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
            public static List<T> GetComponents<T>(this IList<Component> objects)
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
        }

        public static class Ext_GameObject
        {
            public static void DestroyThis(this GameObject obj, float t = 0.0f) => Object.Destroy(obj, t);
            
            public static void DestroyThisImmediate(this GameObject obj, bool allowDestroyingAssets = false) => Object.DestroyImmediate(obj, allowDestroyingAssets);

            public static bool Exists(this GameObject obj) => obj != null;

            public static bool HasComponent<T>(this GameObject obj) where T : Component => obj.GetComponent<T>() != null;
            public static bool HasComponent(this GameObject obj, Type T)
            {
                if (T.IsSubclassOf(typeof(Component)))
                    return obj.GetComponent(T) == null;
                return false;
            }

            public static T GetOrAddComponent<T>(this GameObject obj) where T : Component => obj.TryGetComponent(out T component) ? component : obj.AddComponent<T>();

            public static T[] GetComponents<T>(this GameObject[] objects) where T : Component
            {
                T[] components = new T[objects.Length];
                for (int i = 0; i < objects.Length; i++)
                {
                    components[i] = (objects[i] == null ? null : objects[i].GetComponent<T>());
                }
                return components;
            }
            public static T[] GetComponents<T>(this IList<GameObject> objects) where T : Component
            {
                T[] components = new T[objects.Count];
                for (int i = 0; i < objects.Count; i++)
                {
                    components[i] = (objects[i] == null ? null : objects[i].GetComponent<T>());
                }
                return components;
            }

            public static List<GameObject> GetObjectsWithComponent<T>(this GameObject[] objects) where T : Component
            {
                List<GameObject> itemsWithComponent = new List<GameObject>();
                if (objects.Length > 0)
                {
                    for (int i = 0; i < objects.Length; i++)
                    {
                        GameObject item = objects[i];
                        if (item.HasComponent<T>())
                            itemsWithComponent.Add(item);
                    }
                }
                return itemsWithComponent;
            }
            public static List<GameObject> GetObjectsWithComponent(this GameObject[] objects, Type T)
            {
                if (T.IsSubclassOf(typeof(Component)))
                {
                    List<GameObject> itemsWithComponent = new List<GameObject>();
                    if (objects.Length > 0)
                    {
                        for (int i = 0; i < objects.Length; i++)
                        {
                            GameObject item = objects[i];
                            if (item.HasComponent(T))
                                itemsWithComponent.Add(item);
                        }
                    }
                    return itemsWithComponent;
                }
                return null;
            }
            public static List<GameObject> GetObjectsWithComponent<T>(this IList<GameObject> objects) where T : Component
            {
                List<GameObject> itemsWithComponent = new List<GameObject>();
                if (objects.Count > 0)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        GameObject item = objects[i];
                        if (item.HasComponent<T>())
                            itemsWithComponent.Add(item);
                    }
                }
                return itemsWithComponent;
            }
            public static List<GameObject> GetObjectsWithComponent(this IList<GameObject> objects, Type T)
            {
                if (T.IsSubclassOf(typeof(Component)))
                {
                    List<GameObject> itemsWithComponent = new List<GameObject>();
                    if (objects.Count > 0)
                    {
                        for (int i = 0; i < objects.Count; i++)
                        {
                            GameObject item = objects[i];
                            if (item.HasComponent(T))
                                itemsWithComponent.Add(item);
                        }
                    }
                    return itemsWithComponent;
                }
                return null;
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
            public static List<GameObject> GetObjectsWithTag(this IList<GameObject> objects, string tag)
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

            public static void Rescale(this GameObject obj, float rescale, Axis rescaleAxis)
            {
                Vector3 scale = obj.transform.localScale;
                switch (rescaleAxis)
                {
                    default: break;
                    case Axis.X: scale.x *= rescale; break;
                    case Axis.Y: scale.y *= rescale; break;
                    case Axis.Z: scale.z *= rescale; break;
                }
                obj.transform.localScale = scale;
            }
            public static void Rescale(this GameObject obj, float rescaleX, float rescaleY, float rescaleZ)
            {
                Vector3 scale = obj.transform.localScale;
                scale.x *= rescaleX;
                scale.y *= rescaleY;
                scale.z *= rescaleZ;
                obj.transform.localScale = scale;
            }
            public static void Rescale(this GameObject obj, Vector3 rescaleBy)
            {
                Vector3 scale = obj.transform.localScale;
                scale.x *= rescaleBy.x;
                scale.y *= rescaleBy.y;
                scale.z *= rescaleBy.z;
                obj.transform.localScale = scale;
            }

            public static void Rescale(this GameObject[] objs, float rescale, Axis rescaleAxis)
            {
                Vector3 scale;
                foreach (GameObject obj in objs)
                {
                    scale = obj.transform.localScale;
                    switch (rescaleAxis)
                    {
                        default: break;
                        case Axis.X: scale.x *= rescale; break;
                        case Axis.Y: scale.y *= rescale; break;
                        case Axis.Z: scale.z *= rescale; break;
                    }
                    obj.transform.localScale = scale;
                }
            }
            public static void Rescale(this GameObject[] objs, float rescaleX, float rescaleY, float rescaleZ)
            {
                Vector3 scale;
                foreach (GameObject obj in objs)
                {
                    scale = obj.transform.localScale;
                    scale.x *= rescaleX;
                    scale.y *= rescaleY;
                    scale.z *= rescaleZ;
                    obj.transform.localScale = scale;
                }
            }
            public static void Rescale(this GameObject[] objs, Vector3 rescaleBy)
            {
                Vector3 scale;
                foreach (GameObject obj in objs)
                {
                    scale = obj.transform.localScale;
                    scale.x *= rescaleBy.x;
                    scale.y *= rescaleBy.y;
                    scale.z *= rescaleBy.z;
                    obj.transform.localScale = scale;
                }
            }

            public static void Rescale(this IList<GameObject> objs, float rescale, Axis rescaleAxis)
            {
                Vector3 scale;
                foreach (GameObject obj in objs)
                {
                    scale = obj.transform.localScale;
                    switch (rescaleAxis)
                    {
                        default: break;
                        case Axis.X: scale.x *= rescale; break;
                        case Axis.Y: scale.y *= rescale; break;
                        case Axis.Z: scale.z *= rescale; break;
                    }
                    obj.transform.localScale = scale;
                }
            }
            public static void Rescale(this IList<GameObject> objs, float rescaleX, float rescaleY, float rescaleZ)
            {
                Vector3 scale;
                foreach (GameObject obj in objs)
                {
                    scale = obj.transform.localScale;
                    scale.x *= rescaleX;
                    scale.y *= rescaleY;
                    scale.z *= rescaleZ;
                    obj.transform.localScale = scale;
                }
            }
            public static void Rescale(this IList<GameObject> objs, Vector3 rescaleBy)
            {
                Vector3 scale;
                foreach (GameObject obj in objs)
                {
                    scale = obj.transform.localScale;
                    scale.x *= rescaleBy.x;
                    scale.y *= rescaleBy.y;
                    scale.z *= rescaleBy.z;
                    obj.transform.localScale = scale;
                }
            }

            public static Mesh GetMesh(this GameObject obj) => obj == null ? null : (obj.HasComponent<MeshFilter>() ? obj.GetComponent<MeshFilter>().sharedMesh : null);
        }

        public static class Ext_Transform
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

            public static void Rescale(this Transform trn, float rescale, Axis rescaleAxis)
            {
                Vector3 scale = trn.localScale;
                switch (rescaleAxis)
                {
                    default: break;
                    case Axis.X: scale.x *= rescale; break;
                    case Axis.Y: scale.y *= rescale; break;
                    case Axis.Z: scale.z *= rescale; break;
                }
                trn.localScale = scale;
            }
            public static void Rescale(this Transform trn, float rescaleX, float rescaleY, float rescaleZ)
            {
                Vector3 scale = trn.localScale;
                scale.x *= rescaleX;
                scale.y *= rescaleY;
                scale.z *= rescaleZ;
                trn.localScale = scale;
            }
            public static void Rescale(this Transform trn, Vector3 rescaleBy)
            {
                Vector3 scale = trn.localScale;
                scale.x *= rescaleBy.x;
                scale.y *= rescaleBy.y;
                scale.z *= rescaleBy.z;
                trn.localScale = scale;
            }

            public static void Rescale(this Transform[] trns, float rescale, Axis rescaleAxis)
            {
                Vector3 scale;
                foreach (Transform trn in trns)
                {
                    scale = trn.localScale;
                    switch (rescaleAxis)
                    {
                        default: break;
                        case Axis.X: scale.x *= rescale; break;
                        case Axis.Y: scale.y *= rescale; break;
                        case Axis.Z: scale.z *= rescale; break;
                    }
                    trn.localScale = scale;
                }
            }
            public static void Rescale(this Transform[] trns, float rescaleX, float rescaleY, float rescaleZ)
            {
                Vector3 scale;
                foreach (Transform trn in trns)
                {
                    scale = trn.localScale;
                    scale.x *= rescaleX;
                    scale.y *= rescaleY;
                    scale.z *= rescaleZ;
                    trn.localScale = scale;
                }
            }
            public static void Rescale(this Transform[] trns, Vector3 rescaleBy)
            {
                Vector3 scale;
                foreach (Transform trn in trns)
                {
                    scale = trn.localScale;
                    scale.x *= rescaleBy.x;
                    scale.y *= rescaleBy.y;
                    scale.z *= rescaleBy.z;
                    trn.localScale = scale;
                }
            }

            public static void Rescale(this IList<Transform> trns, float rescale, Axis rescaleAxis)
            {
                Vector3 scale;
                foreach (Transform trn in trns)
                {
                    scale = trn.localScale;
                    switch (rescaleAxis)
                    {
                        default: break;
                        case Axis.X: scale.x *= rescale; break;
                        case Axis.Y: scale.y *= rescale; break;
                        case Axis.Z: scale.z *= rescale; break;
                    }
                    trn.localScale = scale;
                }
            }
            public static void Rescale(this IList<Transform> trns, float rescaleX, float rescaleY, float rescaleZ)
            {
                Vector3 scale;
                foreach (Transform trn in trns)
                {
                    scale = trn.localScale;
                    scale.x *= rescaleX;
                    scale.y *= rescaleY;
                    scale.z *= rescaleZ;
                    trn.localScale = scale;
                }
            }
            public static void Rescale(this IList<Transform> trns, Vector3 rescaleBy)
            {
                Vector3 scale;
                foreach (Transform trn in trns)
                {
                    scale = trn.localScale;
                    scale.x *= rescaleBy.x;
                    scale.y *= rescaleBy.y;
                    scale.z *= rescaleBy.z;
                    trn.localScale = scale;
                }
            }

            public static void DestroyAllChildren(this Transform trn)
            {
                for (int i = trn.childCount - 1; i >= 0; i--)
                {
                    trn.GetChild(i).gameObject.DestroyThis();
                }
            }
        }

        public static class Ext_Rect
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

        public static class Ext_Vector2
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

        public static class Ext_Vector3
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

            public static Vector2 AsVec2Int(this Vector3 vec3)
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

        public static class Ext_Coroutine
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

            /*public static Coroutine Override(this MonoBehaviour source, Coroutine routine, IEnumerator method)
            {
                if (routine != null)
                    source.StopCoroutine(routine);
                return routine = source.StartCoroutine(method);
            }*/
        }

        public static class Ext_Mesh
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

            public static Mesh Clone(this Mesh mesh)
            {
                Mesh meshOut = new Mesh();
                meshOut.vertices = mesh.vertices;
                meshOut.normals = mesh.normals;
                meshOut.triangles = mesh.triangles;
                meshOut.colors = mesh.colors.Clone() as Color[];
                meshOut.uv = mesh.uv;
                meshOut.uv2 = mesh.uv2;
                meshOut.uv3 = mesh.uv3;
                meshOut.uv4 = mesh.uv4;
                meshOut.uv5 = mesh.uv5;
                meshOut.uv6 = mesh.uv6;
                meshOut.uv7 = mesh.uv7;
                meshOut.uv8 = mesh.uv8;
                meshOut.RecalculateBounds();
                meshOut.RecalculateTangents();
                return meshOut;
            }

            public static Mesh Rotate(this Mesh mesh, Vector3 radians, Vector3 centre)
            {
                // x' = x cos a - y sin a
                // y' = y cos a + x sin a
                Vector3[] vertices = mesh.vertices;
                Vector3[] normals = mesh.normals;

                if (radians.magnitude != 0.0f)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 vert_A = vertices[i] - centre, vert_B = Vector3.zero;
                        // x radians --> Y/Z
                        if (radians.x != 0.0f)
                        {
                            vert_B.x = vert_A.x;
                            vert_B.y = vert_A.y * Mathf.Cos(radians.x) - vert_A.z * Mathf.Sin(radians.x);
                            vert_B.z = vert_A.z * Mathf.Cos(radians.x) + vert_A.y * Mathf.Sin(radians.x);
                            vert_A = vert_B;
                            vert_B = Vector3.zero;
                        }
                        // y radians --> X/Z
                        if (radians.y != 0.0f)
                        {
                            vert_B.x = vert_A.x * Mathf.Cos(radians.y) - vert_A.z * Mathf.Sin(radians.y);
                            vert_B.y = vert_A.y;
                            vert_B.z = vert_A.z * Mathf.Cos(radians.y) + vert_A.x * Mathf.Sin(radians.y);
                            vert_A = vert_B;
                            vert_B = Vector3.zero;
                        }
                        // z radians --> X/Y
                        if (radians.z != 0.0f)
                        {
                            vert_B.x = vert_A.x * Mathf.Cos(radians.z) - vert_A.y * Mathf.Sin(radians.z);
                            vert_B.y = vert_A.y * Mathf.Cos(radians.z) + vert_A.x * Mathf.Sin(radians.z);
                            vert_B.z = vert_A.z;
                            vert_A = vert_B;
                            vert_B = Vector3.zero;
                        }
                        vertices[i] = vert_A + centre;
                    }
                    for (int i = 0; i < normals.Length; i++)
                    {
                        Vector3 norm_A = normals[i], norm_B = Vector3.zero;
                        // x radians --> Y/Z
                        if (radians.x != 0.0f)
                        {
                            norm_B.x = norm_A.x;
                            norm_B.y = norm_A.y * Mathf.Cos(radians.x) - norm_A.z * Mathf.Sin(radians.x);
                            norm_B.z = norm_A.z * Mathf.Cos(radians.x) + norm_A.y * Mathf.Sin(radians.x);
                            norm_A = norm_B;
                            norm_B = Vector3.zero;
                        }
                        // y radians --> X/Z
                        if (radians.y != 0.0f)
                        {
                            norm_B.x = norm_A.x * Mathf.Cos(radians.y) - norm_A.z * Mathf.Sin(radians.y);
                            norm_B.y = norm_A.y;
                            norm_B.z = norm_A.z * Mathf.Cos(radians.y) + norm_A.x * Mathf.Sin(radians.y);
                            norm_A = norm_B;
                            norm_B = Vector3.zero;
                        }
                        // z radians --> X/Y
                        if (radians.z != 0.0f)
                        {
                            norm_B.x = norm_A.x * Mathf.Cos(radians.z) - norm_A.y * Mathf.Sin(radians.z);
                            norm_B.y = norm_A.y * Mathf.Cos(radians.z) + norm_A.x * Mathf.Sin(radians.z);
                            norm_B.z = norm_A.z;
                            norm_A = norm_B;
                            norm_B = Vector3.zero;
                        }
                        normals[i] = norm_A;
                    }
                }

                Mesh meshOut = mesh.Clone();
                meshOut.normals = normals;
                meshOut.vertices = vertices;
                meshOut.RecalculateBounds();
                meshOut.RecalculateTangents();

                return meshOut;
            }

            /// <summary>
            /// Creates a new mesh from the meshes of component GameObjects.
            /// </summary>
            /// <remarks>All source model assets for all component meshes MUST be marked as Read/Write.</remarks>
            /// <param name="mesh">The target mesh to override.</param>
            /// <param name="templateObjects">The template GameObjects to draw mesh data from.</param>
            /// <param name="destroyObjects">Whether to destroy the component objects after the new mesh is created.</param>
            /// <returns>The combined mesh.</returns>
            public static Mesh MergeFrom(IList<GameObject> templateObjects, bool destroyObjects = false)
            {
                Mesh mesh = new Mesh();
                CombineInstance[] combine = new CombineInstance[templateObjects.Count];
                for (int i = templateObjects.Count - 1; i >= 0; i--)
                {
                    combine[i].mesh = templateObjects[i].GetComponent<MeshFilter>().sharedMesh;
                    combine[i].transform = templateObjects[i].transform.localToWorldMatrix;
                    if (destroyObjects)
                        templateObjects[i].DestroyThis();
                    else
                        templateObjects[i].SetActive(false);
                }
                mesh.CombineMeshes(combine);
                return mesh;
            }
            /// <summary>
            /// Creates a new mesh from the meshes of component GameObjects.
            /// </summary>
            /// <remarks>All source model assets for all component meshes MUST be marked as Read/Write.</remarks>
            /// <param name="mesh">The target mesh to override.</param>
            /// <param name="templateObjects">The template GameObjects to draw mesh data from.</param>
            /// <param name="destroyObjects">Whether to destroy the component objects after the new mesh is created.</param>
            /// <returns>The combined mesh.</returns>
            public static Mesh MergeFrom(this Mesh mesh, IList<GameObject> templateObjects, bool destroyObjects = false)
            {
                CombineInstance[] combine = new CombineInstance[templateObjects.Count];
                MeshFilter filter;
                for (int i = templateObjects.Count - 1; i >= 0; i--)
                {
                    if (templateObjects[i] != null && (filter = templateObjects[i].GetComponent<MeshFilter>()) != null)
                        combine[i].mesh = filter.sharedMesh;
                    combine[i].transform = templateObjects[i].transform.localToWorldMatrix;
                    if (destroyObjects)
                        templateObjects[i].DestroyThis();
                    else
                        templateObjects[i].SetActive(false);
                }
                mesh.CombineMeshes(combine);
                return mesh;
            }
        }

        public static class Ext_Color
        {
            public static bool ApproximatelyEquals(this Color clr, Color comparison, float marginOfError = 0.05f)
            {
                if (clr.r.ApproximatelyEquals(comparison.r, marginOfError) && clr.g.ApproximatelyEquals(comparison.g, marginOfError) && clr.b.ApproximatelyEquals(comparison.b, marginOfError))
                    return true;
                else
                    return false;
            }

            public static Color FromHex(string hexValue, float a)
            {
                if (hexValue.Length > 6)
                    hexValue = hexValue.Substring(0, 6);
                else if (hexValue.Length < 6)
                    hexValue = hexValue.PadRight(6, '0');
                if (hexValue.IsHexadecimal())
                {
                    byte r = Ext_Char.ParseHexToByte(hexValue[0], hexValue[1]);
                    byte g = Ext_Char.ParseHexToByte(hexValue[2], hexValue[3]);
                    byte b = Ext_Char.ParseHexToByte(hexValue[4], hexValue[5]);
                    return FromBytes(r, g, b, a);
                }
                else
                {
                    return new Color(0f, 0f, 0f, a);
                }
            }

            public static Color FromBytes(byte r, byte g, byte b, float a)
            {
                return new Color(r / 255f, g / 255f, b / 255f, a);
            }

            public static Color AdjustAlpha(this Color clr, float newAlpha) => new Color(clr.r, clr.g, clr.b, newAlpha);
        }

        public static class Ext_Object
        {
#if UNITY_EDITOR
            public static bool IsInResourcesFolder(this Object assetObj)
            {
                string path = AssetDatabase.GetAssetPath(assetObj);
                string required = "Assets/Resources/";
                if (path.IsNullOrEmpty() || path.Length <= required.Length || path.Substring(0, required.Length) != required)
                    return false;
                else
                    return true;
            }
#endif

            public static Object Clone(this Object original) => Object.Instantiate(original);
            public static Object Clone(this Object original, Transform parent) => Object.Instantiate(original, parent);
            public static Object Clone(this Object original, Transform parent, bool instantiateInWorldSpace) => Object.Instantiate(original, parent, instantiateInWorldSpace);
            public static Object Clone(this Object original, Vector3 position, Quaternion rotation) => Object.Instantiate(original, position, rotation);
            public static Object Clone(this Object original, Vector3 position, Quaternion rotation, Transform parent) => Object.Instantiate(original, position, rotation, parent);
            public static T Clone<T>(this T original) where T : Object => Object.Instantiate(original as Object) as T;
            public static T Clone<T>(this T original, Transform parent) where T : Object => Object.Instantiate(original as Object, parent) as T;
            public static T Clone<T>(this T original, Transform parent, bool instantiateInWorldSpace) where T : Object => Object.Instantiate(original as Object, parent, instantiateInWorldSpace) as T;
            public static T Clone<T>(this T original, Vector3 position, Quaternion rotation) where T : Object => Object.Instantiate(original as Object, position, rotation) as T;
            public static T Clone<T>(this T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object => Object.Instantiate(original as Object, position, rotation, parent) as T;
        }

        public static class Ext_Material
        {
            public static Material DefaultDiffuse => new Material(Shader.Find("Default-Diffuse")) { color = Color.white };
        }
    }

    // OBJECT RETURN INFORMATION STRUCTS
    namespace Unity
    {
        using NeoCambion.Collections;
        using UnityEngine;
        using UnityEngine.Rendering;
        using UnityEngine.SceneManagement;

        public struct ObjectMeshInfo
        {
            public readonly bool isNull;

            private Mesh _mesh;
            public Mesh mesh;
            public Material material
            {
                get { return _materials.HasContents() ? _materials[0] : null; }
                set
                {
                    if (!isNull)
                    {
                        if (!_materials.HasContents())
                            _materials = new Material[1];
                        _materials[0] = value;
                    }
                }
            }
            private Material[] _materials;
            public Material[] materials
            {
                get { return _materials; }
                set { if (!isNull) _materials = value; }
            }
            private ObjectLightingInfo _lighting;
            public ObjectLightingInfo lighting
            {
                get { return _lighting; }
                set { if (!isNull) _lighting = value; }
            }
            private MotionVectorGenerationMode _motionVectors;
            public MotionVectorGenerationMode motionVectors
            {
                get { return _motionVectors; }
                set { if (!isNull) _motionVectors = value; }
            }
            private bool _allowDynamicOcclusion;
            public bool allowDynamicOcclusion
            {
                get { return _allowDynamicOcclusion; }
                set { if (!isNull) _allowDynamicOcclusion = value; }
            }

            public ObjectMeshInfo(GameObject source) : this()
            {
                isNull = false;
                if (source == null)
                {
                    _mesh = null;
                    _materials = new Material[0];
                    _lighting = ObjectLightingInfo.Empty;
                    _motionVectors = default;
                    _allowDynamicOcclusion = true;
                }
                else
                {
                    _mesh = source.GetComponent<MeshFilter>().sharedMesh;
                    MeshRenderer meshRenderer = source.GetComponent<MeshRenderer>();
                    _materials = meshRenderer.sharedMaterials;
                    _lighting = new ObjectLightingInfo(meshRenderer);
                    _motionVectors = meshRenderer.motionVectorGenerationMode;
                    _allowDynamicOcclusion = meshRenderer.allowOcclusionWhenDynamic;
                }
            }

            public static ObjectMeshInfo Null { get { return new ObjectMeshInfo(true); } }
            private ObjectMeshInfo(bool isNull = true) : this()
            {
                this.isNull = true;
                _mesh = null;
                _materials = null;
                _lighting = ObjectLightingInfo.Null;
                _motionVectors = (MotionVectorGenerationMode)(-1);
                _allowDynamicOcclusion = false;
            }
            public static ObjectMeshInfo Empty { get { return new ObjectMeshInfo(0); } }
            private ObjectMeshInfo(int size = 0) : this()
            {
                isNull = false;
                _mesh = null;
                _materials = new Material[0];
                _lighting = ObjectLightingInfo.Empty;
                _motionVectors = default;
                _allowDynamicOcclusion = true;
            }
        }

        public struct ObjectLightingInfo
        {
            public readonly bool isNull;

            private ShadowCastingMode _shadowCastingMode;
            public ShadowCastingMode shadowCastingMode
            {
                get { return _shadowCastingMode; }
                set { if (!isNull) _shadowCastingMode = value; }
            }
            private bool _receiveShadows;
            public bool receiveShadows
            {
                get { return _receiveShadows; }
                set { if (!isNull) _receiveShadows = value; }
            }
            private ObjectLightProbeInfo _lightProbes;
            public ObjectLightProbeInfo lightProbes
            {
                get { return _lightProbes; }
                set { if (!isNull) _lightProbes = value; }
            }

            public ObjectLightingInfo(MeshRenderer infoSource) : this()
            {
                isNull = false;
                if (infoSource == null)
                {

                }
                else
                {

                }
            }
            public ObjectLightingInfo(GameObject infoSource) : this()
            {
                isNull = false;
                MeshRenderer meshRenderer = infoSource?.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    _shadowCastingMode = ShadowCastingMode.On;
                    _receiveShadows = true;
                    _lightProbes = ObjectLightProbeInfo.Empty;
                }
                else
                {
                    _shadowCastingMode = meshRenderer.shadowCastingMode;
                    _receiveShadows = meshRenderer.receiveShadows;
                    _lightProbes = new ObjectLightProbeInfo(meshRenderer);
                }
            }

            public static ObjectLightingInfo Null { get { return new ObjectLightingInfo(true); } }
            private ObjectLightingInfo(bool isNull = true) : this()
            {
                this.isNull = true;
                _shadowCastingMode = (ShadowCastingMode)(-1);
                _receiveShadows = false;
                _lightProbes = ObjectLightProbeInfo.Null;
            }
            public static ObjectLightingInfo Empty { get { return new ObjectLightingInfo(0); } }
            private ObjectLightingInfo(int size = 0) : this()
            {
                isNull = true;
                _shadowCastingMode = ShadowCastingMode.On;
                _receiveShadows = true;
                _lightProbes = ObjectLightProbeInfo.Empty;
            }
        }

        public struct ObjectLightProbeInfo
        {
            public readonly bool isNull;

            private LightProbeUsage _usage;
            public LightProbeUsage usage
            {
                get { return _usage; }
                set { if (!isNull) _usage = value; }
            }
            private GameObject _proxyVolumeOverride;
            public GameObject proxyVolumeOverride
            {
                get { return isNull ? null : (usage == LightProbeUsage.UseProxyVolume ? _proxyVolumeOverride : null); }
                set { if (!isNull) _proxyVolumeOverride = value; }
            }
            private ReflectionProbeUsage _reflectionUsage;
            public ReflectionProbeUsage reflectionUsage
            {
                get { return _reflectionUsage; }
                set { if (!isNull) _reflectionUsage = value; }
            }
            private Transform _probeAnchor;
            public Transform probeAnchor
            {
                get { return _probeAnchor; }
                set { if (!isNull) _probeAnchor = value; }
            }

            public ObjectLightProbeInfo(MeshRenderer infoSource) : this()
            {
                isNull = false;
                if (infoSource == null)
                {
                    _usage = LightProbeUsage.BlendProbes;
                    _proxyVolumeOverride = null;
                    _reflectionUsage = ReflectionProbeUsage.BlendProbes;
                    _probeAnchor = null;
                }
                else
                {
                    _usage = infoSource.lightProbeUsage;
                    _proxyVolumeOverride = infoSource.lightProbeProxyVolumeOverride;
                    _reflectionUsage = infoSource.reflectionProbeUsage;
                    _probeAnchor = infoSource.probeAnchor;
                }
            }
            public ObjectLightProbeInfo(GameObject infoSource) : this()
            {
                isNull = false;
                MeshRenderer meshRenderer = infoSource?.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    _usage = LightProbeUsage.BlendProbes;
                    _proxyVolumeOverride = null;
                    _reflectionUsage = ReflectionProbeUsage.BlendProbes;
                    _probeAnchor = null;
                }
                else
                {
                    _usage = meshRenderer.lightProbeUsage;
                    _proxyVolumeOverride = meshRenderer.lightProbeProxyVolumeOverride;
                    _reflectionUsage = meshRenderer.reflectionProbeUsage;
                    _probeAnchor = meshRenderer.probeAnchor;
                }
            }

            public static ObjectLightProbeInfo Null { get { return new ObjectLightProbeInfo(true); } }
            private ObjectLightProbeInfo(bool isNull = true) : this()
            {
                this.isNull = true;
                _usage = (LightProbeUsage)(-1);
                _proxyVolumeOverride = null;
                _reflectionUsage = (ReflectionProbeUsage)(-1);
                _probeAnchor = null;
            }
            public static ObjectLightProbeInfo Empty { get { return new ObjectLightProbeInfo(0); } }
            private ObjectLightProbeInfo(int size = 0) : this()
            {
                isNull = false;
                _usage = LightProbeUsage.BlendProbes;
                _proxyVolumeOverride = null;
                _reflectionUsage = ReflectionProbeUsage.BlendProbes;
                _probeAnchor = null;
            }
        }

        public struct SceneInfo
        {
            public int buildIndex;
            public string name;
            public string path;
            public int rootCount;
            public readonly bool isNull => buildIndex < 0 && name == null && path == null && rootCount < 0;

            public SceneInfo(int buildIndex, string name, string path, int rootCount)
            {
                this.buildIndex = buildIndex;
                this.name = name;
                this.path = path;
                this.rootCount = rootCount;
            }
            public SceneInfo(Scene source)
            {
                buildIndex = source.buildIndex;
                name = source.name;
                path = source.path;
                rootCount = source.rootCount;
            }

            public static SceneInfo Null => new SceneInfo(-1, null, null, -1);
            public static SceneInfo Active => new SceneInfo(SceneManager.GetActiveScene());
        }
    }
}
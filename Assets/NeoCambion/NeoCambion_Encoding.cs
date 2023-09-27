namespace NeoCambion.Encoding
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using NeoCambion.Collections;

    public static class EncodingDelim
    {
        public static readonly char typeStart = '<';
        public static readonly char typeEnd = '>';

        public static readonly char valueStart = '{';
        public static readonly char valueEnd = '}';

        public static readonly char arrayStart = '[';
        public static readonly char arrayEnd = ']';
        public static readonly char arrayLength = '>';

        public static readonly char split = ',';

        public struct NumDelimRef
        {
            public readonly Type type;
            public readonly char cType;
            public readonly char cSigned;
            public readonly string delim => "#" + cType + cSigned;
            public NumDelimRef(Type type, char cType, bool signed)
            {
                this.type = type;
                this.cType = cType;
                cSigned = signed ? 's' : 'u';
            }

            public static NumDelimRef Sbyte => new NumDelimRef(typeof(sbyte), 'b', true);
            public static NumDelimRef Byte => new NumDelimRef(typeof(byte), 'b', false);
            public static NumDelimRef Short => new NumDelimRef(typeof(short), 's', true);
            public static NumDelimRef Ushort => new NumDelimRef(typeof(ushort), 's', false);
            public static NumDelimRef Int => new NumDelimRef(typeof(int), 'i', true);
            public static NumDelimRef Uint => new NumDelimRef(typeof(uint), 'i', false);
            public static NumDelimRef Long => new NumDelimRef(typeof(long), 'l', true);
            public static NumDelimRef Ulong => new NumDelimRef(typeof(ulong), 'l', false);
            public static NumDelimRef Nint => new NumDelimRef(typeof(nint), 'n', true);
            public static NumDelimRef Nuint => new NumDelimRef(typeof(nuint), 'n', false);
            public static NumDelimRef Float => new NumDelimRef(typeof(float), 'f', true);
            public static NumDelimRef Double => new NumDelimRef(typeof(double), 'd', true);
            public static NumDelimRef Decimal => new NumDelimRef(typeof(decimal), 'D', true);
        }
        public static string Numeric(this Type T)
        {
            switch (T.Name)
            {
                default: throw new ArgumentException("\"" + T.Name + "\" is not a numeric type!");
                case "SByte": return NumDelimRef.Sbyte.delim;
                case "Byte": return NumDelimRef.Byte.delim;
                case "Int16": return NumDelimRef.Short.delim;
                case "UInt16": return NumDelimRef.Ushort.delim;
                case "Int32": return NumDelimRef.Int.delim;
                case "UInt32": return NumDelimRef.Uint.delim;
                case "Int64": return NumDelimRef.Long.delim;
                case "UInt64": return NumDelimRef.Ulong.delim;
                case "IntPtr": return NumDelimRef.Nint.delim;
                case "UIntPtr": return NumDelimRef.Nuint.delim;
            }
        }
    }

    public interface EncodableValue<T>
    {
        public T value { get; set; }
        public string Encode(bool includeType = true);
    }
    public static class EncodableValue
    {

    }
    public interface DecodableString<T>
    {
        public string encoded { get; set; }
        public T Decode();
    }
    public static class DecodableString
    {
        public static string RemoveDelimiters(string encoded)
        {
            if (encoded == null || encoded.Length == 0)
                throw DecodableString.Empty;
            else
            {
                if (encoded[0] == EncodingDelim.typeStart)
                    encoded = encoded.Substring(encoded.IndexOf(EncodingDelim.typeEnd) + 1);
                if (encoded[0] == EncodingDelim.valueStart)
                    encoded = encoded.Substring(1, encoded.Length - 2);
            }
            return encoded;
        }

        public static MissingFieldException Empty => new MissingFieldException("String to decode is empty!");
        public static FormatException BadFormat => new FormatException("String to decode is incorrectly formatted!");
    }

    public struct EncodableGeneric<T> : EncodableValue<T>
    {
        public T value { get; set; }
        public EncodableGeneric(T value) { this.value = value; }
        public string Encode(bool includeType = true) => '\"' + value.ToString() + '\"';
    }
    public struct DecodableGeneric<T>
    {
        public string encoded { get; set; }
        public DecodableGeneric(string encoded) { this.encoded = encoded; }
        public T Decode() => default;
    }

    public interface EncodableArray<T>
    {
        public T[] values { get; set; }
        public string Encode(bool includeType = true);
    }
    public interface DecodableArray<T>
    {
        public string encoded { get; set; }
        public void RemoveDelimiters()
        {
            if (encoded == null || encoded.Length == 0)
                throw DecodableString.Empty;
            else
            {
                if (encoded[0] == EncodingDelim.typeStart)
                    encoded = encoded.Substring(encoded.IndexOf(EncodingDelim.typeEnd) + 1);
                if (encoded[0] == EncodingDelim.arrayStart)
                    encoded = encoded.Substring(1, encoded.Length - 2);
            }
        }
        public T[] Decode();
    }

    public struct EncodableIVec2Int : EncodableValue<IVec2Int>
    {
        public IVec2Int value { get; set; }
        public EncodableIVec2Int(IVec2Int value) { this.value = value; }
        public string Encode(bool includeType = true) => (includeType ? "<IVec2Int>" : "") + EncodingDelim.valueStart + value.x + EncodingDelim.split + value.y + EncodingDelim.valueEnd;
    }
    public struct DecodableUVec2Int : DecodableString<UVec2Int>
    {
        public string encoded { get; set; }
        public DecodableUVec2Int(string encoded) { this.encoded = DecodableString.RemoveDelimiters(encoded); }
        public UVec2Int Decode()
        {
            if (encoded == null || encoded.Length == 0)
                throw DecodableString.Empty;
            int indSplit = encoded.IndexOf(EncodingDelim.split);
            if (indSplit < 0)
                throw DecodableString.BadFormat;
            else
            {
                string xS, yS;
                if (encoded[0] == EncodingDelim.valueStart)
                {
                    xS = encoded.Substring(1, indSplit - 1);
                    yS = encoded.Substring(indSplit + 1, encoded.Length - indSplit - 2);
                }
                else
                {
                    xS = encoded.Substring(0, indSplit);
                    yS = encoded.Substring(indSplit + 1, encoded.Length - indSplit - 1);
                }
                int x, y;
                try
                {
                    x = int.Parse(xS);
                    try
                    {
                        y = int.Parse(yS);
                        return new UVec2Int(x, y);
                    }
                    catch { throw DecodableString.BadFormat; }
                }
                catch { throw DecodableString.BadFormat; }
            }
        }
    }
    public struct DecodableVec2Int : DecodableString<Vec2Int>
    {
        public string encoded { get; set; }
        public DecodableVec2Int(string encoded) { this.encoded = DecodableString.RemoveDelimiters(encoded); }
        public Vec2Int Decode()
        {
            if (encoded == null || encoded.Length == 0)
                throw DecodableString.Empty;
            int indSplit = encoded.IndexOf(EncodingDelim.split);
            if (indSplit < 0)
                throw DecodableString.BadFormat;
            else
            {
                string xS, yS;
                if (encoded[0] == EncodingDelim.valueStart)
                {
                    xS = encoded.Substring(1, indSplit - 1);
                    yS = encoded.Substring(indSplit + 1, encoded.Length - indSplit - 2);
                }
                else
                {
                    xS = encoded.Substring(0, indSplit);
                    yS = encoded.Substring(indSplit + 1, encoded.Length - indSplit - 1);
                }
                int x, y;
                try
                {
                    x = int.Parse(xS);
                    try
                    {
                        y = int.Parse(yS);
                        return new Vec2Int(x, y);
                    }
                    catch { throw DecodableString.BadFormat; }
                }
                catch { throw DecodableString.BadFormat; }
            }
        }
    }

    public static class StringEncoding
    {
        public static readonly Type[] Supported = new Type[]
        {
            typeof(IVec2Int),
            typeof(UVec2Int),
            typeof(Vec2Int),
        };

        public static bool EncodingSupported<T>() => Supported.Contains(typeof(T));
        public static bool EncodingSupported<T>(this T item) => Supported.Contains(typeof(T));
        public static NotSupportedException Unsupported(string typeName) => new NotSupportedException("\"" + typeName + "\" is not currently a supported type for encoding!");

        public static string Encode<T>(this T target)
        {
            if (typeof(T).IsNumeric())
                return EncodingDelim.Numeric(typeof(T)) + target.ToString();
            else if (Supported.Contains(typeof(T)))
                return new EncodableGeneric<T>(target).Encode();
            else
                throw Unsupported(typeof(T).ToString());
        }

        /*public static string EncodeArray(this IList<IVec2Int> target)
        {
            if (target.Count > 0)
            {
                string result = "" + EncodingDelim.arrStart + target.Count + EncodingDelim.arrLength + target[0].Encode();
                for (int i = 1; i < target.Count; i++)
                {
                    result += EncodingDelim.split + target[1].Encode();
                }
            }
            return EncodingDelim.arrStart + "" + EncodingDelim.arrEnd;
        }*/
    }

    public static class StringDecoding
    {
        public static Type[] Supported => StringEncoding.Supported;

        public static bool DecodingSupported<T>() => Supported.Contains(typeof(T));
        public static bool DecodingSupported<T>(this T item) => Supported.Contains(typeof(T));
        public static NotSupportedException Unsupported(string typeName) => new NotSupportedException("\"" + typeName + "\" is not currently a supported type for decoding!");

       /* public static object Decode(this string encoded)
        {
            (int typeStart, int typeEnd) = encoded.IndicesOfPair(EncodingDelim.typeStart, EncodingDelim.typeEnd);
            if (typeStart < 0 || typeEnd < typeStart)
            {
                switch ()
            }
            else
            {

            }
        }
        public static T Decode<T>(this string encoded)
        {
            string tName = typeof(T).Name;
            switch (tName)
            {
                default:

            }
        }*/
    }

    /*namespace Unity
    {
        using UnityEngine;

        public struct EncodableVector2 : EncodableValue<Vector2>
        {
            public Vector2 value { get; set; }
            public EncodableVector2(Vector2 value) { this.value = value; }
            public string Encode() => "<Vector2>" + EncodingDelim.valueStart + value.x + EncodingDelim.split + value.y + EncodingDelim.valueEnd;
        }
        public struct EncodableVector3 : EncodableValue<Vector3>
        {
            public Vector3 value { get; set; }
            public EncodableVector3(Vector3 value) { this.value = value; }
            public string Encode() => "<Vector3>" + EncodingDelim.valueStart + value.x + EncodingDelim.split + value.y + EncodingDelim.split + value.z + EncodingDelim.valueEnd;
        }
        public struct EncodableVector4 : EncodableValue<Vector4>
        {
            public Vector4 value { get; set; }
            public EncodableVector4(Vector4 value) { this.value = value; }
            public string Encode() => "<Vector4>" + EncodingDelim.valueStart + value.x + EncodingDelim.split + value.y + EncodingDelim.split + value.z + EncodingDelim.split + value.w + EncodingDelim.valueEnd;
        }
        public struct EncodableColor : EncodableValue<Color>
        {
            public Color value { get; set; }
            public EncodableColor(Color value) { this.value = value; }
            public string Encode() => "<Color>" + EncodingDelim.valueStart + value.r + EncodingDelim.split + value.g + EncodingDelim.split + value.b + EncodingDelim.split + value.a + EncodingDelim.valueEnd;
        }

        public static class StringEncoding
        {
            private static Type[] _Supported;
            public static Type[] Supported
            {
                get
                {
                    if (UnitySupported == null)
                        UnitySupported = Encoding.StringEncoding.Supported.Combine(_Supported);
                    return UnitySupported;
                }
            }
            private static Type[] UnitySupported = new Type[]
            {
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Color),
            };

            public static bool EncodingSupported<T>() => Supported.Contains(typeof(T));
            public static bool EncodingSupported<T>(this T item) => Supported.Contains(typeof(T));
        }

        public static class StringDecoding
        {
            public static Type[] Supported => StringEncoding.Supported;


        }
    }*/
}
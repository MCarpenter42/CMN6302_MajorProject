namespace NeoCambion
{
    using System;

    public static class VecUtility
    {

    }

    public interface IVec2Int : IComparable<IVec2Int>, IEquatable<IVec2Int>
    {
        public int x { get; set; }
        public int y { get; set; }

        public void SetX(int x);
        public void SetY(int y);
        public void Set(int x, int y) { this.x = x; this.y = y; }
        public void Set(IVec2Int position) { x = position.x; y = position.y; }

        public string ToString() => "(" + x + ", " + y + ")";
        public string ToString(bool typePrefix) => (typePrefix ? "IVec2Int:" : null) + "(" + x + ", " + y + ")";
        public string ToString(AsciiBracketing bracketing) => (x + ", " + y).Bracket(bracketing == AsciiBracketing.None ? AsciiBracketing.BracketRound : bracketing);
        public string ToString(bool typePrefix, AsciiBracketing bracketing) => (typePrefix ? "IVec2Int:" : null) + (x + ", " + y).Bracket(bracketing == AsciiBracketing.None ? AsciiBracketing.BracketRound : bracketing);
    }

    [Serializable]
    public struct UVec2Int : IVec2Int
    {
        private int X, Y;
        public int x { get { return X; } set { SetX(value); } }
        public int y { get { return Y; } set { SetY(value); } }
        public void SetX(int x)
        {
            if (x == int.MinValue)
                X = 0;
            else if (x < 0)
                X = -x;
            else
                X = x;
        }
        public void SetY(int y)
        {
            if (y == int.MinValue)
                Y = 0;
            else if (y < 0)
                Y = -y;
            else
                Y = y;
        }

        public UVec2Int(bool placeholder = false)
        {
            X = 0;
            Y = 0;
        }

        public UVec2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public UVec2Int(UVec2Int original, int xOffset, int yOffset)
        {
            X = original.x + xOffset;
            Y = original.y + yOffset;
        }

        public UVec2Int(UVec2Int original, IVec2Int offset)
        {
            X = original.x + offset.x;
            Y = original.y + offset.y;
        }

        public static UVec2Int zero { get { return new UVec2Int(0, 0); } }
        public static UVec2Int one { get { return new UVec2Int(1, 1); } }
        public static UVec2Int min { get { return new UVec2Int(0, 0); } }
        public static UVec2Int max { get { return new UVec2Int(int.MaxValue, int.MaxValue); } }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public override string ToString() => "(" + x + ", " + y + ")";
        public string ToString(bool typePrefix) => (typePrefix ? "UVec2Int:" : null) + "(" + x + ", " + y + ")";
        public string ToString(AsciiBracketing bracketing) => (x + ", " + y).Bracket(bracketing == AsciiBracketing.None ? AsciiBracketing.BracketRound : bracketing);
        public string ToString(bool typePrefix, AsciiBracketing bracketing) => (typePrefix ? "UVec2Int:" : null) + (x + ", " + y).Bracket(bracketing == AsciiBracketing.None ? AsciiBracketing.BracketRound : bracketing);

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(UVec2Int))
                return false;
            return Equals((IVec2Int)obj);
        }

        public bool Equals(IVec2Int otherVect)
        {
            return x == otherVect.x && y == otherVect.y;
        }

        /// <summary>
        /// Compares with another IVec2Int-based value.
        /// </summary>
        /// <returns>A ternary value converted to decimal.</returns>
        /// <remarks>0 --> x(left) < x(right) / y(left) < y(right)</remarks>
        /// <remarks>1 --> x(left) < x(right) / y(left) = y(right)</remarks>
        /// <remarks>2 --> x(left) < x(right) / y(left) > y(right)</remarks>
        /// <remarks>3 --> x(left) = x(right) / y(left) < y(right)</remarks>
        /// <remarks>4 --> x(left) = x(right) / y(left) = y(right)</remarks>
        /// <remarks>5 --> x(left) = x(right) / y(left) > y(right)</remarks>
        /// <remarks>6 --> x(left) > x(right) / y(left) < y(right)</remarks>
        /// <remarks>7 --> x(left) > x(right) / y(left) = y(right)</remarks>
        /// <remarks>8 --> x(left) > x(right) / y(left) > y(right)</remarks>
        public int CompareTo(IVec2Int otherVect)
        {
            int i = 0;
            if (x < otherVect.x)
                i += 2;
            else if (x == otherVect.x)
                i += 1;
            if (y < otherVect.y)
                i += 6;
            else if (y == otherVect.y)
                i += 3;
            return i;
        }

        public override int GetHashCode() => (x, y).GetHashCode();

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        #region [ OPERATORS ]

        public static bool operator ==(UVec2Int operand1, IVec2Int operand2) => operand1.Equals(operand2);

        public static bool operator !=(UVec2Int operand1, IVec2Int operand2) => !operand1.Equals(operand2);

        public static UVec2Int operator +(UVec2Int operand) => operand;
        public static UVec2Int operator +(UVec2Int left, byte right)
        {
            return new UVec2Int(left.x + right, left.x + right);
        }
        public static UVec2Int operator +(UVec2Int left, double right)
        {
            return new UVec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static UVec2Int operator +(UVec2Int left, float right)
        {
            return new UVec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static UVec2Int operator +(UVec2Int left, long right)
        {
            return new UVec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static UVec2Int operator +(UVec2Int left, int right)
        {
            return new UVec2Int(left.x + right, left.x + right);
        }
        public static UVec2Int operator +(UVec2Int left, short right)
        {
            return new UVec2Int(left.x + right, left.x + right);
        }
        public static UVec2Int operator +(UVec2Int left, ulong right)
        {
            return new UVec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static UVec2Int operator +(UVec2Int left, uint right)
        {
            return new UVec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static UVec2Int operator +(UVec2Int left, ushort right)
        {
            return new UVec2Int(left.x + right, left.x + right);
        }
        public static UVec2Int operator +(UVec2Int left, IVec2Int right)
        {
            return new UVec2Int(left.x + right.x, left.x + right.y);
        }

        public static UVec2Int operator ++(UVec2Int operand)
        {
            operand.x++;
            operand.y++;
            return operand;
        }

        public static Vec2Int operator -(UVec2Int operand) => new Vec2Int(-operand.x, -operand.y);
        public static Vec2Int operator -(UVec2Int left, byte right)
        {
            return new Vec2Int(left.x - right, left.x - right);
        }
        public static Vec2Int operator -(UVec2Int left, double right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(UVec2Int left, float right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(UVec2Int left, long right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(UVec2Int left, int right)
        {
            return new Vec2Int(left.x - right, left.x - right);
        }
        public static Vec2Int operator -(UVec2Int left, short right)
        {
            return new Vec2Int(left.x - right, left.x - right);
        }
        public static Vec2Int operator -(UVec2Int left, ulong right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(UVec2Int left, uint right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(UVec2Int left, ushort right)
        {
            return new Vec2Int(left.x - right, left.x - right);
        }
        public static Vec2Int operator -(UVec2Int left, IVec2Int right)
        {
            return new Vec2Int(left.x - right.x, left.x - right.y);
        }

        public static UVec2Int operator --(UVec2Int operand)
        {
            operand.x--;
            operand.y--;
            return operand;
        }

        public static UVec2Int operator *(UVec2Int left, byte right)
        {
            return new UVec2Int(left.x * right, left.x * right);
        }
        public static UVec2Int operator *(UVec2Int left, double right)
        {
            return new UVec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static UVec2Int operator *(UVec2Int left, float right)
        {
            return new UVec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static UVec2Int operator *(UVec2Int left, long right)
        {
            return new UVec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static UVec2Int operator *(UVec2Int left, int right)
        {
            return new UVec2Int(left.x * right, left.x * right);
        }
        public static UVec2Int operator *(UVec2Int left, short right)
        {
            return new UVec2Int(left.x * right, left.x * right);
        }
        public static UVec2Int operator *(UVec2Int left, ulong right)
        {
            return new UVec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static UVec2Int operator *(UVec2Int left, uint right)
        {
            return new UVec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static UVec2Int operator *(UVec2Int left, ushort right)
        {
            return new UVec2Int(left.x * right, left.x * right);
        }
        public static UVec2Int operator *(UVec2Int left, IVec2Int right)
        {
            return new UVec2Int(left.x * right.x, left.x * right.y);
        }

        public static UVec2Int operator /(UVec2Int left, byte right)
        {
            return new UVec2Int(left.x / right, left.x / right);
        }
        public static UVec2Int operator /(UVec2Int left, double right)
        {
            return new UVec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static UVec2Int operator /(UVec2Int left, float right)
        {
            return new UVec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static UVec2Int operator /(UVec2Int left, long right)
        {
            return new UVec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static UVec2Int operator /(UVec2Int left, int right)
        {
            return new UVec2Int(left.x / right, left.x / right);
        }
        public static UVec2Int operator /(UVec2Int left, short right)
        {
            return new UVec2Int(left.x / right, left.x / right);
        }
        public static UVec2Int operator /(UVec2Int left, ulong right)
        {
            return new UVec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static UVec2Int operator /(UVec2Int left, uint right)
        {
            return new UVec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static UVec2Int operator /(UVec2Int left, ushort right)
        {
            return new UVec2Int(left.x / right, left.x / right);
        }
        public static UVec2Int operator /(UVec2Int left, IVec2Int right)
        {
            return new UVec2Int(left.x / right.x, left.x / right.y);
        }

        public static bool operator >(UVec2Int left, IVec2Int right)
        {
            int i = left.CompareTo(right);
            if (i == 5 || i == 7 || i == 8)
                return true;
            return false;
        }
        
        public static bool operator <(UVec2Int left, IVec2Int right)
        {
            int i = left.CompareTo(right);
            if (i == 0 || i == 1 || i == 3)
                return true;
            return false;
        }

        public static implicit operator Vec2Int(UVec2Int vec) => new Vec2Int(vec.x, vec.y);

        #endregion
    }

    [Serializable]
    public struct Vec2Int : IVec2Int
    {
        private int X, Y;
        public int x { get { return X; } set { SetX(value); } }
        public int y { get { return Y; } set { SetY(value); } }
        public void SetX(int x) => X = x;
        public void SetY(int y) => Y = y;

        public Vec2Int(bool placeholder = false)
        {
            X = 0;
            Y = 0;
        }

        public Vec2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vec2Int(IVec2Int original, int xOffset, int yOffset)
        {
            X = original.x + xOffset;
            Y = original.y + yOffset;
        }

        public Vec2Int(IVec2Int original, IVec2Int offset)
        {
            X = original.x + offset.x;
            Y = original.y + offset.y;
        }

        public static Vec2Int zero { get { return new Vec2Int(0, 0); } }
        public static Vec2Int one { get { return new Vec2Int(1, 1); } }
        public static Vec2Int min { get { return new Vec2Int(int.MinValue, int.MinValue); } }
        public static Vec2Int max { get { return new Vec2Int(int.MaxValue, int.MaxValue); } }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public override string ToString() => "(" + x + ", " + y + ")";
        public string ToString(bool typePrefix) => (typePrefix ? "Vec2Int:" : null) + "(" + x + ", " + y + ")";
        public string ToString(AsciiBracketing bracketing) => (x + ", " + y).Bracket(bracketing == AsciiBracketing.None ? AsciiBracketing.BracketRound : bracketing);
        public string ToString(bool typePrefix, AsciiBracketing bracketing) => (typePrefix ? "Vec2Int:" : null) + (x + ", " + y).Bracket(bracketing == AsciiBracketing.None ? AsciiBracketing.BracketRound : bracketing);

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Vec2Int))
                return false;
            return Equals((IVec2Int)obj);
        }

        public bool Equals(IVec2Int otherVect)
        {
            return x == otherVect.x && y == otherVect.y;
        }

        /// <summary>
        /// Compares with another IVec2Int-based value.
        /// </summary>
        /// <returns>A ternary value converted to decimal.</returns>
        /// <remarks>0 --> x(left) < x(right) / y(left) < y(right)</remarks>
        /// <remarks>1 --> x(left) < x(right) / y(left) = y(right)</remarks>
        /// <remarks>2 --> x(left) < x(right) / y(left) > y(right)</remarks>
        /// <remarks>3 --> x(left) = x(right) / y(left) < y(right)</remarks>
        /// <remarks>4 --> x(left) = x(right) / y(left) = y(right)</remarks>
        /// <remarks>5 --> x(left) = x(right) / y(left) > y(right)</remarks>
        /// <remarks>6 --> x(left) > x(right) / y(left) < y(right)</remarks>
        /// <remarks>7 --> x(left) > x(right) / y(left) = y(right)</remarks>
        /// <remarks>8 --> x(left) > x(right) / y(left) > y(right)</remarks>
        public int CompareTo(IVec2Int otherVect)
        {
            int i = 0;
            if (x < otherVect.x)
                i += 2;
            else if (x == otherVect.x)
                i += 1;
            if (y < otherVect.y)
                i += 6;
            else if (y == otherVect.y)
                i += 3;
            return i;
        }

        public override int GetHashCode() => (x, y).GetHashCode();

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        #region [ OPERATORS ]

        public static bool operator ==(Vec2Int operand1, IVec2Int operand2)
        {
            return operand1.Equals(operand2);
        }

        public static bool operator !=(Vec2Int operand1, IVec2Int operand2)
        {
            return !operand1.Equals(operand2);
        }

        public static Vec2Int operator +(Vec2Int operand) => operand;
        public static Vec2Int operator +(Vec2Int left, byte right)
        {
            return new Vec2Int(left.x + right, left.x + right);
        }
        public static Vec2Int operator +(Vec2Int left, double right)
        {
            return new Vec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static Vec2Int operator +(Vec2Int left, float right)
        {
            return new Vec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static Vec2Int operator +(Vec2Int left, long right)
        {
            return new Vec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static Vec2Int operator +(Vec2Int left, int right)
        {
            return new Vec2Int(left.x + right, left.x + right);
        }
        public static Vec2Int operator +(Vec2Int left, short right)
        {
            return new Vec2Int(left.x + right, left.x + right);
        }
        public static Vec2Int operator +(Vec2Int left, ulong right)
        {
            return new Vec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static Vec2Int operator +(Vec2Int left, uint right)
        {
            return new Vec2Int(left.x + (int)right, left.x + (int)right);
        }
        public static Vec2Int operator +(Vec2Int left, ushort right)
        {
            return new Vec2Int(left.x + right, left.x + right);
        }
        public static Vec2Int operator +(Vec2Int left, IVec2Int right)
        {
            return new Vec2Int(left.x + right.x, left.x + right.y);
        }

        public static Vec2Int operator ++(Vec2Int operand)
        {
            operand.x++;
            operand.y++;
            return operand;
        }

        public static Vec2Int operator -(Vec2Int operand) => new Vec2Int(-operand.x, -operand.y);
        public static Vec2Int operator -(Vec2Int left, byte right)
        {
            return new Vec2Int(left.x - right, left.x - right);
        }
        public static Vec2Int operator -(Vec2Int left, double right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(Vec2Int left, float right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(Vec2Int left, long right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(Vec2Int left, int right)
        {
            return new Vec2Int(left.x - right, left.x - right);
        }
        public static Vec2Int operator -(Vec2Int left, short right)
        {
            return new Vec2Int(left.x - right, left.x - right);
        }
        public static Vec2Int operator -(Vec2Int left, ulong right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(Vec2Int left, uint right)
        {
            return new Vec2Int(left.x - (int)right, left.x - (int)right);
        }
        public static Vec2Int operator -(Vec2Int left, ushort right)
        {
            return new Vec2Int(left.x - right, left.x - right);
        }
        public static Vec2Int operator -(Vec2Int left, IVec2Int right)
        {
            return new Vec2Int(left.x - right.x, left.x - right.y);
        }

        public static Vec2Int operator --(Vec2Int operand)
        {
            operand.x--;
            operand.y--;
            return operand;
        }

        public static Vec2Int operator *(Vec2Int left, byte right)
        {
            return new Vec2Int(left.x * right, left.x * right);
        }
        public static Vec2Int operator *(Vec2Int left, double right)
        {
            return new Vec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static Vec2Int operator *(Vec2Int left, float right)
        {
            return new Vec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static Vec2Int operator *(Vec2Int left, long right)
        {
            return new Vec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static Vec2Int operator *(Vec2Int left, int right)
        {
            return new Vec2Int(left.x * right, left.x * right);
        }
        public static Vec2Int operator *(Vec2Int left, short right)
        {
            return new Vec2Int(left.x * right, left.x * right);
        }
        public static Vec2Int operator *(Vec2Int left, ulong right)
        {
            return new Vec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static Vec2Int operator *(Vec2Int left, uint right)
        {
            return new Vec2Int(left.x * (int)right, left.x * (int)right);
        }
        public static Vec2Int operator *(Vec2Int left, ushort right)
        {
            return new Vec2Int(left.x * right, left.x * right);
        }
        public static Vec2Int operator *(Vec2Int left, IVec2Int right)
        {
            return new Vec2Int(left.x * right.x, left.x * right.y);
        }

        public static Vec2Int operator /(Vec2Int left, byte right)
        {
            return new Vec2Int(left.x / right, left.x / right);
        }
        public static Vec2Int operator /(Vec2Int left, double right)
        {
            return new Vec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static Vec2Int operator /(Vec2Int left, float right)
        {
            return new Vec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static Vec2Int operator /(Vec2Int left, long right)
        {
            return new Vec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static Vec2Int operator /(Vec2Int left, int right)
        {
            return new Vec2Int(left.x / right, left.x / right);
        }
        public static Vec2Int operator /(Vec2Int left, short right)
        {
            return new Vec2Int(left.x / right, left.x / right);
        }
        public static Vec2Int operator /(Vec2Int left, ulong right)
        {
            return new Vec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static Vec2Int operator /(Vec2Int left, uint right)
        {
            return new Vec2Int(left.x / (int)right, left.x / (int)right);
        }
        public static Vec2Int operator /(Vec2Int left, ushort right)
        {
            return new Vec2Int(left.x / right, left.x / right);
        }
        public static Vec2Int operator /(Vec2Int left, IVec2Int right)
        {
            return new Vec2Int(left.x / right.x, left.x / right.y);
        }

        public static bool operator >(Vec2Int left, IVec2Int right)
        {
            int i = left.CompareTo(right);
            if (i == 5 || i == 7 || i == 8)
                return true;
            return false;
        }

        public static bool operator <(Vec2Int left, IVec2Int right)
        {
            int i = left.CompareTo(right);
            if (i == 0 || i == 1 || i == 3)
                return true;
            return false;
        }

        public static implicit operator UVec2Int(Vec2Int vec) => new UVec2Int(vec.x, vec.y);

        #endregion
    }

    namespace Unity
    {
        using UnityEngine;
        using UnityEditor;
#if UNITY_EDITOR
        using NeoCambion.Unity.Editor;
#endif

        public static class VecUtilityUnity
        {
            public static Vector2 ToVector2(this Vec2Int position)
            {
                return new Vector2(position.x, position.y);
            }

            public static Vector2Int ToVector2Int(this Vec2Int position)
            {
                return new Vector2Int(position.x, position.y);
            }

            public static Vec2Int ToVec2Int(this Vector2 position)
            {
                return new Vec2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
            }

            public static Vec2Int ToVec2Int(this Vector2Int position)
            {
                return new Vec2Int(position.x, position.y);
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(IVec2Int))]
        public class UVec2IntDrawer : PropertyDrawer
        {
            float wField;
            Rect xRect, yRect;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                position = EditorElements.PrefixLabel(position, label);
                wField = position.width / 2.0f;

                xRect = new Rect(position) { width = wField };
                xRect = EditorElements.PrefixLabel(xRect, new GUIContent("X"), 20);
                EditorGUI.PropertyField(xRect, property.FindPropertyRelative("x"));

                yRect = new Rect(position) { width = wField, x = position.x + wField };
                yRect = EditorElements.PrefixLabel(yRect, new GUIContent("Y"), 20);
                EditorGUI.PropertyField(yRect, property.FindPropertyRelative("y"));

                EditorGUI.EndProperty();
            }
        }
#endif
    }
}
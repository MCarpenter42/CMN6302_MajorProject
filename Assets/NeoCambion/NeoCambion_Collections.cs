namespace NeoCambion.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public enum SquareGridDirection
    {
        INVALID = -1,
        Up,     UpRight,
        Right,  DownRight,
        Down,   DownLeft,
        Left,   UpLeft
    }

    public static class UGrid2D
    {
        public static int[] Adjacent(int x, int y, SquareGridDirection direction)
        {
            switch (direction)
            {
                default:
                case SquareGridDirection.Up:        return new int[] { x + 0, y + 1 };
                case SquareGridDirection.UpRight:   return new int[] { x + 1, y + 1 };
                case SquareGridDirection.Right:     return new int[] { x + 1, y + 0 };
                case SquareGridDirection.DownRight: return new int[] { x + 1, y - 1 };
                case SquareGridDirection.Down:      return new int[] { x + 0, y - 1 };
                case SquareGridDirection.DownLeft:  return new int[] { x - 1, y - 1 };
                case SquareGridDirection.Left:      return new int[] { x - 1, y + 0 };
                case SquareGridDirection.UpLeft:    return new int[] { x - 1, y + 1 };
            }
        }

        public static UVec2Int Adjacent(UVec2Int position, SquareGridDirection direction)
        {
            switch (direction)
            {
                default:
                case SquareGridDirection.Up:        return new UVec2Int(position,  0,  1);
                case SquareGridDirection.UpRight:   return new UVec2Int(position,  1,  1);
                case SquareGridDirection.Right:     return new UVec2Int(position,  1,  0);
                case SquareGridDirection.DownRight: return new UVec2Int(position,  1, -1);
                case SquareGridDirection.Down:      return new UVec2Int(position,  0, -1);
                case SquareGridDirection.DownLeft:  return new UVec2Int(position, -1, -1);
                case SquareGridDirection.Left:      return new UVec2Int(position, -1,  0);
                case SquareGridDirection.UpLeft:    return new UVec2Int(position, -1,  1);
            }
        }

        public static int[,] AllAdjacent(int x, int y)
        {
            return new int[,]
            {
                { x + 0, y + 1 },
                { x + 1, y + 1 },
                { x + 1, y + 0 },
                { x + 1, y - 1 },
                { x + 0, y - 1 },
                { x - 1, y - 1 },
                { x - 1, y + 0 },
                { x - 1, y + 1 }
            };
        }

        public static UVec2Int[] AllAdjacent(UVec2Int position)
        {
            return new UVec2Int[]
            {
                new UVec2Int(position,  0,  1),
                new UVec2Int(position,  1,  1),
                new UVec2Int(position,  1,  0),
                new UVec2Int(position,  1, -1),
                new UVec2Int(position,  0, -1),
                new UVec2Int(position, -1, -1),
                new UVec2Int(position, -1,  0),
                new UVec2Int(position, -1,  1)
            };
        }

        public static SquareGridDirection Direction(UVec2Int checkFrom, UVec2Int checkTo)
        {
            Vec2Int disp = checkTo - checkFrom;

            return SquareGridDirection.INVALID;
        }
    }

    public class UGrid2D<T>
    {
        public class GridItem<Tval>
        {
            public Tval value;
            public GridItem(Tval value) => this.value = value;
        }
        public class GridRowEnds
        {
            public int left;
            public int right;
        }
        protected static GridRowEnds rowEndsDefault = new GridRowEnds() { left = -1, right = -1 };

        protected List<List<GridItem<T>>> contents = new List<List<GridItem<T>>>();
        protected List<GridRowEnds> rowEnds = new List<GridRowEnds>();
        protected UVec2Int size = UVec2Int.zero;
        // contents[y][x] --> position x, y

        public T valueOnNew;
        public Callback<T, T> onNewCallback;

        protected T CreateNew(T[] ph = null) => New<T>.InstanceCopy(valueOnNew);
        protected GridItem<T> NewItem() => new GridItem<T>(onNewCallback.Invoke());

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public UGrid2D(T valueOnNew = default)
        {
            this.valueOnNew = valueOnNew;
            onNewCallback = CreateNew;
        }
        
        public UGrid2D(Callback<T, T> onNewCallback)
        {
            valueOnNew = default;
            this.onNewCallback = onNewCallback;
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        protected void PadX(int padToTotal)
        {
            foreach (List<GridItem<T>> row in contents)
            {
                if (row.Count < padToTotal)
                    row.PadRight(padToTotal);
            }
            if (padToTotal > size.x)
                size.x = padToTotal;
        }
        protected void PadY(int padToTotal)
        {
            int y = contents.Count, diff = padToTotal - y;
            if (diff > 0)
            {
                contents.PadRightLists(padToTotal);
                rowEnds.PadRight(padToTotal, rowEndsDefault);
            }
            for (int i = y; i < padToTotal; i++)
            {
                contents[i].PadRight(size.x);
            }
            if (padToTotal > size.y)
                size.y = padToTotal;
        }
        protected void TrimX(int trimTo)
        {

        }
        protected void TrimY(int trimTo)
        {

        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        /// <summary>
        /// Gets the value at the given grid position.
        /// </summary>
        /// <param name="x">Position on the grid's X axis.</param>
        /// <param name="y">Position on the grid's Y axis.</param>
        /// <param name="clamp">Prevents out of range exception when set to "true".</param>
        /// <returns>The target value.</returns>
        /// <exception cref="IndexOutOfRangeException">X and Y values must both be from 0 to 2147483647.</exception>
        public T Get(int x, int y, bool clamp = true)
        {
            if (x < 0)
            {
                if (clamp)
                    x = 0;
                else
                    throw new IndexOutOfRangeException("[ X = " + x + " ] is out of range - must not be negative");
            }
            if (y < 0)
            {
                if (clamp)
                    y = 0;
                else
                    throw new IndexOutOfRangeException("[ Y = " + y + " ] is out of range - must not be negative");
            }

            PadX(x + 1);
            PadY(y + 1);

            if (contents[y][x] == null)
                contents[y][x] = NewItem();

            return contents[y][x].value;
        }

        /// <summary>
        /// Sets the value at the given grid position.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="x">Position on the grid's X axis.</param>
        /// <param name="y">Position on the grid's Y axis.</param>
        /// <param name="clamp">Prevents out of range exception when set to "true".</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">X and Y values must both be from 0 to 2147483647.</exception>
        public void Set(T value, int x, int y, bool clamp = true)
        {
            if (x < 0)
            {
                if (clamp)
                    x = 0;
                else
                    throw new IndexOutOfRangeException("[ X = " + x + " ] is out of range - must not be negative");
            }
            if (y < 0)
            {
                if (clamp)
                    y = 0;
                else
                    throw new IndexOutOfRangeException("[ Y = " + y + " ] is out of range - must not be negative");
            }

            PadX(x + 1);
            PadY(y + 1);

            if (contents[y][x] == null)
                contents[y][x] = NewItem();

            contents[y][x].value = value;
        }

        /// <summary>
        /// Accesses the value at the given grid position.
        /// </summary>
        /// <remarks>Positional values are clamped for this accessor.</remarks>
        /// <param name="x">Position on the grid's X axis, from 0 to 2147483647.</param>
        /// <param name="y">Position on the grid's Y axis, from 0 to 2147483647.</param>
        /// <returns>The target value.</returns>
        public virtual T this[int x, int y] { get { return Get(x, y); } set { Set(value, x, y); } }

        /// <summary>
        /// Accesses the value at the given grid position.
        /// </summary>
        /// <remarks>Positional values are clamped for this accessor.</remarks>
        /// <param name="pos">Position on the grid. Both X and Y must be from 0 to 2147483647.</param>
        /// <returns>The target value.</returns>
        public virtual T this[IVec2Int pos] { get { return Get(pos.x, pos.y); } set { Set(value, pos.x, pos.y); } }

        public virtual void Clear()
        {
            contents.Clear();
            rowEnds.Clear();
            size = UVec2Int.zero;
        }
    }

    public static class Grid2D
    {
        public static int[] Adjacent(int x, int y, SquareGridDirection direction)
        {
            switch (direction)
            {
                default:
                case SquareGridDirection.Up:        return new int[] { x + 0, y + 1 };
                case SquareGridDirection.UpRight:   return new int[] { x + 1, y + 1 };
                case SquareGridDirection.Right:     return new int[] { x + 1, y + 0 };
                case SquareGridDirection.DownRight: return new int[] { x + 1, y - 1 };
                case SquareGridDirection.Down:      return new int[] { x + 0, y - 1 };
                case SquareGridDirection.DownLeft:  return new int[] { x - 1, y - 1 };
                case SquareGridDirection.Left:      return new int[] { x - 1, y + 0 };
                case SquareGridDirection.UpLeft:    return new int[] { x - 1, y + 1 };
            }
        }

        public static Vec2Int Adjacent(Vec2Int position, SquareGridDirection direction)
        {
            switch (direction)
            {
                default:
                case SquareGridDirection.Up:        return new Vec2Int(position, 0, 1);
                case SquareGridDirection.UpRight:   return new Vec2Int(position, 1, 1);
                case SquareGridDirection.Right:     return new Vec2Int(position, 1, 0);
                case SquareGridDirection.DownRight: return new Vec2Int(position, 1, -1);
                case SquareGridDirection.Down:      return new Vec2Int(position, 0, -1);
                case SquareGridDirection.DownLeft:  return new Vec2Int(position, -1, -1);
                case SquareGridDirection.Left:      return new Vec2Int(position, -1, 0);
                case SquareGridDirection.UpLeft:    return new Vec2Int(position, -1, 1);
            }
        }

        public static int[,] AllAdjacent(int x, int y)
        {
            return new int[,]
            {
                { x + 0, y + 1 },
                { x + 1, y + 1 },
                { x + 1, y + 0 },
                { x + 1, y - 1 },
                { x + 0, y - 1 },
                { x - 1, y - 1 },
                { x - 1, y + 0 },
                { x - 1, y + 1 }
            };
        }

        public static Vec2Int[] AllAdjacent(Vec2Int position)
        {
            return new Vec2Int[]
            {
                new Vec2Int(position,  0,  1),
                new Vec2Int(position,  1,  1),
                new Vec2Int(position,  1,  0),
                new Vec2Int(position,  1, -1),
                new Vec2Int(position,  0, -1),
                new Vec2Int(position, -1, -1),
                new Vec2Int(position, -1,  0),
                new Vec2Int(position, -1,  1)
            };
        }

        public static SquareGridDirection Direction(Vec2Int checkFrom, Vec2Int checkTo)
        {
            Vec2Int disp = checkTo - checkFrom;

            return SquareGridDirection.INVALID;
        }
    }

    public class Grid2D<T> : UGrid2D<T>
    {
        protected const int MinBoundary = -1073741825;
        protected const int MaxBoundary = 1073741824;

        protected Vec2Int offset = Vec2Int.zero;

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        protected void PadNegX(int newOffset)
        {
            if (newOffset < offset.x)
            {
                int padToTotal = size.x + (offset.x - newOffset);
                if (size.y > 0)
                {
                    foreach (List<GridItem<T>> row in contents)
                        row.PadLeft(padToTotal);
                }
                offset.x = newOffset;
            }
        }
        protected void PadNegY(int newOffset)
        {
            if (newOffset < offset.y)
            {
                int addSize = offset.y - newOffset, padToTotal = size.y + (offset.y - newOffset);
                contents.PadLeftLists(padToTotal);
                rowEnds.PadLeft(padToTotal, rowEndsDefault);
                for (int i = 0; i < addSize; i++)
                {
                    contents[i].PadLeft(padToTotal);
                }
                offset.y = newOffset;
            }
        }
        protected void TrimNegX(int trimTo)
        {

        }
        protected void TrimNegY(int trimTo)
        {

        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        /// <summary>
        /// Gets the value at the given grid position, allowing for negative values.
        /// </summary>
        /// <param name="x">Position on the grid's X axis.</param>
        /// <param name="y">Position on the grid's Y axis.</param>
        /// <param name="clamp">Prevents out of range exception when set to "true".</param>
        /// <returns>The target value.</returns>
        /// <exception cref="IndexOutOfRangeException">X and Y values must both be from -1073741824 to 1073741823.</exception>
        public T OffsetGet(int x, int y, bool clamp = true)
        {
            string logStr1 = "[1] Coords: (" + x + ", " + y + ")", logStr2 = "";

            if (x <= MinBoundary)
            {
                if (clamp)
                    x = MinBoundary + 1;
                else
                    throw new IndexOutOfRangeException("[ X = " + x + " ] is out of range - must be greater than " + MinBoundary);
            }
            else if (x >= MaxBoundary)
            {
                if (clamp)
                    x = MaxBoundary - 1;
                else
                    throw new IndexOutOfRangeException("[ X = " + x + " ] is out of range - must be less than " + MaxBoundary);
            }
            if (y <= MinBoundary)
            {
                if (clamp)
                    y = MinBoundary + 1;
                else
                    throw new IndexOutOfRangeException("[ Y = " + y + " ] is out of range - must be greater than " + MinBoundary);
            }
            else if (y >= MaxBoundary)
            {
                if (clamp)
                    y = MaxBoundary - 1;
                else
                    throw new IndexOutOfRangeException("[ Y = " + y + " ] is out of range - must be less than " + MaxBoundary);
            }

            logStr2 += "[2] Offset: (" + offset.x + ", " + offset.y + ")";

            if (x < 0)
                PadNegX(x);
            if (y < 0)
                PadNegY(y);

            logStr2 += " --> (" + offset.x + ", " + offset.y + ")";

            int xTrue = x - offset.x, yTrue = y - offset.y;
            PadX(xTrue + 1);
            PadY(yTrue + 1);

            logStr1 += " --> (" + xTrue + ", " + yTrue + ")";

            //UnityEngine.Debug.Log(logStr1 + "\n" + logStr2);

            if (contents.InBounds(yTrue))
            {
                if (contents[yTrue].InBounds(xTrue))
                {
                    if (contents[yTrue][xTrue] == null)
                        contents[yTrue][xTrue] = NewItem();

                    return contents[yTrue][xTrue].value;
                }
                else
                    throw new IndexOutOfRangeException("Value out of range: reversing offset of " + offset.ToString(false) + " for x = " + x + " gives " + xTrue + ", while target row's width is " + contents[yTrue].Count);
            }
            else
                throw new IndexOutOfRangeException("Value out of range: reversing offset of " + offset.ToString(false) + " for y = " + y + " gives " + yTrue + ", while the grid's height is " + contents.Count);
        }

        /// <summary>
        /// Sets the value at the given grid position, allowing for negative values.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="x">Position on the grid's X axis.</param>
        /// <param name="y">Position on the grid's Y axis.</param>
        /// <param name="clamp">Prevents out of range exception when set to "true".</param>
        /// <returns>The target value.</returns>
        /// <exception cref="IndexOutOfRangeException">X and Y values must both be from -1073741824 to 1073741823.</exception>
        public void OffsetSet(T value, int x, int y, bool clamp = true)
        {
            if (x <= MinBoundary)
            {
                if (clamp)
                    x = MinBoundary + 1;
                else
                    throw new IndexOutOfRangeException("[ X = " + x + " ] is out of range - must be greater than " + MinBoundary);
            }
            else if (x >= MaxBoundary)
            {
                if (clamp)
                    x = MaxBoundary - 1;
                else
                    throw new IndexOutOfRangeException("[ X = " + x + " ] is out of range - must be less than " + MaxBoundary);
            }
            if (y <= MinBoundary)
            {
                if (clamp)
                    y = MinBoundary + 1;
                else
                    throw new IndexOutOfRangeException("[ Y = " + y + " ] is out of range - must be greater than " + MinBoundary);
            }
            else if (y >= MaxBoundary)
            {
                if (clamp)
                    y = MaxBoundary - 1;
                else
                    throw new IndexOutOfRangeException("[ Y = " + y + " ] is out of range - must be less than " + MaxBoundary);
            }

            if (x < 0)
                PadNegX(x);
            if (y < 0)
                PadNegY(y);

            int xTrue = x - offset.x, yTrue = y - offset.y;
            PadX(xTrue + 1);
            PadY(yTrue + 1);

            if (contents[yTrue][xTrue] == null)
                contents[yTrue][xTrue] = NewItem();

            contents[yTrue][xTrue].value = value;
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        /// <summary>
        /// Accesses the value at the given grid position, allowing for negative values.
        /// </summary>
        /// <remarks>Positional values are clamped for this accessor.</remarks>
        /// <param name="x">Position on the grid's X axis, from -1073741824 to 1073741823.</param>
        /// <param name="y">Position on the grid's Y axis, from -1073741824 to 1073741823.</param>
        /// <returns>The target value.</returns>
        public override T this[int x, int y] { get { return OffsetGet(x, y); } set { OffsetSet(value, x, y); } }

        /// <summary>
        /// Accesses the value at the given grid position, allowing for negative values.
        /// </summary>
        /// <remarks>Positional values are clamped for this accessor.</remarks>
        /// <param name="pos">Position on the grid. Both X and Y must be from -1073741824 to 1073741823.</param>
        /// <returns>The target value.</returns>
        public override T this[IVec2Int pos] { get { return OffsetGet(pos.x, pos.y); } set { OffsetSet(value, pos.x, pos.y); } }

        public override void Clear()
        {
            contents.Clear();
            rowEnds.Clear();
            size = UVec2Int.zero;
            offset = Vec2Int.zero;
        }
    }

    public enum HexGridDirection
    {
        INVALID = -1,
        Up,     UpRight,    DownRight,
        Down,   DownLeft,   UpLeft,
    }

    public static class HexGrid2D
    {
        public static int[] Adjacent(int x, int y, HexGridDirection direction)
        {
            if (x % 2 == 0)
            {
                switch (direction)
                {
                    default: return new int[] { x, y };
                    // +0, +1
                    case HexGridDirection.Up:        return new int[] { x + 0, y + 1 };
                    // +1, +0
                    case HexGridDirection.UpRight:   return new int[] { x + 1, y + 0 };
                    // +1, -1
                    case HexGridDirection.DownRight: return new int[] { x + 1, y - 1 };
                    // +0, -1
                    case HexGridDirection.Down:      return new int[] { x + 0, y - 1 };
                    // -1, -1
                    case HexGridDirection.DownLeft:  return new int[] { x - 1, y - 1 };
                    // -1, +0
                    case HexGridDirection.UpLeft:    return new int[] { x - 1, y + 0 };
                }
            }
            else
            {
                switch (direction)
                {
                    default: return new int[] { x, y };
                    // +0, +1
                    case HexGridDirection.Up:        return new int[] { x + 0, y + 1 };
                    // +1, +1
                    case HexGridDirection.UpRight:   return new int[] { x + 1, y + 1 };
                    // +1, +0
                    case HexGridDirection.DownRight: return new int[] { x + 1, y + 0 };
                    // +0, -1
                    case HexGridDirection.Down:      return new int[] { x + 0, y + 1 };
                    // -1, +0
                    case HexGridDirection.DownLeft:  return new int[] { x - 1, y + 0 };
                    // -1, +1
                    case HexGridDirection.UpLeft:    return new int[] { x - 1, y + 1 };
                }
            }
        }
        
        public static Vec2Int Adjacent(Vec2Int position, HexGridDirection direction)
        {
            if (position.x % 2 == 0)
            {
                switch (direction)
                {
                    default: return position;
                    // +0, +1
                    case HexGridDirection.Up:        return new Vec2Int(position, 0, 1);
                    // +1, +0
                    case HexGridDirection.UpRight:   return new Vec2Int(position, 1, 0);
                    // +1, -1
                    case HexGridDirection.DownRight: return new Vec2Int(position, 1, -1);
                    // +0, -1
                    case HexGridDirection.Down:      return new Vec2Int(position, 0, -1);
                    // -1, -1
                    case HexGridDirection.DownLeft:  return new Vec2Int(position, -1, -1);
                    // -1, +0
                    case HexGridDirection.UpLeft:    return new Vec2Int(position, -1, 0);
                }
            }
            else
            {
                switch (direction)
                {
                    default: return position;
                    // +0, +1
                    case HexGridDirection.Up:        return new Vec2Int(position, 0, 1);
                    // +1, +1
                    case HexGridDirection.UpRight:   return new Vec2Int(position, 1, 1);
                    // +1, +0
                    case HexGridDirection.DownRight: return new Vec2Int(position, 1, 0);
                    // +0, -1
                    case HexGridDirection.Down:      return new Vec2Int(position, 0, -1);
                    // -1, +0
                    case HexGridDirection.DownLeft:  return new Vec2Int(position, -1, 0);
                    // -1, +1
                    case HexGridDirection.UpLeft:    return new Vec2Int(position, -1, 1);
                }
            }
        }

        public static int[,] AllAdjacent(int x, int y)
        {
            int[,] positions = new int[6, 2];
            if (x % 2 == 0)
            {
                // +0, +1
                positions[0, 0] = x + 0; positions[0, 1] = y + 1;
                // +1, +0
                positions[1, 0] = x + 1; positions[1, 1] = y + 0;
                // +1, -1
                positions[2, 0] = x + 1; positions[2, 1] = y - 1;
                // +0, -1
                positions[3, 0] = x + 0; positions[3, 1] = y - 1;
                // -1, -1
                positions[4, 0] = x - 1; positions[4, 1] = y - 1;
                // -1, +0
                positions[5, 0] = x - 1; positions[5, 1] = y + 0;
            }
            else
            {
                // +0, +1
                positions[0, 0] = x + 0; positions[0, 1] = y + 1;
                // +1, +1
                positions[1, 0] = x + 1; positions[1, 1] = y + 1;
                // +1, +0
                positions[2, 0] = x + 1; positions[2, 1] = y - 0;
                // +0, -1
                positions[3, 0] = x + 0; positions[3, 1] = y - 1;
                // -1, +0
                positions[4, 0] = x - 1; positions[4, 1] = y - 0;
                // -1, +1
                positions[5, 0] = x - 1; positions[5, 1] = y + 1;
            }
            return positions;
        }
        
        public static Vec2Int[] AllAdjacent(Vec2Int position)
        {
            Vec2Int[] positions = new Vec2Int[6];
            if (position.x % 2 == 0)
            {
                // +0, +1
                positions[0] = new Vec2Int(position, 0, 1);
                // +1, +0
                positions[1] = new Vec2Int(position, 1, 0);
                // +1, -1
                positions[2] = new Vec2Int(position, 1, -1);
                // +0, -1
                positions[3] = new Vec2Int(position, 0, -1);
                // -1, -1
                positions[4] = new Vec2Int(position, -1, -1);
                // -1, +0
                positions[5] = new Vec2Int(position, -1, 0);
            }
            else
            {
                // +0, +1
                positions[0] = new Vec2Int(position, 0, 1);
                // +1, +1
                positions[1] = new Vec2Int(position, 1, 1);
                // +1, +0
                positions[2] = new Vec2Int(position, 1, 0);
                // +0, -1
                positions[3] = new Vec2Int(position, 0, -1);
                // -1, +0
                positions[4] = new Vec2Int(position, -1, 0);
                // -1, +1
                positions[5] = new Vec2Int(position, -1, 1);
            }
            return positions;
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static HexGridDirection Direction(Vec2Int checkFrom, Vec2Int checkTo)
        {
            Vec2Int disp = checkTo - checkFrom;
            if (checkFrom.x % 2 == 0)
            {
                if (disp.x > 0)
                {
                    if (disp.y < 0) // +1, -1
                        return HexGridDirection.DownRight;
                    else if (disp.y == 0) // +1, +0
                        return HexGridDirection.UpRight;
                }
                else if (disp.x < 0)
                {
                    if (disp.y > 0) // +0, +1
                        return HexGridDirection.Up;
                    else if (disp.y < 0) // +0, -1
                        return HexGridDirection.Down;
                }
                else
                {
                    if (disp.y < 0) // -1, -1
                        return HexGridDirection.DownLeft;
                    else if (disp.y == 0) // -1, +0
                        return HexGridDirection.UpLeft;
                }
            }
            else
            {
                if (disp.x > 0)
                {
                    if (disp.y > 0) // 1, 1
                        return HexGridDirection.UpRight;
                    else if (disp.y == 0) // 1, 0
                        return HexGridDirection.DownRight;
                }
                else if (disp.x < 0)
                {
                    if (disp.y > 0) // 0, 1
                        return HexGridDirection.Up;
                    else if (disp.y < 0) // 0, -1
                        return HexGridDirection.Down;
                }
                else
                {
                    if (disp.y > 0) // -1, 1
                        return HexGridDirection.UpLeft;
                    else if (disp.y == 0) // -1, 0
                        return HexGridDirection.DownLeft;
                }
            }
            return HexGridDirection.INVALID;
        }

        public static HexGridDirection Invert(this HexGridDirection direction)
        {
            switch (direction)
            {
                default: return HexGridDirection.INVALID;

                case HexGridDirection.Up:        return HexGridDirection.Down;
                case HexGridDirection.UpRight:   return HexGridDirection.DownLeft;
                case HexGridDirection.DownRight: return HexGridDirection.UpLeft;
                case HexGridDirection.Down:      return HexGridDirection.Up;
                case HexGridDirection.DownLeft:  return HexGridDirection.UpRight;
                case HexGridDirection.UpLeft:    return HexGridDirection.DownRight;
            }
        }

        private static HexGridDirection[,] rotationTable = new HexGridDirection[6, 6]
        {      /*  +0                          +1                          +2                          +3                          +4                          +5                         */
       /* Up */ { HexGridDirection.Up,        HexGridDirection.UpRight,   HexGridDirection.DownRight, HexGridDirection.Down,      HexGridDirection.DownLeft,  HexGridDirection.UpLeft,    },
  /* UpRight */ { HexGridDirection.UpRight,   HexGridDirection.DownRight, HexGridDirection.Down,      HexGridDirection.DownLeft,  HexGridDirection.UpLeft,    HexGridDirection.Up,        },
/* DownRight */ { HexGridDirection.DownRight, HexGridDirection.Down,      HexGridDirection.DownLeft,  HexGridDirection.UpLeft,    HexGridDirection.Up,        HexGridDirection.UpRight,   },
     /* Down */ { HexGridDirection.Down,      HexGridDirection.DownLeft,  HexGridDirection.UpLeft,    HexGridDirection.Up,        HexGridDirection.UpRight,   HexGridDirection.DownRight, },
 /* DownLeft */ { HexGridDirection.DownLeft,  HexGridDirection.UpLeft,    HexGridDirection.Up,        HexGridDirection.UpRight,   HexGridDirection.DownRight, HexGridDirection.Down,      },
   /* UpLeft */ { HexGridDirection.UpLeft,    HexGridDirection.Up,        HexGridDirection.UpRight,   HexGridDirection.DownRight, HexGridDirection.Down,      HexGridDirection.DownLeft,  },
        };
        /// <summary>
        /// Rotates the input direction by the amount specified. Rotation amount is clamped -6 to 6 inclusive.
        /// </summary>
        /// <param name="direction">The initial direction.</param>
        /// <param name="rotation">The rotation amount, from -6 to 6 inclusive.</param>
        /// <returns>The rotated direction.</returns>
        public static HexGridDirection Rotate(this HexGridDirection direction, int rotation)
        {
            if (rotation >= 6 || rotation <= -6)
                return direction;
            else if (rotation < 0)
                rotation += 6;
            return rotationTable[(int)direction, rotation];
        }

        private static int[,] dirCompareTable = new int[6, 6]
        {  /* 0  1  2  3  4  5  */
    /* 0 */ { 0, 1, 2, 3, 2, 1, },
    /* 1 */ { 1, 0, 1, 2, 3, 2, },
    /* 2 */ { 2, 1, 0, 1, 2, 3, },
    /* 3 */ { 3, 2, 1, 0, 1, 2, },
    /* 4 */ { 2, 3, 2, 1, 0, 1, },
    /* 5 */ { 1, 2, 3, 2, 1, 0, }
        };
        public static int Compare(this HexGridDirection checkFrom, HexGridDirection checkTo)
        {
            if (checkFrom == HexGridDirection.INVALID || checkTo == HexGridDirection.INVALID)
                return -1;
            return dirCompareTable[(int)checkFrom, (int)checkTo];
        }
    }
    
    public class HexGrid2D<T> : Grid2D<T>
    {
        public HexGrid2D(T valueOnNew = default)
        {
            this.valueOnNew = valueOnNew;
        }

        public HexGrid2D(Callback<T, T> onNewCallback)
        {
            valueOnNew = default;
            this.onNewCallback = onNewCallback;
        }

        public T AdjacentValue(int x, int y, HexGridDirection direction)
        {
            if (x % 2 == 0)
            {
                switch (direction)
                {
                    default:
                    // +0, +1
                    case HexGridDirection.Up:
                        return this[x + 0, y + 1];
                    // +1, +0
                    case HexGridDirection.UpRight:
                        return this[x + 1, y + 0];
                    // +1, -1
                    case HexGridDirection.DownRight:
                        return this[x + 1, y - 1];
                    // +0, -1
                    case HexGridDirection.Down:
                        return this[x + 0, y - 1];
                    // -1, -1
                    case HexGridDirection.DownLeft:
                        return this[x - 1, y - 1];
                    // -1, +0
                    case HexGridDirection.UpLeft:
                        return this[x - 1, y + 0];
                }
            }
            else
            {
                switch (direction)
                {
                    default:
                    // +0, +1
                    case HexGridDirection.Up:
                        return this[x + 0, y + 1];
                    // +1, +1
                    case HexGridDirection.UpRight:
                        return this[x + 1, y + 1];
                    // +1, +0
                    case HexGridDirection.DownRight:
                        return this[x + 1, y + 0];
                    // +0, -1
                    case HexGridDirection.Down:
                        return this[x + 0, y + 1];
                    // -1, +0
                    case HexGridDirection.DownLeft:
                        return this[x - 1, y + 0];
                    // -1, +1
                    case HexGridDirection.UpLeft:
                        return this[x - 1, y + 1];
                }
            }
        }
        
        public T AdjacentValue(Vec2Int position, HexGridDirection direction)
        {
            if (position.x % 2 == 0)
            {
                switch (direction)
                {
                    default:
                    // +0, +1
                    case HexGridDirection.Up:
                        return this[position.x + 0, position.y + 1];
                    // +1, +0
                    case HexGridDirection.UpRight:
                        return this[position.x + 1, position.y + 0];
                    // +1, -1
                    case HexGridDirection.DownRight:
                        return this[position.x + 1, position.y - 1];
                    // +0, -1
                    case HexGridDirection.Down:
                        return this[position.x + 0, position.y - 1];
                    // -1, -1
                    case HexGridDirection.DownLeft:
                        return this[position.x - 1, position.y - 1];
                    // -1, +0
                    case HexGridDirection.UpLeft:
                        return this[position.x - 1, position.y + 0];
                }
            }
            else
            {
                switch (direction)
                {
                    default:
                    // +0, +1
                    case HexGridDirection.Up:
                        return this[position.x + 0, position.y + 1];
                    // +1, +1
                    case HexGridDirection.UpRight:
                        return this[position.x + 1, position.y + 1];
                    // +1, +0
                    case HexGridDirection.DownRight:
                        return this[position.x + 1, position.y + 0];
                    // +0, -1
                    case HexGridDirection.Down:
                        return this[position.x + 0, position.y - 1];
                    // -1, +0
                    case HexGridDirection.DownLeft:
                        return this[position.x - 1, position.y + 0];
                    // -1, +1
                    case HexGridDirection.UpLeft:
                        return this[position.x - 1, position.y + 1];
                }
            }
        }

        public T[] AllAdjacentValues(int x, int y)
        {
            T[] values = new T[6];
            if (x % 2 == 0)
            {
                values[0] = this[x + 0, y + 1];
                // +1, +0
                values[1] = this[x + 1, y + 0];
                // +1, -1
                values[2] = this[x + 1, y - 1];
                // +0, -1
                values[3] = this[x + 0, y - 1];
                // -1, -1
                values[4] = this[x - 1, y - 1];
                // -1, +0
                values[5] = this[x - 1, y + 0];
            }
            else
            {
                // +0, +1
                values[0] = this[x + 0, y + 1];
                // +1, +1
                values[1] = this[x + 1, y + 1];
                // +1, +0
                values[2] = this[x + 1, y + 0];
                // +0, -1
                values[3] = this[x + 0, y + 1];
                // -1, +0
                values[4] = this[x - 1, y + 0];
                // -1, +1
                values[5] = this[x - 1, y + 1];
            }
            return values;
        }
        
        public T[] AllAdjacentValues(Vec2Int position)
        {
            T[] values = new T[6];
            if (position.x % 2 == 0)
            {
                values[0] = this[position.x + 0, position.y + 1];
                // +1, +0
                values[1] = this[position.x + 1, position.y + 0];
                // +1, -1
                values[2] = this[position.x + 1, position.y - 1];
                // +0, -1
                values[3] = this[position.x + 0, position.y - 1];
                // -1, -1
                values[4] = this[position.x - 1, position.y - 1];
                // -1, +0
                values[5] = this[position.x - 1, position.y + 0];
            }
            else
            {
                // +0, +1
                values[0] = this[position.x + 0, position.y + 1];
                // +1, +1
                values[1] = this[position.x + 1, position.y + 1];
                // +1, +0
                values[2] = this[position.x + 1, position.y + 0];
                // +0, -1
                values[3] = this[position.x + 0, position.y + 1];
                // -1, +0
                values[4] = this[position.x - 1, position.y + 0];
                // -1, +1
                values[5] = this[position.x - 1, position.y + 1];
            }
            return values;
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // COLLECTION TYPES (non-exhaustive)
    /* Tags
    [R] --> Generic typing required
    [A] --> Generic typing available
    [U] --> Generic typing unavailable
    */
    /* VALUE-ONLY <T>
    
    Interfaces
     - [A] ICollection
     - [R] IReadOnlyCollection
     - [A] IList
     - [R] ReadOnlyList
     - [R] IProducerConsumerCollection
    Containers
     - [U] Array
     - [U] ArrayList [not recommended for use]
     - [R] List
     - [A] Queue
     - [R] ConcurrentQueue
     - [A] Stack
     - [R] ConcurrentStack
     - [R] LinkedList
     - [R] SortedSet
    
    */
    /* KEY-VALUE PAIR <Tkey, Tvalue>
    
    Interfaces
     - [A] IDictionary
     - [R] IReadOnlyDictionary
    Containers
     - [U] Hashtable
     - [U] SortedList
     - [R] SortedList
     - [R] Dictionary
     - [R] ConcurrentDictionary

    */

    public static class CollectionValidation
    {
        #region < ICollection >

        public static bool HasContents<T>(this ICollection<T> collection) => collection == null ? false : (collection.Count == 0 ? false : true);
        public static bool HasContents<Tkey, Tvalue>(this ICollection<KeyValuePair<Tkey, Tvalue>> collection) => collection == null ? false : (collection.Count == 0 ? false : true);

        #endregion


        #region < IList >

        public static bool InBounds<T>(this IList<T> collection, int index) => index < 0 ? false : (collection.HasContents() ? index < collection.Count : false);
        public static bool InBounds<Tkey, Tvalue>(this IList<KeyValuePair<Tkey, Tvalue>> collection, int index) => index < 0 ? false : (collection.HasContents() ? index < collection.Count : false);

        public static bool ExistsAt<T>(this IList<T> collection, int index) => collection.InBounds(index) ? (typeof(T).IsNullable() ? true : collection[index].Equals(null)) : false;

        #endregion


        #region < IDictionary >

        public static bool ExistsAt<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key) => collection.ContainsKey(key) ? (typeof(Tvalue).IsNullable() ? true : !collection[key].Equals(null)) : false;
        
        #endregion
    }

    public static class Ext_Collections
    {
        #region [ RETURN ADDED ]

        public static T ReturnAdd<T>(this IList<T> list, T item) { list.Add(item); return item; }
        public static KeyValuePair<Tkey, Tvalue> ReturnAdd<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> dictionary, Tkey key, Tvalue value) { dictionary.Add(key, value); return new KeyValuePair<Tkey, Tvalue>(key, value); }

        #endregion

        #region [ SIZE ADJUSTMENT ]

        public static T[] PadLeft<T>(this T[] collection, int padToTotal)
        {
            if (padToTotal > collection.Length)
            {
                T[] padded = new T[padToTotal];
                int i, n = collection.Length, p = padToTotal - n;
                for (i = 0; i < p; i++)
                {
                    padded[i] = default;
                }
                for (i = 0; i < n; i++)
                {
                    padded[i + p] = collection[i];
                }
                collection = padded;
            }
            return collection;
        }
        
        public static T[] PadLeft<T>(this T[] collection, int padToTotal, T padWith, bool padWithCopy = true)
        {
            if (padToTotal > collection.Length)
            {
                T[] padded = new T[padToTotal];
                int i, n = collection.Length, p = padToTotal - n;
                for (i = 0; i < p; i++)
                {
                    if (padWithCopy)
                        padded[i] = New<T>.InstanceCopy(padWith);
                    else
                        padded[i] = padWith;
                }
                for (i = 0; i < n; i++)
                {
                    padded[i + p] = collection[i];
                }
                collection = padded;
            }
            return collection;
        }
        
        public static List<T> PadLeft<T>(this List<T> collection, int padToTotal)
        {
            if (padToTotal > collection.Count)
            {
                int i = 0, n = padToTotal - collection.Count;
                for (; i < n; i++)
                {
                    collection.Insert(0, default);
                }
            }
            return collection;
        }
        
        public static List<T> PadLeft<T>(this List<T> collection, int padToTotal, T padWith, bool padWithCopy = true)
        {
            if (padToTotal > collection.Count)
            {
                int i = 0, n = padToTotal - collection.Count;
                for (; i < n; i++)
                {
                    if (padWithCopy)
                        collection.Insert(0, New<T>.InstanceCopy(padWith));
                    else
                        collection.Insert(0, padWith);
                }
            }
            return collection;
        }
        
        public static List<List<T>> PadLeftLists<T>(this List<List<T>> collection, int padToTotal)
        {
            if (padToTotal > collection.Count)
            {
                int i = 0, n = padToTotal - collection.Count;
                for (; i < n; i++)
                {
                    collection.Insert(0, new List<T>());
                }
            }
            return collection;
        }

        public static T[] PadRight<T>(this T[] collection, int padToTotal)
        {
            if (padToTotal > collection.Length)
            {
                T[] padded = new T[padToTotal];
                int i, n = collection.Length;
                for (i = 0; i < n; i++)
                {
                    padded[i] = collection[i];
                }
                for (i = n; i < padToTotal; i++)
                {
                    padded[i] = default;
                }
                collection = padded;
            }
            return collection;
        }
        
        public static T[] PadRight<T>(this T[] collection, int padToTotal, T padWith, bool padWithCopy = true)
        {
            if (padToTotal > collection.Length)
            {
                T[] padded = new T[padToTotal];
                int i, n = collection.Length;
                for (i = 0; i < n; i++)
                {
                    padded[i] = collection[i];
                }
                for (i = n; i < padToTotal; i++)
                {
                    if (padWithCopy)
                        padded[i] = New<T>.InstanceCopy(padWith);
                    else
                        padded[i] = padWith;
                }
                collection = padded;
            }
            return collection;
        }

        public static List<T> PadRight<T>(this List<T> collection, int padToTotal)
        {
            if (padToTotal > collection.Count)
            {
                int i = 0, n = padToTotal - collection.Count;
                for (; i < n; i++)
                {
                    collection.Add(default);
                }
            }
            return collection;
        }
        
        public static List<T> PadRight<T>(this List<T> collection, int padToTotal, T padWith, bool padWithCopy = true)
        {
            if (padToTotal > collection.Count)
            {
                int i = 0, n = padToTotal - collection.Count;
                for (; i < n; i++)
                {
                    if (padWithCopy)
                        collection.Add(New<T>.InstanceCopy(padWith));
                    else
                        collection.Add(padWith);
                }
            }
            return collection;
        }

        public static List<List<T>> PadRightLists<T>(this List<List<T>> collection, int padToTotal)
        {
            if (padToTotal > collection.Count)
            {
                int i = 0, n = padToTotal - collection.Count;
                for (; i < n; i++)
                {
                    collection.Add(new List<T>());
                }
            }
            return collection;
        }

        public static T[] PadArray<T>(this T[] array, int padLength = 1, T padValue = default, bool padAtFront = false)
        {
            if (padAtFront)
                return array.PadLeft(array.Length + padLength, padValue, true);
            else
                return array.PadRight(array.Length + padLength, padValue, true);
        }
            
        public static List<T> PadList<T>(this List<T> list, int padCount = 1, T padValue = default, bool padAtFront = false)
        {
            if (padAtFront)
                return list.PadLeft(list.Count + padCount, padValue, true);
            else
                return list.PadRight(list.Count + padCount, padValue, true);
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

        public static T[] Copy<T>(T[] source, T[] destination, bool overwrite = false)
        {
            if (overwrite)
            {
                destination = new T[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    destination[i] = source[i];
                }
                return destination;
            }
            else
            {
                T[] result = new T[source.Length + destination.Length];
                for (int i = 0; i < destination.Length; i++)
                {
                    result[i] = destination[i];
                }
                for (int i = 0, i2 = destination.Length; i < source.Length; i++, i2++)
                {
                    result[i2] = destination[i];
                }
                destination = result;
                return result;
            }
        }
        public static void Copy<T>(IList<T> source, IList<T> destination, bool overwrite = false)
        {
            if (overwrite)
                destination.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
        }

        public static T[] CopyTo<T>(this T[] source, T[] destination, bool overwrite = false)
        {
            if (overwrite)
            {
                destination = new T[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    destination[i] = source[i];
                }
                return destination;
            }
            else
            {
                T[] result = new T[source.Length + destination.Length];
                for (int i = 0; i < destination.Length; i++)
                {
                    result[i] = destination[i];
                }
                for (int i = 0, i2 = destination.Length; i < source.Length; i++, i2++)
                {
                    result[i2] = destination[i];
                }
                destination = result;
                return result;
            }
        }
        public static void CopyTo<T>(this IList<T> source, IList<T> destination, bool overwrite = false)
        {
            if (overwrite)
                destination.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
        }

        public static T[] CopyFrom<T>(this T[] destination, T[] source, bool overwrite = false)
        {
            if (overwrite)
            {
                destination = new T[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    destination[i] = source[i];
                }
                return destination;
            }
            else
            {
                T[] result = new T[source.Length + destination.Length];
                for (int i = 0; i < destination.Length; i++)
                {
                    result[i] = destination[i];
                }
                for (int i = 0, i2 = destination.Length; i < source.Length; i++, i2++)
                {
                    result[i2] = destination[i];
                }
                destination = result;
                return result;
            }
        }
        public static void CopyFrom<T>(this IList<T> destination, IList<T> source, bool overwrite = false)
        {
            if (overwrite)
                destination.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
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

        #region [ COUNT IF ]

        public static int CountIf<T>(this T[] collection, T checkValue, bool invert = false)
        {
            int count = 0;
            foreach (T value in collection)
            {
                if ((value.Equals(checkValue) && !invert) || (!value.Equals(checkValue) && invert))
                    count++;
            }
            return count;
        }
        
        public static int CountIf<T>(this List<T> collection, T checkValue, bool invert = false)
        {
            int count = 0;
            foreach (T value in collection)
            {
                if ((value.Equals(checkValue) && !invert) || (!value.Equals(checkValue) && invert))
                    count++;
            }
            return count;
        }
        
        public static int CountIf(this bool[] collection, bool checkAgainst)
        {
            int count = 0;
            foreach (bool value in collection)
            {
                if (value == checkAgainst)
                    count++;
            }
            return count;
        }
        
        public static int CountIf(this List<bool> collection, bool checkAgainst)
        {
            int count = 0;
            foreach (bool value in collection)
            {
                if (value == checkAgainst)
                    count++;
            }
            return count;
        }

        #endregion

        /// <summary>
        /// Checks if a collection contains a key, and removes the corresponding key-value pair if it does.
        /// </summary>
        /// <typeparam name="Tkey">Key type<./typeparam>
        /// <typeparam name="Tvalue">Value type.</typeparam>
        /// <param name="collection">The target collection.</param>
        /// <param name="key">The key to check for.</param>
        /// <returns>
        /// <b>true</b> if a key-value pair was successfully removed.
        /// </returns>
        public static bool TryRemove<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key)
        {
            if (collection.ContainsKey(key))
            {
                collection.Remove(key);
                return true;
            }
            else
                return false;
        }

        public static bool AddedNull<T>(this IList<T> collection, T value) => collection.ReturnAdd(value) == null;

        public static bool AddUnlessNull<T>(this IList<T> collection, T value)
        {
            if (value != null)
            {
                collection?.Add(value);
                return collection != null;
            }
            return false;
        }

        public static bool RemoveIfNull<T>(this IList<T> collection, int index)
        {
            if (collection.InBounds(index) && typeof(T).IsNullable() && collection[index] == null)
            {
                collection.RemoveAt(index);
                return true;
            }
            return false;
        }
        
        public static bool RemoveIfNull<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key)
        {
            if (collection.ContainsKey(key) && typeof(Tvalue).IsNullable() && collection[key] == null)
            {
                collection.Remove(key);
                return true;
            }
            return false;
        }

        public static void RemoveAllNull<T>(this IList<T> collection)
        {
            if (typeof(T).IsNullable())
            {
                for (int i = collection.Count - 1; i >= 0; i--)
                {
                    if (collection[i] == null)
                        collection.RemoveAt(i);
                }
            }
        }

        public static void RemoveAllNull<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection)
        {
            if (typeof(Tvalue).IsNullable())
            {
                foreach (Tkey key in collection.Keys)
                {
                    if (collection[key] == null)
                        collection.Remove(key);
                }
            }
        }

        public static void AddMultiple<T>(this IList<T> collection, T item, int count = 2)
        {
            if (count < 1)
                collection.Add(item);
            else for (int i = 0; i < count; i++)
            {
                collection.Add(item);
            }
        }
        
        public static void InsertMultiple<T>(this IList<T> collection, int index, T item, int count = 2)
        {
            if (index < 0)
                index = 0;
            else if (index >= collection.Count)
                index = collection.Count - 1;
            if (count < 1)
                collection.Insert(index, item);
            else for (int i = 0; i < count; i++, index++)
            {
                collection.Insert(index, item);
            }
        }

        public static void MultiAddRange<T>(this List<T> list, List<T> item, int count = 2)
        {
            if (count < 1)
                list.AddRange(item);
            else for (int i = 0; i < count; i++)
            {
                    list.AddRange(item);
            }
        }

        public static void AddRanges<T>(this List<T> list, List<T> firstToAdd, params List<T>[] toAdd)
        {
            list.AddRange(firstToAdd);
            foreach (List<T> add in toAdd)
            {
                list.AddRange(add);
            }
        }

        public static KeyValuePair<Tkey, Tvalue> GetOrAdd<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key, Tvalue value)
        {
            if (collection.ContainsKey(key))
                return new KeyValuePair<Tkey, Tvalue>(key, collection[key]);
            else
                return collection.ReturnAdd(key, value);
        }

        public static Tvalue GetOrRemove<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key) where Tvalue : class
        {
            if (collection.ContainsKey(key))
            {
                Tvalue result = collection[key];
                if (result == null)
                    collection.Remove(key);
                return result;
            }
            return null;
        }

        public static void Transfer<T>(this List<T> source, int sourceIndex, List<T> destination)
        {
            if (source.InBounds(sourceIndex))
            {
                destination.Add(source[sourceIndex]);
                source.RemoveAt(sourceIndex);
            }
        }
        public static void Transfer<T>(this List<T> source, int sourceIndex, List<T> destination, int destinationIndex)
        {
            if (source.InBounds(sourceIndex))
            {
                if (destination.InBounds(destinationIndex))
                {
                    destination.Insert(destinationIndex, source[sourceIndex]);
                    source.RemoveAt(sourceIndex);
                }
                else
                {
                    destination.Add(source[sourceIndex]);
                    source.RemoveAt(sourceIndex);
                }
            }
        }

        public static void TransferAll<T>(this List<T> source, List<T> destination)
        {
            int i = 0, x = source.Count;
            for (; i < x; i++)
            {
                destination.Add(source[0]);
                source.RemoveAt(0);
            }
        }
        public static void TransferAll<T>(this List<T> source, List<T> destination, int insertFrom)
        {
            if (destination.InBounds(insertFrom))
            {
                int i = insertFrom, x = insertFrom + source.Count;
                for (; i < x; i++)
                {
                    destination.Insert(i, source[0]);
                    source.RemoveAt(0);
                }
            }
            else
            {
                int i = 0, x = source.Count;
                for (; i < x; i++)
                {
                    destination.Add(source[0]);
                    source.RemoveAt(0);
                }
            }
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

        public static T[] Insert<T>(this T[] array, int index, T item)
        {
            if (array.InBounds(index))
            {
                for (int i = array.Length - 1; i >= index; i--)
                {
                    if (i > index)
                        array[i] = array[i - 1];
                    else
                        array[i] = item;
                }
            }
            return array;
        }

        public static List<T> InsertAndTrim<T>(this List<T> list, int index, T item, int maxCount)
        {
            if (maxCount > 0 && list.InBounds(index) && index < maxCount)
            {
                list.Insert(index, item);
                if (list.Count > maxCount)
                {
                    list.RemoveRange(maxCount, list.Count - maxCount);
                }
            }
            return list;
        }

        public static int[] GetIndices<T>(this T[] collection)
        {
            int[] indices = new int[collection.Length];
            for (int i = 0; i < collection.Length; i++)
            {
                indices[i] = i;
            }
            return indices;
        }
        public static int[] GetIndices<T>(this IList<T> collection)
        {
            int[] indices = new int[collection.Count];
            for (int i = 0; i < collection.Count; i++)
            {
                indices[i] = i;
            }
            return indices;
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

    public static class Ext_KVPCollection
    {
        public static Tkey[] Keys<Tkey, Tvalue>(this KeyValuePair<Tkey, Tvalue>[] array)
        {
            Tkey[] keys = new Tkey[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                keys[i] = array[i].Key;
            }
            return keys;
        }

        public static List<Tkey> Keys<Tkey, Tvalue>(this List<KeyValuePair<Tkey, Tvalue>> list)
        {
            List<Tkey> keys = new List<Tkey>();
            for (int i = 0; i < list.Count; i++)
            {
                keys.Add(list[i].Key);
            }
            return keys;
        }

        public static Tvalue[] Values<Tkey, Tvalue>(this KeyValuePair<Tkey, Tvalue>[] array)
        {
            Tvalue[] keys = new Tvalue[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                keys[i] = array[i].Value;
            }
            return keys;
        }

        public static List<Tvalue> Values<Tkey, Tvalue>(this List<KeyValuePair<Tkey, Tvalue>> list)
        {
            List<Tvalue> keys = new List<Tvalue>();
            for (int i = 0; i < list.Count; i++)
            {
                keys.Add(list[i].Value);
            }
            return keys;
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
        using NeoCambion.Unity;

        public static class Ext_UnityCollection
        {
            public static T RandFrom<T>(this T[] collection)
            {
                if (collection.Length <= 0)
                    throw new IndexOutOfRangeException();
                else
                    return collection[Random.Range(0, collection.Length)];
            }
            public static T RandFrom<T>(this IList<T> collection)
            {
                if (collection.Count <= 0)
                    throw new IndexOutOfRangeException();
                else
                    return collection[Random.Range(0, collection.Count)];
            }

            public static Object AddClone(this IList<Object> collection, Object original)
                => collection.ReturnAdd(original.Clone());
            public static Object AddClone(this IList<Object> collection, Object original, Transform parent) 
                => collection.ReturnAdd(original.Clone(parent));
            public static Object AddClone(this IList<Object> collection, Object original, Transform parent, bool instantiateInWorldSpace) 
                => collection.ReturnAdd(original.Clone(parent, instantiateInWorldSpace));
            public static Object AddClone(this IList<Object> collection, Object original, Vector3 position, Quaternion rotation) 
                => collection.ReturnAdd(original.Clone(position, rotation));
            public static Object AddClone(this IList<Object> collection, Object original, Vector3 position, Quaternion rotation, Transform parent) 
                => collection.ReturnAdd(original.Clone(position, rotation, parent));
            public static T AddClone<T>(this IList<T> collection, T original) where T : Object 
                => collection.ReturnAdd(original.Clone());
            public static T AddClone<T>(this IList<T> collection, T original, Transform parent) where T : Object 
                => collection.ReturnAdd(original.Clone(parent));
            public static T T<T>(this IList<T> collection, T original, Transform parent, bool instantiateInWorldSpace) where T : Object 
                => collection.ReturnAdd(original.Clone(parent, instantiateInWorldSpace));
            public static T AddClone<T>(this IList<T> collection, T original, Vector3 position, Quaternion rotation) where T : Object 
                => collection.ReturnAdd(original.Clone(position, rotation));
            public static T AddClone<T>(this IList<T> collection, T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object 
                => collection.ReturnAdd(original.Clone(position, rotation, parent));

            public static GameObject AddClone<TObject>(this IList<GameObject> collection, TObject original) where TObject : MonoBehaviour
                => collection.ReturnAdd(original.Clone().gameObject);
            public static GameObject AddClone<TObject>(this IList<GameObject> collection, TObject original, Transform parent) where TObject : MonoBehaviour
                => collection.ReturnAdd(original.Clone(parent).gameObject);
            public static GameObject AddClone<TObject>(this IList<GameObject> collection, TObject original, Transform parent, bool instantiateInWorldSpace) where TObject : MonoBehaviour
                => collection.ReturnAdd(original.Clone(parent, instantiateInWorldSpace).gameObject);
            public static GameObject AddClone<TObject>(this IList<GameObject> collection, TObject original, Vector3 position, Quaternion rotation) where TObject : MonoBehaviour
                => collection.ReturnAdd(original.Clone(position, rotation).gameObject);
            public static GameObject AddClone<TObject>(this IList<GameObject> collection, TObject original, Vector3 position, Quaternion rotation, Transform parent) where TObject : MonoBehaviour
                => collection.ReturnAdd(original.Clone(position, rotation, parent).gameObject);
            public static TList AddClone<TList>(this IList<TList> collection, GameObject original) where TList : Component
                => collection.ReturnAdd(original.Clone().GetOrAddComponent<TList>());
            public static TList AddClone<TList>(this IList<TList> collection, GameObject original, Transform parent) where TList : Component
                => collection.ReturnAdd(original.Clone(parent).GetOrAddComponent<TList>());
            public static TList AddClone<TList>(this IList<TList> collection, GameObject original, Transform parent, bool instantiateInWorldSpace) where TList : Component
                => collection.ReturnAdd(original.Clone(parent, instantiateInWorldSpace).GetOrAddComponent<TList>());
            public static TList AddClone<TList>(this IList<TList> collection, GameObject original, Vector3 position, Quaternion rotation) where TList : Component
                => collection.ReturnAdd(original.Clone(position, rotation).GetOrAddComponent<TList>());
            public static TList AddClone<TList>(this IList<TList> collection, GameObject original, Vector3 position, Quaternion rotation, Transform parent) where TList : Component
                => collection.ReturnAdd(original.Clone(position, rotation, parent).GetOrAddComponent<TList>());
            public static TList AddClone<TList, TObject>(this IList<TList> collection, TObject original) where TList : Component where TObject : Component
                => collection.ReturnAdd(original.Clone().gameObject.GetOrAddComponent<TList>());
            public static TList AddClone<TList, TObject>(this IList<TList> collection, TObject original, Transform parent) where TList : Component where TObject : Component
                => collection.ReturnAdd(original.Clone(parent).gameObject.GetOrAddComponent<TList>());
            public static TList AddClone<TList, TObject>(this IList<TList> collection, TObject original, Transform parent, bool instantiateInWorldSpace) where TList : Component where TObject : Component
                => collection.ReturnAdd(original.Clone(parent, instantiateInWorldSpace).gameObject.GetOrAddComponent<TList>());
            public static TList AddClone<TList, TObject>(this IList<TList> collection, TObject original, Vector3 position, Quaternion rotation) where TList : Component where TObject : Component
                => collection.ReturnAdd(original.Clone(position, rotation).gameObject.GetOrAddComponent<TList>());
            public static TList AddClone<TList, TObject>(this IList<TList> collection, TObject original, Vector3 position, Quaternion rotation, Transform parent) where TList : Component where TObject : Component
                => collection.ReturnAdd(original.Clone(position, rotation, parent).gameObject.GetOrAddComponent<TList>());
            
            public static KeyValuePair<Tkey, Tvalue> AddCloneKey<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey originalKey, Tvalue value) where Tkey : Object 
                => collection.ReturnAdd(originalKey.Clone(), value);
            public static KeyValuePair<Tkey, Tvalue> AddCloneKey<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey originalKey, Tvalue value, Transform parent) where Tkey : Object 
                => collection.ReturnAdd(originalKey.Clone(parent), value);
            public static KeyValuePair<Tkey, Tvalue> AddCloneKey<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey originalKey, Tvalue value, Transform parent, bool instantiateInWorldSpace) where Tkey : Object 
                => collection.ReturnAdd(originalKey.Clone(parent, instantiateInWorldSpace), value);
            public static KeyValuePair<Tkey, Tvalue> AddCloneKey<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey originalKey, Tvalue value, Vector3 position, Quaternion rotation) where Tkey : Object 
                => collection.ReturnAdd(originalKey.Clone(position, rotation), value);
            public static KeyValuePair<Tkey, Tvalue> AddCloneKey<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey originalKey, Tvalue value, Vector3 position, Quaternion rotation, Transform parent) where Tkey : Object 
                => collection.ReturnAdd(originalKey.Clone(position, rotation, parent), value);

            public static KeyValuePair<Tkey, Tvalue> AddCloneValue<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key, Tvalue originalValue) where Tvalue : Object
                => collection.ReturnAdd(key, originalValue.Clone());
            public static KeyValuePair<Tkey, Tvalue> AddCloneValue<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key, Tvalue originalValue, Transform parent) where Tvalue : Object
                => collection.ReturnAdd(key, originalValue.Clone(parent));
            public static KeyValuePair<Tkey, Tvalue> AddCloneValue<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key, Tvalue originalValue, Transform parent, bool instantiateInWorldSpace) where Tvalue : Object
                => collection.ReturnAdd(key, originalValue.Clone(parent, instantiateInWorldSpace));
            public static KeyValuePair<Tkey, Tvalue> AddCloneValue<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key, Tvalue originalValue, Vector3 position, Quaternion rotation) where Tvalue : Object
                => collection.ReturnAdd(key, originalValue.Clone(position, rotation));
            public static KeyValuePair<Tkey, Tvalue> AddCloneValue<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> collection, Tkey key, Tvalue originalValue, Vector3 position, Quaternion rotation, Transform parent) where Tvalue : Object
                => collection.ReturnAdd(key, originalValue.Clone(position, rotation, parent));
        }

        public static class Ext_ObjectCollection
        {
            public static bool RemoveAtAndDestroy<T>(this IList<T> collection, int removeAt) where T : Component
            {
                if (collection.InBounds(removeAt))
                {
                    T target = collection[removeAt];
                    collection.RemoveAt(removeAt);
                    if (target != null)
                    {
                        target.gameObject.DestroyThis();
                        return true;
                    }
                }
                return false;
            }

            public static bool RemoveLastAndDestroy<T>(this IList<T> collection) where T : Component
            {
                if (collection.Count > 0)
                {
                    T item = collection.Last();
                    collection.RemoveAt(collection.Count - 1);
                    if (item != null)
                    {
                        item.gameObject.DestroyThis();
                        return true;
                    }
                }
                return false;
            }

            public static int ClearAndDestroy(this IList<GameObject> collection)
            {
                int destroyed = 0;
                for (int i = collection.Count - 1; i >= 0; i--)
                {
                    if (collection[i] != null)
                    {
                        destroyed++;
                        collection[i].DestroyThis();
                    }
                }
                collection.Clear();
                return destroyed;
            }

            public static int ClearAndDestroy<T>(this IList<T> collection) where T : Component
            {
                int destroyed = 0;
                for (int i = collection.Count - 1; i >= 0; i--)
                {
                    if (collection[i] != null)
                    {
                        destroyed++;
                        collection[i].gameObject.DestroyThis();
                    }
                }
                collection.Clear();
                return destroyed;
            }
        }

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
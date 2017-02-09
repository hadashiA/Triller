using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{
    public enum Direction
    {
        Down, Up, Left, Right,
    }

    public class FieldRange : IEnumerable<FieldCoord>, IEquatable<FieldRange>
    {
        public static bool operator ==(FieldRange lhs, FieldRange rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return true;
            if ((object)lhs == null || (object)rhs == null) return false;
            if (lhs.Coords.Count != rhs.Coords.Count) return false;

            for (var i = 0; i < lhs.Count(); i++)
            {
                if (lhs.Coords[i] != rhs.Coords[i]) return false;
            }
            return true;
        }

        public static bool operator !=(FieldRange lhs, FieldRange rhs)
        {
            return !(lhs == rhs);
        }

        public bool IsEmpty
        {
            get { return Coords.Count <= 0; }
        }

        public List<FieldCoord> Coords { get; private set; }

        public FieldRange()
        {
            Coords = new List<FieldCoord>();
        }

        public void Add(FieldCoord coord)
        {
            Coords.Add(coord);
        }

        public bool Contains(FieldCoord coord)
        {
            return Coords.Contains(coord);
        }

        public IEnumerator<FieldCoord> GetEnumerator()
        {
            return Coords.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(FieldRange other)
        {
            if (other == null) return false;
            for (var i = 0; i < Coords.Count; i++)
            {
                if (other.Coords.Count - 1 < i)
                    return false;

                if (Coords[i].Row != other.Coords[i].Row || Coords[i].Col != other.Coords[i].Col)
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder("[");
            for (var i = 0; i < Coords.Count; i++)
            {
                stringBuilder.Append(Coords[i]);
            }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldCoord)
            {
                return Equals((FieldCoord) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            // TODO:
            return ToString().GetHashCode();
        }

    }

    [Serializable]
    public struct FieldCoord : IEquatable<FieldCoord>
    {
        public int Row;
        public int Col;

        public static bool operator ==(FieldCoord lhs, FieldCoord rhs)
        {
            return lhs.Row == rhs.Row && lhs.Col == rhs.Col;
        }

        public static bool operator !=(FieldCoord lhs, FieldCoord rhs)
        {
            return !(lhs == rhs);
        }

        public FieldCoord Up
        {
            get { return new FieldCoord(Col, Row - 1); }
        }

        public FieldCoord Down
        {
            get { return new FieldCoord(Col, Row + 1); }
        }

        public FieldCoord Left
        {
            get { return new FieldCoord(Col - 1, Row); }
        }

        public FieldCoord Right
        {
            get { return new FieldCoord(Col + 1, Row); }
        }

        public FieldCoord(int col, int row)
        {
            Row = row;
            Col = col;
        }

        public FieldCoord Shift(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Up;
                case Direction.Down:
                    return Down;
                case Direction.Left:
                    return Left;
                default:
                    return Right;
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", Col, Row);
        }

        public bool Equals(FieldCoord rhs)
        {
            return Row == rhs.Row && Col == rhs.Col;
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldCoord)
            {
                return Equals((FieldCoord) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            // TODO:
            return ToString().GetHashCode();
        }
    }
}
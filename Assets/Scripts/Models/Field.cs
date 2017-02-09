using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Services;

namespace Models
{
    public class Digging
    {
        public List<Block> Blocks { get; private set; }
        public IEnumerable<FieldCoord> Unbalances { get; private set; }

        public Digging(List<Block> blocks, IEnumerable<FieldCoord> unbalances)
        {
            Blocks = blocks;
            Unbalances = unbalances;
        }
    }

    public class Field : IEnumerable<FieldCoord>
    {
        enum CellType
        {
            Empty, Wall, Block
        }

        struct Cell
        {
            public int Id;
            public CellType Type;

            public static Cell Empty = new Cell {Type = CellType.Empty};

            public bool IsEmpty
            {
                get { return Id == 0; }
            }
        }

        public static Vector3 UpOffset = new Vector3(0f, 0.5f);
        public static Vector3 DownOffset = new Vector3(0f, -0.5f);
        public static Vector3 LeftOffset = new Vector3(-0.5f, 0f);
        public static Vector3 RightOffset = new Vector3(0.5f, 0f);

        public float FallSpeed { get; set; }
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        Cell[][] _grid;
        Dictionary<int, Block> _blocks = new Dictionary<int, Block>();
        BlockScanner _scanner;

        public static Vector3 Offset(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return UpOffset;
                case Direction.Down:
                    return DownOffset;
                case Direction.Left:
                    return LeftOffset;
                default:
                    return RightOffset;
            }
        }

        public Field(FieldCoord size)
        {
            Rows = size.Row;
            Cols = size.Col;
            FallSpeed = 5f;

            _scanner = new BlockScanner(this);

            _grid = new Cell[Rows][];
            for (var row = 0; row < Rows; row++)
            {
                _grid[row] = new Cell[Cols];
            }
        }

        public void AddBlock(FieldCoord coord, Block block)
        {
            _grid[coord.Row][coord.Col] = new Cell
            {
                Type = CellType.Block,
                Id = block.Id,
            };
            _blocks.Add(block.Id, block);
        }

        public FieldCoord GetCoord(Vector3 localPos)
        {
            return new FieldCoord((int)localPos.x, -(int)localPos.y);
        }

        public Vector3 GetLocalPosition(FieldCoord coord)
        {
            return new Vector3(coord.Col + 0.5f, -(coord.Row + 0.5f));
        }

        public Block GetBlock(FieldCoord coord)
        {
            var cell = GetCell(coord);
            return _blocks[cell.Id];
        }

        public bool IsEmpty(FieldCoord coord)
        {
            return GetCell(coord).IsEmpty;
        }

        public bool IsBlock(FieldCoord coord)
        {
            return GetCell(coord).Type == CellType.Block;
        }

        public bool IsItem(FieldCoord coord)
        {
            if (IsBlock(coord))
            {
                return GetBlock(coord).Color == BlockColor.Imo;
            }
            return false;
        }

        public bool CanMove(FieldCoord coord)
        {
            return IsEmpty(coord) || IsItem(coord);
        }

        public bool CanFall(FieldCoord coord, bool withItem = false)
        {
            if (CanMove(coord)) return true;
            if (!IsBlock(coord)) return false;

            var block = GetBlock(coord);
            return block.Falling || (withItem && block.Color == BlockColor.Imo);
        }

        public void SetFalling(FieldCoord coord, bool falling)
        {
            var group = _scanner.Grouping(coord);
            foreach (var member in group)
            {
                var block = GetBlock(member);
                block.Falling = falling;
                _blocks[block.Id] = block;
            }
        }

        public void Move(FieldCoord from, FieldCoord to)
        {
            var cell = GetCell(from);
            Remove(from);
            _grid[to.Row][to.Col] = cell;
        }

        public FieldRange TryStick(FieldCoord coord)
        {
            if (_scanner.IsSticking(coord))
            {
                var group = _scanner.Grouping(coord);
                foreach (var member in group)
                {
                    SetFalling(member, false);
                }
                return group.Coords.Count >= 4 ? group : new FieldRange();
            }
            return new FieldRange();
        }

        public void Remove(FieldCoord coord)
        {
            _grid[coord.Row][coord.Col] = Cell.Empty;
        }

        public Digging Dig(FieldCoord digCoord)
        {
            var blocks = new List<Block>();
            var range = _scanner.Grouping(digCoord);
            foreach (var coord in range)
            {
                if (IsBlock(coord)) // TODO:
                    blocks.Add(GetBlock(coord));
                Remove(coord);
            }

            var unbalances = _scanner.LookupUnbalances(range);
            return new Digging(blocks, unbalances.SelectMany(x => x.Coords));
        }

        public IEnumerator<FieldCoord> GetEnumerator()
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    yield return new FieldCoord(col, row);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        Cell GetCell(FieldCoord coord)
        {
            if (coord.Col < 0 || coord.Row < 0 || coord.Col >= Cols || coord.Row >= Rows)
            {
                return new Cell { Type = CellType.Wall };
            }
            return _grid[coord.Row][coord.Col];
        }
    }
}

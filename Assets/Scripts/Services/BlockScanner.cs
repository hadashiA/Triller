using System.Collections.Generic;
using System.Linq;
using Models;

namespace Services
{
    public class BlockScanner
    {
        readonly Field _field;

        public BlockScanner(Field field)
        {
            _field = field;
        }

        public FieldRange Grouping(FieldCoord entryCoord)
        {
            var range = new FieldRange();
            if (!_field.IsBlock(entryCoord)) return range;
            var stack = new Stack<FieldCoord>();

            var entryBlock = _field.GetBlock(entryCoord);
            var color = entryBlock.Color;

            stack.Push(entryCoord);
            while (stack.Any())
            {
                var coord = stack.Pop();
                range.Add(coord);

                for (var i = 0; i < 4; i++)
                {
                    var direction = (Direction) i;
                    var nextCoord = coord.Shift(direction);
                    if (_field.IsBlock(nextCoord))
                    {
                        var nextBlock = _field.GetBlock(nextCoord);
                        if (nextBlock.Color == color && !range.Contains(nextCoord))
                        {
                            stack.Push(nextCoord);
                        }
                    }
                }
            }
            return range;
        }

        public bool IsSticking(FieldCoord coord)
        {
            if (!_field.IsBlock(coord)) return false;
            var block = _field.GetBlock(coord);

            if (_field.IsBlock(coord.Down))
            {
                var downBlock = _field.GetBlock(coord.Down);
                if (!downBlock.Falling && downBlock.Color == block.Color)
                {
                    return true;
                }
            }

            if (_field.IsBlock(coord.Left))
            {
                var leftBlock = _field.GetBlock(coord.Left);
                if (!leftBlock.Falling && leftBlock.Color == block.Color)
                {
                    return true;
                }
            }

            if (_field.IsBlock(coord.Right))
            {
                var rightBlock = _field.GetBlock(coord.Right);
                if (!rightBlock.Falling && rightBlock.Color == block.Color)
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<FieldRange> LookupUnbalances(FieldRange digged) {
            var result  = new List<FieldRange>();
            var history = new List<FieldRange>();

            var upperGroups = CollectUpperGroups(digged);
            foreach (var upperGroup in upperGroups)
            {
                LookupUnbalancesRecursive(upperGroup, result, history);
            }
            return result;
        }

        void LookupUnbalancesRecursive(FieldRange group, List<FieldRange> result, List<FieldRange> history)
        {
            if (history.Contains(group)) return;
            history.Add(group);

            // 土台になっているグループを調べる
            foreach (var coord in group)
            {
                if (!_field.IsBlock(coord)) continue;
                if (!_field.IsBlock(coord.Down)) continue;
                if (group.Contains(coord.Down)) continue;

                var underGroup = Grouping(coord.Down);
                if (history.Contains(underGroup))
                {
                    if (!result.Contains(underGroup)) return;
                }
                else
                {
                    foreach (var u in underGroup)
                    {
                        var underUnderGroup = Grouping(u.Down);
                        if (!history.Contains(underUnderGroup)) return;
                    }
                    LookupUnbalancesRecursive(underGroup, result, history);
                }
            }

            if (result.Contains(group)) return;
            result.Add(group);

            // 自分に乗っているグループ調べる
            var upperGroups = CollectUpperGroups(group);
            foreach (var upperGroup in upperGroups)
            {
                LookupUnbalancesRecursive(upperGroup, result, history);
            }
        }

        public IEnumerable<FieldRange> CollectUpperGroups(FieldRange group)
        {
            var result = new List<FieldRange>();
            foreach (var coord in group)
            {
                if (_field.IsBlock(coord.Up))
                {
                    var upperGroup = Grouping(coord.Up);
                    if (group != upperGroup)
                    {
                        result.Add(upperGroup);
                    }
                }
            }
            return result;
        }
    }
}

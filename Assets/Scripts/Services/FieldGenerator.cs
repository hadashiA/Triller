using System.Diagnostics;
using Models;

namespace Services
{
    public static class FieldGenerator
    {
        public static Field Generate(DigSettings settings)
        {
            var rand = new System.Random();
            var field = new Field(settings.FieldSize)
            {
                FallSpeed = settings.FallSpeed,
            };

            foreach (var coord in field)
            {
                if (coord.Row < 5) continue;

                if (coord.Row > 10 && rand.Next(30) == 0)
                {
                    field.AddBlock(coord, new Block(BlockColor.Imo));
                }
                else
                {
                    var i = rand.Next(4);
                    switch (i)
                    {
                        case 1:
                            field.AddBlock(coord, new Block(BlockColor.Green));
                            break;
                        case 2:
                            field.AddBlock(coord, new Block(BlockColor.Blue));
                            break;
                        case 3:
                            field.AddBlock(coord, new Block(BlockColor.Yellow));
                            break;
                        default:
                            field.AddBlock(coord, new Block(BlockColor.Red));
                            break;
                    }
                }
            }
            return field;
        }
    }
}
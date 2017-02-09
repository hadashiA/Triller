namespace Models
{
    public enum BlockColor
    {
        Red, Green, Blue, Yellow, Hard, Imo
    }

    public struct Block
    {
        static int _lastId;

        public int Id { get; private set; }
        public BlockColor Color { get; private set; }
        public int Hp { get; set; }
        public bool Falling { get; set; }

        public Block(BlockColor color) : this()
        {
            Id = ++_lastId;
            Color = color;
            Hp = color == BlockColor.Hard ? 5 : 1;
        }

        public override bool Equals(object obj)
        {
            if (obj is Block)
            {
                return ((Block)obj).Id == Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}

using System;

namespace Models
{
    [Serializable]
    public struct Player
    {
        public float Hp;
        public FieldCoord Coord;
        public float WalkSpeed { get; private set; }
        public float ClimeTime { get; private set; }
        public float DigInterval { get; private set; }

        public Player(DigSettings settings)
        {
            Hp = 100f;
            Coord = new FieldCoord((int) (settings.FieldSize.Col * 0.5f), 0);
            WalkSpeed = settings.WalkSpeed;
            ClimeTime = settings.ClimeTime;
            DigInterval = settings.DigInterval;
        }

        public void Damage(float p)
        {
            Dig.Player.Hp -= p;
            if (Dig.Player.Hp <= 0)
            {
                Dig.Player.Hp = 0f;
            }
        }
    }
}

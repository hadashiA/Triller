using System;

namespace Models
{
    [Serializable]
    public class DigSettings
    {
        public FieldCoord FieldSize = new FieldCoord(15, 100);
        public float FallSpeed = 5f;
        public float WalkSpeed = 3f;
        public float DigInterval = 0.5f;
        public float ClimeTime = 0.5f;
        public float ShakeDuration = 0.5f;
        public float BlinkDuration = 0.75f;
        public float DamageSpeed = 3.33f;
    }
}

using System;

namespace EIODE.Utils
{
    public static class Masks
    {
        [Flags]
        public enum Collision
        {
            World,
            Player,
            Hittable,
            Enemy,
            Hitbox
        }
    }
}

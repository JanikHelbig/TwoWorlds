using Unity.Mathematics;

namespace Collision
{
    public readonly struct AABB
    {
        public readonly float2 min;
        public readonly float2 max;

        public AABB(float2 min, float2 max)
        {
            this.min = min;
            this.max = max;
        }

        public float2 Center => min + max * 0.5f;
        public float2 Extents => (max - min) * 0.5f;
    }
}
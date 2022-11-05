using Unity.Mathematics;

namespace Utility
{
    public class DirectionUtility
    {
        public static Direction From(float2 v)
        {
            float2 abs = math.abs(v);

            return abs.x > abs.y
                ? v.x > 0
                    ? Direction.EAST
                    : Direction.WEST
                : v.y > 0
                    ? Direction.NORTH
                    : Direction.SOUTH;
        }
    }
}
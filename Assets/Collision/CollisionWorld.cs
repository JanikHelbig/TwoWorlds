using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;

namespace Collision
{
    public class CollisionWorld : MonoBehaviour
    {
        [SerializeField]
        private LevelManager levelManager;

        private IEnumerable<AABB> GetWorldColliders(World world, float2 position)
        {
            var gridPosition = (int2) round(position);

            for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
            for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
            {
                Tile tile = levelManager.level.tiles[x, y];
                if (tile.world == world)
                    yield break;

                var center = new float2(x, y);
                var extents = new float2(0.5f);

                float2 min = center - extents;
                float2 max = center + extents;
                yield return new AABB(min, max);
            }
        }

        public float2 MovePlayer(World world, AABB player, float2 position)
        {
            foreach (AABB worldCollider in GetWorldColliders(world, position))
            {
                Debug.Log($"Relevant world collider found: ({worldCollider.Center}, {worldCollider.Extents})");
            }

            return new float2();
        }

        private static Sweep SweepAABB(AABB movingAABB, AABB staticAABB, float2 delta)
        {
            return new Sweep();
        }

        private static float2 IntersectSegment(AABB boundingBox, float2 point, float2 delta)
        {
            return float2.zero;
        }
    }
}
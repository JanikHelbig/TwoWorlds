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
                Tile tile = levelManager.level[x, y];
                if (tile.world == world || !tile.IsWalkable())
                    yield break;

                var center = new float2(x, y);
                var extents = new float2(0.5f);

                float2 min = center - extents;
                float2 max = center + extents;
                yield return new AABB(min, max);
            }
        }

        public Sweep MovePlayer(World world, AABB player, float2 position)
        {
            Sweep nearest;
            nearest.hit = null;
            nearest.time = 1;
            nearest.position = position;

            foreach (AABB worldCollider in GetWorldColliders(world, position))
            {
                Sweep sweep = SweepAABB(player, worldCollider, position - player.Center);
                if (sweep.time < nearest.time)
                {
                    nearest = sweep;
                }
            }

            return nearest;
        }

        private static Hit? IntersectAABB(AABB a, AABB b)
        {
            float2 d = a.Center - b.Center;
            float2 p = (a.Extents + b.Extents) - abs(d);

            if (any(p <= float2.zero))
                return null;

            Hit hit;

            if (p.x < p.y)
            {
                float sx = sign(d.x);
                hit.delta = new float2(p.x * sx, 0);
                hit.normal = new float2(sx, 0);
                hit.position = new float2(a.Center.x + a.Extents.x * sx, b.Center.y);
            }
            else
            {
                float sy = sign(d.y);
                hit.delta = new float2(0, p.x * sy);
                hit.normal = new float2(0, sy);
                hit.position = new float2(b.Center.x, a.Center.y + a.Extents.y * sy);
            }

            hit.time = 0;
            return hit;
        }

        private static Sweep SweepAABB(AABB movingAABB, AABB staticAABB, float2 delta)
        {
            var sweep = new Sweep();

            if (all(delta == float2.zero))
            {
                sweep.position = staticAABB.Center;
                Hit? hit = IntersectAABB(staticAABB, movingAABB);

                if (hit.HasValue)
                {
                    Hit h = hit.Value;
                    h.time = 0;
                    hit = h;
                    sweep.time = (sweep.hit = hit).Value.time;
                }
                else
                {
                    sweep.time = 1;
                }

                return sweep;
            }

            sweep.hit = BoxSegmentIntersection(staticAABB, movingAABB.Center, delta, movingAABB.Extents);
            if (sweep.hit.HasValue)
            {
                sweep.time = clamp(sweep.hit.Value.time - EPSILON, 0, 1);
                sweep.position = movingAABB.Center + delta * sweep.time;
                float2 dir = normalize(delta);

                Hit hit = sweep.hit.Value;
                hit.position = clamp(hit.position + dir * movingAABB.Extents, staticAABB.min, staticAABB.max);
                sweep.hit = hit;
            }
            else
            {
                sweep.position = staticAABB.Center + delta;
                sweep.time = 1;
            }

            return sweep;
        }

        private static Hit? BoxSegmentIntersection(AABB boundingBox, float2 position, float2 delta, float2 padding = new())
        {
            float2 scale = 1.0f / delta;
            float2 sig = sign(scale);

            float2 nearTime2 = (boundingBox.Center - sig * (boundingBox.Extents + padding) - position) * scale;
            float2 farTime2 = (boundingBox.Center + sig * (boundingBox.Extents + padding) - position) * scale;

            if (any(nearTime2 > farTime2))
                return null;

            float nearTime = cmax(nearTime2);
            float farTime = cmin(farTime2);

            if (nearTime >= 1 || farTime <= 0)
                return null;

            Hit hit;
            hit.time = clamp(nearTime, 0, 1);

            hit.normal = nearTime2.x > nearTime2.y
                ? new float2(-sig.x, 0)
                : new float2(0, -sig.y);

            hit.delta = (1.0f - hit.time) * -delta;
            hit.position = position + delta * hit.time;

            return hit;
        }
    }
}
using UnityEngine;
using Unity.Mathematics;
using Collision;
using CustomInput;
using Utility;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private CollisionWorld collisionWorld;

        [Header("Settings")]
        [SerializeField] private World world;
        [SerializeField] private float colliderSize;

        private Transform _tf;
        private AABB _collider;

        private void Awake()
        {
            _tf = transform;
        }

        private void Update()
        {
            float2 currentPosition = ((float3)_tf.position).xz;

            var extents = new float2(colliderSize);
            float2 min = currentPosition - extents;
            float2 max = currentPosition + extents;
            _collider = new AABB(min, max);

            float2 moveInput = inputManager.GetPlayerMoveInput(world);
            float2 delta = moveInput * Time.deltaTime;

            Sweep sweep = collisionWorld.MovePlayer(world, _collider, delta);

            currentPosition = sweep.position;
            _tf.position = _tf.position.With(x: currentPosition.x, z: currentPosition.y);
        }
    }
}
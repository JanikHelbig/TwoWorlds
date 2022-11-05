using System;
using Collision;
using CustomInput;
using Unity.Mathematics;
using UnityEngine;

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

        private AABB _collider;
        private float2 _currentPosition;

        private void Update()
        {
            var extents = new float2(colliderSize);

            float2 min = _currentPosition - extents;
            float2 max = _currentPosition + extents;
            _collider = new AABB(min, max);

            float2 moveInput = inputManager.GetPlayerMoveInput(world);
            Sweep sweep = collisionWorld.MovePlayer(world, _collider, moveInput);


        }
    }
}
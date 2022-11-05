using System;
using UnityEngine;
using Unity.Mathematics;
using CustomInput;
using Utility;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private LevelManager levelManager;

        [Header("Settings")]
        [SerializeField] private World world;
        [SerializeField] private float moveSpeed = 2;
        [SerializeField] private float pushTriggerThreshold;

        private Transform _tf;
        private Rigidbody _rigid;

        private Collider _pushingCollider;
        private float _pushDuration;

        private float2 Position => new(_tf.position.x, _tf.position.z);

        private void Awake()
        {
            _tf = transform;
            _rigid = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            float2 currentPosition = ((float3) _rigid.position).xz;

            float2 moveInput = inputManager.GetPlayerMoveInput(world);
            float2 delta = moveInput * moveSpeed * Time.deltaTime;

            Vector3 targetPos = _tf.position.With(
                x: currentPosition.x + delta.x,
                z: currentPosition.y + delta.y);

            _rigid.MovePosition(targetPos);
        }

        private void OnCollisionStay(Collision collisionInfo)
        {
            if (_pushingCollider == collisionInfo.collider)
            {
                _pushDuration += Time.deltaTime;
                if (_pushDuration >= pushTriggerThreshold)
                {
                    float3 pos = _pushingCollider.transform.position;
                    int2 gridPos = (int2) math.round(pos.xz);
                    float2 dir = pos.xz - Position;
                    levelManager.level.TryMoveTile(gridPos, DirectionUtility.From(dir));
                    _pushDuration = 0;
                }
            }
            else
            {
                _pushingCollider = collisionInfo.collider;
            }
        }
    }
}
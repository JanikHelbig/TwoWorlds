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
        [SerializeField] private float moveSpeed = 2;

        private Transform _tf;
        private Rigidbody _rigid;
        private AABB _collider;

        private void Awake()
        {
            _tf = transform;
            _rigid = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            float2 currentPosition = ((float3)_tf.position).xz;

            float2 moveInput = inputManager.GetPlayerMoveInput(world);
            float2 delta = moveInput * moveSpeed * Time.deltaTime;

            Vector3 targetPos = _tf.position.With(
                x: currentPosition.x + delta.x,
                z: currentPosition.y + delta.y);

            _rigid.MovePosition(targetPos);
        }
    }
}
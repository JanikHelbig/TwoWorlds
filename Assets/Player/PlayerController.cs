using UnityEngine;
using Unity.Mathematics;
using CustomInput;
using Utility;
using DG.Tweening;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private LevelManager levelManager;

        [Header("Settings")]
        [SerializeField] private World world;

        private Transform _tf;

        private float _moveDelay = 0;
        
        private void Awake()
        {
            _tf = transform;
        }

        private void Update()
        {
            if (levelManager.BlockInput)
                return;

            _moveDelay -= Time.deltaTime;
            if (_moveDelay > 0)
                return;

            float2 moveInput = inputManager.GetPlayerMoveInput(world);

            if (moveInput.x == 0 && moveInput.y == 0)
                return;

            float3 pos = transform.position;
            int2 currentPos = (int2) math.round(pos.xz);
            var d = DirectionUtility.From(moveInput);
            int2 nextPos = currentPos.OffsetPosition(d);

            if (levelManager.IsOccuipiedByOtherPlayer(gameObject, nextPos))
                return;

            Tile t = levelManager.level[nextPos.x, nextPos.y];

            if (t.IsWalkable() && t.world == world)
            {
                // TODO: move animation
                Vector3 target = _tf.localPosition.With(x: nextPos.x, z: nextPos.y);
                transform.DOLocalMove(target, 0.5f).Play();
                _moveDelay = 0.5f;
            }

            if (t.IsPushable() && t.world != this.world && levelManager.level.CanMoveTile(nextPos, d))
            {
                // TODO: push move animation
                levelManager.level.TryMoveTile(nextPos, d);
                Vector3 target = _tf.localPosition.With(x: nextPos.x, z: nextPos.y);
                transform.DOLocalMove(target, 0.5f).Play();
                _moveDelay = 0.5f;
            }
        }
    }
}
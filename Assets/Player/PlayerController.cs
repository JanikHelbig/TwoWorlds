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

        private Animator _animator;

        private Transform _tf;

        private float _moveDelay = 0;
        
        private void Awake()
        {
            _tf = transform;
            _animator = GetComponentInChildren<Animator>();
            levelManager.OnLevelCompleted += () =>
            {
                this.transform.rotation = Quaternion.Euler(0, 180, 0);
                _animator.SetTrigger("Victory");
            };
        }

        private void Update()
        {
            if (levelManager.blockInput)
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

            if (levelManager.IsOccupiedByOtherPlayer(gameObject, nextPos))
                return;

            switch(d)
            {
                case Direction.NORTH:
                    this.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Direction.EAST:
                    this.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case Direction.WEST:
                    this.transform.rotation = Quaternion.Euler(0, 270, 0);
                    break;
                case Direction.SOUTH:
                    this.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }

            Tile t = levelManager.level[nextPos.x, nextPos.y];

            if (t.IsWalkable() && t.world == world)
            {
                _animator.SetBool("Move", true);
                Vector3 target = _tf.localPosition.With(x: nextPos.x, z: nextPos.y);
                transform.DOLocalMove(target, 0.5f).Play().OnComplete(()=>_animator.SetBool("Move",false));
                _moveDelay = 0.5f;
            }

            if (t.IsPushable() && t.world != this.world && levelManager.level.CanMoveTile(nextPos, d))
            {
                levelManager.level.TryMoveTile(nextPos, d);
                Vector3 target = _tf.localPosition.With(x: nextPos.x, z: nextPos.y);
                _animator.SetBool("Push", true);
                this.transform.DOLocalMove(target, 0.5f).Play().OnComplete(() => _animator.SetBool("Push", false));
                _moveDelay = 0.5f;
            }
        }
    }
}
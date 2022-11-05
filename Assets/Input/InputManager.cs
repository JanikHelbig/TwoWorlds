using System;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace CustomInput
{
    public class InputManager : MonoBehaviour
    {
        private InputActions _inputActions;

        private ControlMode _controlMode = ControlMode.TwoPlayers;
        private World _currentSingleWorld = World.Light;

        private void Awake()
        {
            _inputActions = new InputActions();
            _inputActions.Enable();
        }

        public void SelectControlMode(ControlMode controlMode)
        {
            _controlMode = controlMode;
        }

        public float2 GetPlayerMoveInput(World world)
        {
            float2 moveInput = GetRelevantMoveInput(world);
            return moveInput;
        }

        private float2 GetRelevantMoveInput(World playerWorld)
        {
            InputAction action = (_controlMode, _currentSingleWorld, playerWorld) switch
            {
                (ControlMode.OnePlayer,  World.Light, World.Light) => _inputActions.Game.MovePrimary,
                (ControlMode.OnePlayer,  World.Dark,  World.Dark ) => _inputActions.Game.MovePrimary,
                (ControlMode.TwoPlayers, _,           World.Light) => _inputActions.Game.MovePrimary,
                (ControlMode.TwoPlayers, _,           World.Dark ) => _inputActions.Game.MoveSecondary,
                _                                                  => null
            };

            return action?.ReadValue<Vector2>() ?? float2.zero;
        }
    }
}
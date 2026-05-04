using Player;
using UnityEngine;

namespace Input
{
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;

        public bool InteractionPressedThisFrame { get; private set; }
        public bool ClosePressedThisFrame { get; private set; }
        public bool VirtualLeftPressed => _virtualLeftPressed;
        public bool VirtualRightPressed => _virtualRightPressed;
        public bool VirtualJumpPressed => _virtualJumpPressed;
        public bool VirtualInteractPressed => _virtualInteractPressed;
        public bool LeftPressed => ResolvePressedState(IsKeyboardLeftPressed(), _virtualLeftPressed);
        public bool RightPressed => ResolvePressedState(IsKeyboardRightPressed(), _virtualRightPressed);
        public bool JumpPressed => ResolvePressedState(UnityEngine.Input.GetKey(KeyCode.Space), _virtualJumpPressed);
        public bool InteractPressed => ResolvePressedState(UnityEngine.Input.GetKey(KeyCode.E), _virtualInteractPressed);

        private bool _virtualLeftPressed;
        private bool _virtualRightPressed;
        private bool _virtualJumpPressed;
        private bool _virtualInteractPressed;
        private bool _virtualJumpRequested;
        private bool _virtualInteractRequested;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
        }

        private void Update()
        {
            var move = ResolveMoveInput(
                IsKeyboardLeftPressed(),
                IsKeyboardRightPressed(),
                _virtualLeftPressed,
                _virtualRightPressed);
            playerController?.SetMoveInput(move);

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) || _virtualJumpRequested)
            {
                playerController?.RequestJump();
            }

            InteractionPressedThisFrame = UnityEngine.Input.GetKeyDown(KeyCode.E) || _virtualInteractRequested;
            ClosePressedThisFrame = UnityEngine.Input.GetKeyDown(KeyCode.E) ||
                UnityEngine.Input.GetKeyDown(KeyCode.Escape) ||
                _virtualInteractRequested;
            _virtualJumpRequested = false;
            _virtualInteractRequested = false;
        }

        public void SetVirtualLeftPressed(bool pressed)
        {
            _virtualLeftPressed = pressed;
        }

        public void SetVirtualRightPressed(bool pressed)
        {
            _virtualRightPressed = pressed;
        }

        public void SetVirtualJumpPressed(bool pressed)
        {
            if (pressed && !_virtualJumpPressed)
            {
                _virtualJumpRequested = true;
            }

            _virtualJumpPressed = pressed;
        }

        public void SetVirtualInteractPressed(bool pressed)
        {
            if (pressed && !_virtualInteractPressed)
            {
                _virtualInteractRequested = true;
            }

            _virtualInteractPressed = pressed;
        }

        public static float ResolveMoveInput(
            bool keyboardLeftPressed,
            bool keyboardRightPressed,
            bool virtualLeftPressed,
            bool virtualRightPressed)
        {
            var move = 0f;
            if (ResolvePressedState(keyboardLeftPressed, virtualLeftPressed))
            {
                move -= 1f;
            }

            if (ResolvePressedState(keyboardRightPressed, virtualRightPressed))
            {
                move += 1f;
            }

            return move;
        }

        public static bool ResolvePressedState(bool keyboardPressed, bool virtualPressed)
        {
            return keyboardPressed || virtualPressed;
        }

        private static bool IsKeyboardLeftPressed()
        {
            return UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow);
        }

        private static bool IsKeyboardRightPressed()
        {
            return UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow);
        }
    }
}

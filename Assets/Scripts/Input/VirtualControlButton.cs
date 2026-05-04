using UnityEngine;
using UnityEngine.EventSystems;

namespace Input
{
    public enum VirtualControlAction
    {
        Left,
        Right,
        Jump,
        Interact
    }

    public class VirtualControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private VirtualControlAction action;

        private bool _pressed;
        private bool _oneShotRequestedDuringPress;

        public void Configure(PlayerInputReader reader, VirtualControlAction controlAction)
        {
            inputReader = reader;
            action = controlAction;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            _oneShotRequestedDuringPress = false;
            Apply(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_oneShotRequestedDuringPress)
            {
                _oneShotRequestedDuringPress = false;
                return;
            }

            if (action == VirtualControlAction.Jump)
            {
                inputReader?.SetVirtualJumpPressed(true);
                inputReader?.SetVirtualJumpPressed(false);
            }
            else if (action == VirtualControlAction.Interact)
            {
                inputReader?.SetVirtualInteractPressed(true);
                inputReader?.SetVirtualInteractPressed(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Release();
        }

        private void OnDisable()
        {
            Release();
        }

        private void Release()
        {
            if (!_pressed)
            {
                return;
            }

            _pressed = false;
            Apply(false);
        }

        private void Apply(bool pressed)
        {
            if (inputReader == null)
            {
                return;
            }

            switch (action)
            {
                case VirtualControlAction.Left:
                    inputReader.SetVirtualLeftPressed(pressed);
                    break;
                case VirtualControlAction.Right:
                    inputReader.SetVirtualRightPressed(pressed);
                    break;
                case VirtualControlAction.Jump:
                    inputReader.SetVirtualJumpPressed(pressed);
                    _oneShotRequestedDuringPress = pressed || _oneShotRequestedDuringPress;
                    break;
                case VirtualControlAction.Interact:
                    inputReader.SetVirtualInteractPressed(pressed);
                    _oneShotRequestedDuringPress = pressed || _oneShotRequestedDuringPress;
                    break;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using VContainer;

namespace Game.Core.Input.Joysticks
{
    public class FloatingJoystick : Joystick
    {
        [Inject]
        private InputActionAsset m_inputActionAsset;

        private InputAction m_moveAction;
        private Gamepad m_virtualGamepad;

        protected override void Start()
        {
            base.Start();
            background.gameObject.SetActive(false);

            m_moveAction = m_inputActionAsset.FindAction("Player/Move");
            m_moveAction?.Enable();

            m_virtualGamepad = (Gamepad)InputSystem.AddDevice("Gamepad");
        }

        private void OnDestroy()
        {
            m_moveAction?.Disable();

            if (m_virtualGamepad != null)
                InputSystem.RemoveDevice(m_virtualGamepad);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            background.gameObject.SetActive(true);
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            background.gameObject.SetActive(false);
            base.OnPointerUp(eventData);
            UpdateInputAction();
        }

        protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, UnityEngine.Camera cam)
        {
            base.HandleInput(magnitude, normalised, radius, cam);
            UpdateInputAction();
        }

        private void UpdateInputAction()
        {
            if (m_virtualGamepad == null)
                return;

            var inputValue = new Vector2(Horizontal, Vertical);
            InputSystem.QueueStateEvent(m_virtualGamepad, new GamepadState { leftStick = inputValue });
        }
    }
}
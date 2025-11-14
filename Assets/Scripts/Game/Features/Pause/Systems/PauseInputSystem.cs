using Game.Core.UI;
using UnityEngine.InputSystem;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Pause.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class PauseInputSystem : SystemBase
    {
        public override SystemUpdateMask UpdateMask => SystemUpdateMask.Update;

        [Inject]
        private IWindowManager m_windowManager;

        [Inject]
        private InputActionAsset m_inputActionAsset;

        private InputAction m_cancelAction;
        private UI.PauseMenu m_pauseMenu;
        private bool m_wasPressedLastFrame;

        public override void OnAwake()
        {
            m_cancelAction = m_inputActionAsset.FindAction("UI/Cancel");
            m_cancelAction?.Enable();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            m_cancelAction?.Disable();
        }

        public override void OnUpdate()
        {
            if (m_cancelAction == null)
                return;

            var isPressed = m_cancelAction.ReadValue<float>() > 0.5f;

            if (isPressed && !m_wasPressedLastFrame)
            {
                HandleCancelInput();
            }

            m_wasPressedLastFrame = isPressed;
        }

        private void HandleCancelInput()
        {
            if (m_pauseMenu == null)
            {
                m_pauseMenu = m_windowManager.GetWindowInstance<UI.PauseMenu>();
            }

            if (m_pauseMenu != null && m_pauseMenu.IsOpen)
            {
                m_pauseMenu.Close();
                return;
            }

            if (m_windowManager.OpenWindows.Count > 0)
            {
                m_windowManager.CloseTopWindow();
                return;
            }

            if (m_pauseMenu != null)
            {
                m_pauseMenu.Open();
            }
        }
    }
}
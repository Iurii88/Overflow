using Game.Core.Logging;
using Game.Core.UI;
using Game.Features.Pause;
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
        private IPauseManager m_pauseManager;

        [Inject]
        private InputActionAsset m_inputActionAsset;

        private InputAction m_cancelAction;
        private UI.PauseMenu m_pauseMenu;
        private bool m_wasPressedLastFrame;
        private bool m_eventsSubscribed;

        public override void OnAwake()
        {
            m_cancelAction = m_inputActionAsset.FindAction("UI/Cancel");
            m_cancelAction?.Enable();

            TrySubscribeToPauseMenu();
        }

        private void TrySubscribeToPauseMenu()
        {
            if (m_eventsSubscribed)
                return;

            if (m_pauseMenu == null)
            {
                m_pauseMenu = m_windowManager.GetWindowInstance<UI.PauseMenu>();
            }

            if (m_pauseMenu != null)
            {
                m_pauseMenu.OnWindowOpened += HandlePauseMenuOpened;
                m_pauseMenu.OnWindowClosed += HandlePauseMenuClosed;
                m_eventsSubscribed = true;
                GameLogger.Log("[PauseInputSystem] Successfully subscribed to pause menu events");
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            m_cancelAction?.Disable();

            if (m_pauseMenu != null && m_eventsSubscribed)
            {
                m_pauseMenu.OnWindowOpened -= HandlePauseMenuOpened;
                m_pauseMenu.OnWindowClosed -= HandlePauseMenuClosed;
                m_eventsSubscribed = false;
            }
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
            TrySubscribeToPauseMenu();

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

        private void HandlePauseMenuOpened(AWindowViewComponent _)
        {
            GameLogger.Log("[PauseInputSystem] Pause menu opened - pausing game");
            m_pauseManager.Pause();
        }

        private void HandlePauseMenuClosed(AWindowViewComponent _)
        {
            GameLogger.Log("[PauseInputSystem] Pause menu closed - resuming game");
            m_pauseManager.Resume();
        }
    }
}
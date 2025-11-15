using Game.Core.Logging;
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
        [Inject]
        private IWindowManager m_windowManager;

        [Inject]
        private IPauseManager m_pauseManager;

        [Inject]
        private InputActionAsset m_inputActionAsset;

        private InputAction m_cancelAction;
        private UI.PauseMenu m_pauseMenu;

        public override void OnAwake()
        {
            m_cancelAction = m_inputActionAsset.FindAction("UI/Cancel");
            m_cancelAction.performed += OnCancelPerformed;
            m_cancelAction.Enable();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            m_cancelAction.performed -= OnCancelPerformed;
            m_cancelAction.Disable();

            CancelSubscription();
        }

        private void EnsureSubscription()
        {
            if (m_pauseMenu != null)
                return;

            m_pauseMenu = m_windowManager.GetWindowInstance<UI.PauseMenu>();
            if (m_pauseMenu == null)
                return;

            m_pauseMenu.OnWindowOpened += HandlePauseMenuOpened;
            m_pauseMenu.OnWindowClosed += HandlePauseMenuClosed;
            GameLogger.Log("[PauseInputSystem] Successfully subscribed to pause menu events");
        }

        private void CancelSubscription()
        {
            if (m_pauseMenu == null)
                return;

            m_pauseMenu.OnWindowOpened -= HandlePauseMenuOpened;
            m_pauseMenu.OnWindowClosed -= HandlePauseMenuClosed;
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            EnsureSubscription();
            HandleCancelInput();
        }

        private void HandleCancelInput()
        {
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
                m_pauseMenu.Open();
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
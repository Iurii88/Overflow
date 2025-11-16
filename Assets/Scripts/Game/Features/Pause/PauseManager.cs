using System;
using Game.Core.Extensions;
using Game.Core.Logging;
using Game.Features.Pause.Extensions;
using Game.Features.Sessions.Attributes;
using VContainer;

namespace Game.Features.Pause
{
    [AutoRegister]
    public class PauseManager : IPauseManager
    {
        [Inject]
        private IExtensionExecutor m_extensionExecutor;

        public bool IsPaused { get; private set; }

        public event Action OnPaused;
        public event Action OnResumed;

        public void Pause()
        {
            if (IsPaused)
                return;

            IsPaused = true;
            GameLogger.Log("Game paused");
            OnPaused?.Invoke();
            m_extensionExecutor?.Execute<IGamePausedExtension>(ext => ext.OnGamePaused());
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            GameLogger.Log("Game resumed");
            OnResumed?.Invoke();
            m_extensionExecutor?.Execute<IGameResumedExtension>(ext => ext.OnGameResumed());
        }

        public void TogglePause()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
}
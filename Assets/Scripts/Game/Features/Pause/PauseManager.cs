using System;
using Game.Core.Logging;
using Game.Core.Reflection.Attributes;

namespace Game.Features.Pause
{
    [AutoRegister]
    public class PauseManager : IPauseManager
    {
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
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            GameLogger.Log("Game resumed");
            OnResumed?.Invoke();
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
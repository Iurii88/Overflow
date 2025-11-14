using System;

namespace Game.Features.Pause
{
    public interface IPauseManager
    {
        bool IsPaused { get; }

        event Action OnPaused;
        event Action OnResumed;

        void Pause();
        void Resume();
        void TogglePause();
    }
}

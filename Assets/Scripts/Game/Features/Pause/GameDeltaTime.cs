using Game.Core.Reflection.Attributes;
using UnityEngine;
using VContainer;

namespace Game.Features.Pause
{
    [AutoRegister]
    public class GameDeltaTime : IGameDeltaTime
    {
        [Inject]
        private readonly IPauseManager m_pauseManager;

        public float DeltaTime => m_pauseManager.IsPaused ? 0f : Time.deltaTime;
        public float FixedDeltaTime => m_pauseManager.IsPaused ? 0f : Time.fixedDeltaTime;
    }
}
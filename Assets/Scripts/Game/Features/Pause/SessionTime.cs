using Game.Core.Logging;
using Game.Core.Reflection.Attributes;
using UnityEngine;
using VContainer;

namespace Game.Features.Pause
{
    [AutoRegister]
    public class SessionTime : ISessionTime
    {
        private readonly IPauseManager m_pauseManager;

        private float m_timeScale = 1f;

        [Inject]
        public SessionTime(IPauseManager pauseManager)
        {
            m_pauseManager = pauseManager;
        }

        public float DeltaTime => m_pauseManager.IsPaused ? 0f : Time.deltaTime * m_timeScale;
        public float FixedDeltaTime => m_pauseManager.IsPaused ? 0f : Time.fixedDeltaTime * m_timeScale;
        public float UnscaledDeltaTime => Time.unscaledDeltaTime;
        public float UnscaledFixedDeltaTime => Time.fixedUnscaledDeltaTime;

        public float ElapsedTime { get; private set; }

        public float UnscaledElapsedTime { get; private set; }

        public float TimeScale
        {
            get => m_timeScale;
            set => m_timeScale = Mathf.Max(0f, value);
        }

        public int FrameCount { get; private set; }

        public void Update()
        {
            ElapsedTime += DeltaTime;
            UnscaledElapsedTime += UnscaledDeltaTime;
            FrameCount++;

            GameLogger.Log($"ElapsedTime: {ElapsedTime}, FrameCount: {FrameCount}");
        }
    }
}
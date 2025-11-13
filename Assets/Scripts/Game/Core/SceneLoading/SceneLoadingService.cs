using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Logging;
using UnityEngine.SceneManagement;

namespace Game.Core.SceneLoading
{
    public class SceneLoadingService : IGameSceneConfigurationProvider
    {
        private const string GameSceneName = "Game";

        private GameSceneConfiguration m_currentConfiguration;

        public GameSceneConfiguration GetConfiguration()
        {
            return m_currentConfiguration;
        }

        public async UniTask LoadGameSceneAsync(GameSceneConfiguration configuration, CancellationToken cancellationToken = default)
        {
            m_currentConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            GameLogger.Log($"[SceneLoadingService] Loading game scene with map: {configuration.mapId}");

            var asyncOperation = SceneManager.LoadSceneAsync(GameSceneName, LoadSceneMode.Single);

            if (asyncOperation == null)
            {
                GameLogger.Error($"[SceneLoadingService] Failed to load scene: {GameSceneName}");
                return;
            }

            await asyncOperation.ToUniTask(cancellationToken: cancellationToken);

            GameLogger.Log("[SceneLoadingService] Game scene loaded successfully");
        }

        public async UniTask UnloadGameSceneAsync(CancellationToken cancellationToken = default)
        {
            GameLogger.Log("[SceneLoadingService] Unloading game scene");

            var scene = SceneManager.GetSceneByName(GameSceneName);

            if (!scene.isLoaded)
            {
                GameLogger.Warning($"[SceneLoadingService] Scene {GameSceneName} is not loaded");
                return;
            }

            var asyncOperation = SceneManager.UnloadSceneAsync(scene);

            if (asyncOperation == null)
            {
                GameLogger.Error($"[SceneLoadingService] Failed to unload scene: {GameSceneName}");
                return;
            }

            await asyncOperation.ToUniTask(cancellationToken: cancellationToken);

            GameLogger.Log("[SceneLoadingService] Game scene unloaded successfully");

            m_currentConfiguration = null;
        }
    }
}
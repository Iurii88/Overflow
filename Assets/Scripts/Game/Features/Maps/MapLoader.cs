using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Initialization;
using Game.Core.Logging;
using Game.Core.Reflection.Attributes;
using Game.Core.VContainer;
using Game.Features.Maps.Content;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Game.Features.Maps
{
    [AutoRegister]
    public class MapLoader : IAsyncLoader, IUniTaskAsyncDisposable
    {
        [Inject]
        private IContentManager m_contentManager;

        [Inject]
        private GameLifeTimeScope m_gameScope;

        public string mapId;

        private string m_loadedSceneName;

        public async UniTask LoadAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                GameLogger.Warning("[MapLoader] No map ID specified, skipping scene load");
                return;
            }

            var map = m_contentManager.Get<ContentMap>(mapId);
            if (map == null)
            {
                GameLogger.Error($"[MapLoader] Map not found: {mapId}");
                return;
            }

            if (string.IsNullOrEmpty(map.scene))
            {
                GameLogger.Warning($"[MapLoader] Map '{mapId}' has no scene specified");
                return;
            }

            GameLogger.Log($"[MapLoader] Loading scene: {map.scene}");

            using (LifetimeScope.EnqueueParent(m_gameScope))
            {
                var asyncOperation = SceneManager.LoadSceneAsync(map.scene, LoadSceneMode.Additive);
                if (asyncOperation == null)
                {
                    GameLogger.Error($"[MapLoader] Failed to start loading scene: {map.scene}");
                    return;
                }

                await asyncOperation.ToUniTask(cancellationToken: cancellationToken);
                m_loadedSceneName = map.scene;
                GameLogger.Log($"[MapLoader] Scene loaded successfully: {map.scene}");
            }
        }

        public async UniTask UnloadAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(m_loadedSceneName))
            {
                GameLogger.Warning("[MapLoader] No scene loaded, skipping unload");
                return;
            }

            GameLogger.Log($"[MapLoader] Unloading scene: {m_loadedSceneName}");

            var scene = SceneManager.GetSceneByName(m_loadedSceneName);
            if (!scene.isLoaded)
            {
                GameLogger.Warning($"[MapLoader] Scene {m_loadedSceneName} is not loaded");
                m_loadedSceneName = null;
                return;
            }

            var asyncOperation = SceneManager.UnloadSceneAsync(scene);
            if (asyncOperation == null)
            {
                GameLogger.Error($"[MapLoader] Failed to unload scene: {m_loadedSceneName}");
                return;
            }

            await asyncOperation.ToUniTask(cancellationToken: cancellationToken);
            GameLogger.Log($"[MapLoader] Scene unloaded successfully: {m_loadedSceneName}");
            m_loadedSceneName = null;
        }

        public async UniTask DisposeAsync()
        {
            await UnloadAsync();
        }
    }
}
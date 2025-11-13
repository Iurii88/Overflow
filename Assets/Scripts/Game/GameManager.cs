using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Content.Attributes;
using Game.Core.Extensions;
using Game.Core.Initialization;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Core.SceneLoading;
using Game.Core.Settings;
using Game.Features.Bootstraps;
using Game.Features.LoadingScreen.Extensions;
using Game.Features.Maps;
using Game.Features.Maps.Content;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game
{
    public interface IGameManager
    {
        UniTask RestartAsync(CancellationToken cancellationToken = default);
    }

    public class GameManager : MonoBehaviour, IGameManager, IAsyncStartable
    {
        public LifetimeScope gameScope;

        [Inject]
        private IObjectResolver m_objectResolver;

        [Inject]
        private IReflectionManager m_reflectionManager;

        [Inject]
        private IContentManager m_contentManager;

        [Inject]
        private EcsBootstrap m_ecsBootstrap;

        [Inject]
        private IExtensionExecutor m_extensionExecutor;

        [Inject]
        private IGameSceneConfigurationProvider m_configurationProvider;

        [ContentSelector(typeof(ContentMap))]
        public string selectedMapId;

        private MapLoader m_mapLoader;

        public string CurrentMapId { get; private set; }

        private async UniTask Load(CancellationToken cancellation)
        {
            await m_extensionExecutor.ExecuteAsync<IGameStartLoadingExtension>(extension => extension.OnGameStartLoading());

            RuntimeSettingsLoader.LoadAllSettings(m_reflectionManager);

            CurrentMapId = GetMapId();
            m_mapLoader = new MapLoader(gameScope, m_contentManager, CurrentMapId);

            await new LoaderConfiguration()
                .Register(m_contentManager)
                .Register(m_mapLoader).After(m_contentManager)
                .Register(m_ecsBootstrap).After(m_mapLoader)
                .LoadAsync(cancellation, OnLoadProgress);

            await m_extensionExecutor.ExecuteAsync<IGameFinishLoadingExtension>(extension => extension.OnGameFinishLoading());
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await Load(cancellation);
        }

        public async UniTask ShutdownAsync(CancellationToken cancellationToken = default)
        {
            GameLogger.Log("[GameManager] Starting shutdown sequence");

            if (m_mapLoader != null)
            {
                GameLogger.Log("[GameManager] Unloading map scene");
                await m_mapLoader.UnloadAsync(cancellationToken);
            }

            GameLogger.Log("[GameManager] Shutdown complete");
        }

        public async UniTask RestartAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(CurrentMapId))
            {
                GameLogger.Error("[GameManager] Cannot restart: no map loaded");
                return;
            }

            GameLogger.Log($"[GameManager] Restarting with map: {CurrentMapId}");

            await ShutdownAsync(cancellationToken);
            await StartAsync(cancellationToken);
        }

        private string GetMapId()
        {
            var config = m_configurationProvider?.GetConfiguration();
            if (config != null && !string.IsNullOrEmpty(config.mapId))
            {
                GameLogger.Log($"[GameManager] Using map ID from configuration: {config.mapId}");
                return config.mapId;
            }

            GameLogger.Log($"[GameManager] Using selected map ID: {selectedMapId}");
            return selectedMapId;
        }

        private void OnLoadProgress(float progress, string loaderName, int completed, int total)
        {
            GameLogger.Log($"[GameManager] Loading progress: {completed}/{total} ({progress:P0}) - Completed: {loaderName}");
            m_extensionExecutor.Execute<IGameLoadProgressExtension>(extension => extension.OnGameLoadProgress(progress, loaderName, completed, total));
        }
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Content.Attributes;
using Game.Core.Extensions;
using Game.Core.Initialization;
using Game.Core.Lifecycle;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Core.Settings;
using Game.Features.Bootstraps;
using Game.Features.Maps;
using Game.Features.Maps.Content;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game
{
    public class GameManager : MonoBehaviour, IAsyncStartable
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

        [ContentSelector(typeof(ContentMap))]
        public string selectedMapId;

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            await Load(cancellation);
        }

        private async UniTask Load(CancellationToken cancellation)
        {
            await m_extensionExecutor.ExecuteAsync<IGameStartLoadingExtension>(extension => extension.OnGameStartLoading());

            RuntimeSettingsLoader.LoadAllSettings(m_reflectionManager);

            var mapLoader = new MapLoader(gameScope, m_contentManager, selectedMapId);

            await new LoaderConfiguration()
                .Register(m_contentManager)
                .Register(mapLoader).After(m_contentManager)
                .Register(m_ecsBootstrap).After(mapLoader)
                .LoadAsync(cancellation, OnLoadProgress);

            await m_extensionExecutor.ExecuteAsync<IGameFinishLoadingExtension>(extension => extension.OnGameFinishLoading());
        }

        private void OnLoadProgress(float progress, string loaderName, int completed, int total)
        {
            GameLogger.Log($"[GameManager] Loading progress: {completed}/{total} ({progress:P0}) - Completed: {loaderName}");
            m_extensionExecutor.Execute<IGameLoadProgressExtension>(extension => extension.OnGameLoadProgress(progress, loaderName, completed, total));
        }
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Content.Attributes;
using Game.Core.Initialization;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Core.Settings;
using Game.Features.Bootstraps;
using Game.Features.Entities.Content;
using Game.Features.LoadingScreen;
using Game.Features.Maps;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game
{
    public class GameManager : MonoBehaviour, IAsyncStartable
    {
        [Inject]
        private IObjectResolver m_objectResolver;

        [Inject]
        private IReflectionManager m_reflectionManager;

        [Inject]
        private IContentManager m_contentManager;

        [Inject]
        private EcsBootstrap m_ecsBootstrap;

        [Inject]
        private LoadingScreen m_loadingScreen;

        public LifetimeScope gameScope;

        [ContentSelector(typeof(ContentMap))]
        public string selectedMapId;

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            await Load(cancellation);
        }

        private async UniTask Load(CancellationToken cancellation)
        {
            m_loadingScreen.gameObject.SetActive(true);
            RuntimeSettingsLoader.LoadAllSettings(m_reflectionManager);

            var mapLoader = new MapLoader(m_contentManager, selectedMapId);

            await new LoaderConfiguration()
                .Register(m_contentManager)
                .Register(mapLoader).After(m_contentManager)
                .Register(m_ecsBootstrap).After(mapLoader)
                .LoadAsync(cancellation, OnLoadProgress);

            await UniTask.Delay(100, cancellationToken: cancellation);

            m_loadingScreen.gameObject.SetActive(false);
        }

        private void OnLoadProgress(float progress, string loaderName, int completed, int total)
        {
            GameLogger.Log($"[GameManager] Loading progress: {completed}/{total} ({progress:P0}) - Completed: {loaderName}");
            m_loadingScreen.SetProgress(progress, loaderName, completed, total);
        }
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Initialization;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Core.Settings;
using Game.Features.Bootstraps;
using Game.Features.LoadingScreen;
using VContainer;
using VContainer.Unity;

namespace Game
{
    public class GameManager : IAsyncStartable
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

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            m_loadingScreen.gameObject.SetActive(true);

            RuntimeSettingsLoader.LoadAllSettings(m_reflectionManager);

            // Configure and load with progress tracking
            await new LoaderConfiguration()
                .Register(m_contentManager)
                .Register(m_ecsBootstrap).After(m_contentManager)
                .LoadAsync(cancellation, OnLoadProgress);

            m_loadingScreen.gameObject.SetActive(false);
        }

        private void OnLoadProgress(float progress, string loaderName, int completed, int total)
        {
            GameLogger.Log($"[GameManager] Loading progress: {completed}/{total} ({progress:P0}) - Completed: {loaderName}");
            m_loadingScreen.SetProgress(progress, loaderName, completed, total);
        }
    }
}
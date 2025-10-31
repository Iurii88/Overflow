using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Initialization;
using Game.Core.Logging;
using Game.Core.Logging.Modules;
using Game.Core.Reflection;
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

            GameLogger.Initialize(LogLevel.Debug, new LevelModule(), new ColorModule());

            // Configure loader dependencies using fluent API
            var orderedLoaders = new LoaderConfiguration()
                .Register(m_contentManager)
                .Register(m_ecsBootstrap).After(m_contentManager)
                .ResolveOrder();

            // Load in the resolved dependency order
            foreach (var asyncLoader in orderedLoaders)
                await asyncLoader.LoadAsync(cancellation);

            m_loadingScreen.gameObject.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Initialization;
using Game.Core.Initialization.Interfaces;
using Game.Core.Logging;
using Game.Core.Logging.Modules;
using Game.Core.Reflection;
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
            var tasks = new List<IAsyncLoader>
            {
                m_contentManager,
                m_ecsBootstrap
            };

            //load in the specific order
            foreach (var asyncLoader in tasks)
                await asyncLoader.LoadAsync(cancellation);

            m_loadingScreen.gameObject.SetActive(false);
        }
    }
}
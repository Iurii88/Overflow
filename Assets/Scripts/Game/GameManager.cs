using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Bootstraps;
using Game.Core.Bootstraps.Interfaces;
using Game.Core.Content;
using Game.Core.Logging;
using Game.Core.Reflection;
using VContainer;
using VContainer.Unity;

namespace Game
{
    public class GameManager : IAsyncStartable
    {
        [Inject]
        private IReflectionManager m_reflectionManager;

        [Inject]
        private IContentManager m_contentManager;

        [Inject]
        private EcsBootstrap m_ecsBootstrap;

        [Inject]
        private IObjectResolver m_objectResolver;

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            GameLogger.Initialize(LogLevel.Debug, new LevelModule(), new ColorModule());
            var tasks = new List<IAsyncLoader>
            {
                m_contentManager,
                m_ecsBootstrap
            };
            
            //load in the specific order
            foreach (var asyncLoader in tasks)
                await asyncLoader.LoadAsync(cancellation);
        }
    }
}
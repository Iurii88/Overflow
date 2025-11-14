using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Content.Attributes;
using Game.Core.Extensions;
using Game.Core.Initialization;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Core.Reflection.Attributes;
using Game.Core.SceneLoading;
using Game.Core.VContainer;
using Game.Features.Bootstraps;
using Game.Features.LoadingScreen.Extensions;
using Game.Features.Maps;
using Game.Features.Maps.Content;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Game
{
    public interface IGameManager
    {
        UniTask RestartAsync(CancellationToken cancellationToken = default);
        UniTask GoToMainMenuAsync(CancellationToken cancellationToken = default);
    }

    public class GameManager : MonoBehaviour, IGameManager, IAsyncStartable
    {
        [Inject]
        public GameLifeTimeScope gameScope;

        [Inject]
        private IReflectionManager m_reflectionManager;

        [ContentSelector(typeof(ContentMap))]
        public string customMapId;

        private LifetimeScope m_childScope;
        private IExtensionExecutor m_extensionExecutor;

        //to set up parameters from MainMenu
        public static GameConfiguration Configuration;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            m_childScope = gameScope.CreateChild(builder => { AutoRegistration(m_reflectionManager, builder); });

            m_extensionExecutor = m_childScope.Container.Resolve<IExtensionExecutor>();
            await m_extensionExecutor.ExecuteAsync<IGameStartLoadingExtension>(extension => extension.OnGameStartLoading());

            var contentManager = m_childScope.Container.Resolve<IContentManager>();
            var ecsBootstrap = m_childScope.Container.Resolve<EcsBootstrap>();
            var mapLoader = m_childScope.Container.Resolve<MapLoader>();
            mapLoader.mapId = GetMapId();

            await new LoaderConfiguration()
                .Register(contentManager)
                .Register(mapLoader).After(contentManager)
                .Register(ecsBootstrap).After(mapLoader)
                .LoadAsync(cancellation, OnLoadProgress);

            await m_extensionExecutor.ExecuteAsync<IGameFinishLoadingExtension>(extension => extension.OnGameFinishLoading());
            await m_extensionExecutor.ExecuteAsync<ISessionStartExtension>(extension => extension.OnSessionStart());
        }

        public async UniTask RestartAsync(CancellationToken cancellationToken = default)
        {
            var asyncDisposables = m_childScope.Container.Resolve<IEnumerable<IUniTaskAsyncDisposable>>();

            foreach (var disposable in asyncDisposables)
                await disposable.DisposeAsync();

            m_childScope.Dispose();
            await StartAsync(cancellationToken);
        }

        public async UniTask GoToMainMenuAsync(CancellationToken cancellationToken = default)
        {
            if (m_extensionExecutor != null)
                await m_extensionExecutor.ExecuteAsync<ISessionEndExtension>(extension => extension.OnSessionEnd());

            if (m_childScope != null)
            {
                var asyncDisposables = m_childScope.Container.Resolve<IEnumerable<IUniTaskAsyncDisposable>>();

                foreach (var disposable in asyncDisposables)
                    await disposable.DisposeAsync();

                m_childScope.Dispose();
                m_childScope = null;
            }

            m_extensionExecutor = null;
            Configuration = null;

            await SceneManager.LoadSceneAsync("MainMenu");
        }

        private string GetMapId()
        {
            if (Configuration != null && !string.IsNullOrEmpty(Configuration.mapId))
            {
                GameLogger.Log($"[GameManager] Using map ID from configuration: {Configuration.mapId}");
                return Configuration.mapId;
            }

            GameLogger.Log($"[GameManager] Using selected map ID: {customMapId}");
            return customMapId;
        }

        private void OnLoadProgress(float progress, string loaderName, int completed, int total)
        {
            GameLogger.Log($"[GameManager] Loading progress: {completed}/{total} ({progress:P0}) - Completed: {loaderName}");
            m_extensionExecutor.Execute<IGameLoadProgressExtension>(extension => extension.OnGameLoadProgress(progress, loaderName, completed, total));
        }

        private static void AutoRegistration(IReflectionManager reflectionManager, IContainerBuilder builder)
        {
            var types = reflectionManager.GetByAttribute<AutoRegisterAttribute>();
            foreach (var typeInfo in types)
            {
                var lifetime = Lifetime.Singleton;
                var attribute = typeInfo.GetCustomAttributes(typeof(AutoRegisterAttribute), false);
                if (attribute.Length > 0 && attribute[0] is AutoRegisterAttribute autoRegister)
                    lifetime = autoRegister.Lifetime;

                builder.Register(typeInfo, lifetime).AsImplementedInterfaces().AsSelf();
            }
        }
    }
}
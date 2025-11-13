using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Factories;
using Game.Core.Initialization;
using Game.Core.Logging;
using Game.Core.Reflection.Attributes;
using UnsafeEcs.Core.Bootstrap;
using UnsafeEcs.Core.Worlds;
using VContainer;

namespace Game.Features.Bootstraps
{
    [AutoRegister]
    public class EcsBootstrap : IAsyncLoader, IUniTaskAsyncDisposable
    {
        [Inject]
        private readonly IObjectResolver m_resolver;

        [Inject]
        private IEntityFactory m_entityFactory;

        public async UniTask DisposeAsync()
        {
            await DestroyAllEntitiesAsync();
            WorldManager.Destroy();
        }

        private async UniTask DestroyAllEntitiesAsync()
        {
            var world = WorldManager.Worlds[0];
            if (world == null)
            {
                GameLogger.Warning("[EcsBootstrap] No world found during disposal");
                return;
            }

            var query = world.EntityManager.CreateQuery();
            var allEntities = query.Fetch();

            if (allEntities.Length == 0)
            {
                GameLogger.Log("[EcsBootstrap] No entities to destroy");
                return;
            }

            GameLogger.Log($"[EcsBootstrap] Destroying {allEntities.Length} entities before world cleanup");

            foreach (var entity in allEntities)
                await m_entityFactory.DestroyEntityAsync(world.entityManagerWrapper, entity);

            GameLogger.Log("[EcsBootstrap] All entities destroyed successfully");
        }

        public UniTask LoadAsync(CancellationToken cancellationToken)
        {
            var gameAssembly = Assembly.GetExecutingAssembly();
            var ecsAssembly = Assembly.Load("UnsafeEcs");
            var assemblies = new[]
            {
                gameAssembly,
                ecsAssembly
            };

            var world = WorldManager.CreateWorld();
            WorldBootstrap.onSystemCreated = system => m_resolver.Inject(system);
            WorldBootstrap.Initialize(assemblies, new List<World> { world }, new WorldBootstrapOptions
            {
                dontDestroyOnLoad = false,
                logLevel = WorldBootstrap.LogLevel.Normal
            });

            return UniTask.CompletedTask;
        }
    }
}
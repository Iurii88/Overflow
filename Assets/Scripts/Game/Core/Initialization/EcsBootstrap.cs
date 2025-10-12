using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Bootstraps.Interfaces;
using Game.Core.Reflection.Attributes;
using UnsafeEcs.Core.Bootstrap;
using UnsafeEcs.Core.Worlds;
using VContainer;

namespace Game.Core.Bootstraps
{
    [AutoRegister]
    public class EcsBootstrap : IAsyncLoader, IDisposable
    {
        [Inject]
        private readonly IObjectResolver m_resolver;
        
        public void Dispose()
        {
            WorldManager.DestroyAllWorlds();
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
            WorldBootstrap.Initialize(assemblies, new List<World> { world });

            return UniTask.CompletedTask;
        }
    }
}
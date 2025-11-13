using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Initialization;
using Game.Core.Reflection.Attributes;
using UnsafeEcs.Core.Bootstrap;
using UnsafeEcs.Core.Worlds;
using VContainer;

namespace Game.Features.Bootstraps
{
    [AutoRegister]
    public class EcsBootstrap : IAsyncLoader, IDisposable
    {
        [Inject]
        private readonly IObjectResolver m_resolver;

        public void Dispose()
        {
            WorldManager.Destroy();
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
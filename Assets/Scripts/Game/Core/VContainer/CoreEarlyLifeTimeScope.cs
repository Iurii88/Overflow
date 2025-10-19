using Game.Core.Reflection;
using VContainer;
using VContainer.Unity;

namespace Game.Core.VContainer
{
    public class CoreEarlyLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IReflectionManager, ReflectionManager>(Lifetime.Singleton).As<IInitializable>();
        }
    }
}
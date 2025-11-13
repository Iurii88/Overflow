using Game.Core.Reflection;
using Game.Core.Settings;
using VContainer;
using VContainer.Unity;

namespace Game.Core.VContainer
{
    public class ProjectContextScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ReflectionManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RuntimeSettingsLoader>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
}
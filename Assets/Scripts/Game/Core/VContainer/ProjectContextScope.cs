using Game.Core.Content;
using Game.Core.Content.Converters.Registry;
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
            builder.Register<JsonConverterRegistry>(Lifetime.Singleton).AsSelf();
            builder.Register<ContentManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
}
using Game.Core.Content;
using Game.Core.Content.Converters.Registry;
using Game.Core.Reflection;
using VContainer;
using VContainer.Unity;

namespace Game.Core.Scopes
{
    public class CoreLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IReflectionManager, ReflectionManager>(Lifetime.Singleton).As<IInitializable>();
            builder.Register<JsonConverterRegistry>(Lifetime.Singleton);
            builder.Register<IContentManager, ContentManager>(Lifetime.Singleton).As<IAsyncStartable>();
        }
    }
}
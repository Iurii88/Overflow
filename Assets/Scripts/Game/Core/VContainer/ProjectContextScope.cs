using Game.Core.Addressables;
using Game.Core.Content;
using Game.Core.Content.Converters.Registry;
using Game.Core.Reflection;
using Game.Core.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Core.VContainer
{
    public class ProjectContextScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            Application.targetFrameRate = 60;

            builder.Register<ReflectionManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RuntimeSettingsLoader>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<JsonConverterRegistry>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<ContentManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<GlobalAddressableManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
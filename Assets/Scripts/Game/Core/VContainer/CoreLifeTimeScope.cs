using System;
using Game.Core.Reflection;
using Game.Core.Reflection.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Game.Core.VContainer
{
    public class CoreLifeTimeScope : LifetimeScope
    {
        [SerializeField]
        private InputActionAsset playerInput;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerInput);

            var reflectionManager = Parent.Container.Resolve<IReflectionManager>();
            RegisterTypesWithAttribute<AutoRegisterAttribute>(reflectionManager, builder);
        }

        private static void RegisterTypesWithAttribute<TAttribute>(IReflectionManager reflectionManager, IContainerBuilder builder) where TAttribute : Attribute
        {
            var types = reflectionManager.GetByAttribute<TAttribute>();
            foreach (var typeInfo in types)
                builder.Register(typeInfo, Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
}
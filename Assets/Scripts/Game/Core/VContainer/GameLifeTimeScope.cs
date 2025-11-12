using System;
using Game.Core.Reflection;
using Game.Core.Reflection.Attributes;
using Game.Core.UI.Layers;
using Game.Features.LoadingScreen;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Game.Core.VContainer
{
    public class GameLifeTimeScope : LifetimeScope
    {
        [SerializeField]
        private InputActionAsset playerInput;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerInput);
            builder.RegisterComponentInHierarchy<GameManager>().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<LoadingScreen>();
            builder.RegisterComponentInHierarchy<UILayerManager>();

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
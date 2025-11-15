using Game.Core.Input.Joysticks;
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
            builder.RegisterComponentInHierarchy<FloatingJoystick>();
        }
    }
}
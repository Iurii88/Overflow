using Game.Core.Input.Joysticks;
using Game.Core.UI.Layers;
using Game.Features.LoadingScreen;
using Game.Features.Sessions;
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

        [SerializeField]
        private GameManager gameManager;

        [SerializeField]
        private LoadingScreen loadingScreen;

        [SerializeField]
        private UILayerManager uiLayerManager;

        [SerializeField]
        private FloatingJoystick floatingJoystick;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerInput);
            builder.RegisterComponent(gameManager).AsImplementedInterfaces();
            builder.RegisterComponent(loadingScreen);
            builder.RegisterComponent(uiLayerManager);
            builder.RegisterComponent(floatingJoystick);
        }
    }
}
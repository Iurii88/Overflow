using Game.Features.LoadingScreen;
using VContainer;
using VContainer.Unity;

namespace Game.VContainer
{
    public class GameLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameManager>();
            builder.RegisterComponentInHierarchy<LoadingScreen>();
        }
    }
}
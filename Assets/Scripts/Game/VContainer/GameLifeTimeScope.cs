using VContainer;
using VContainer.Unity;

namespace Game.Scopes
{
    public class GameLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameManager>();
        }
    }
}
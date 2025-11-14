using Game.Features.Pause;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Pause.Groups
{
    [UpdateInWorld(WorldBootstrap.AllWorldsIndex)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class PauseAwareSystemGroup : SystemGroup
    {
        [Inject]
        private IPauseManager m_pauseManager;

        public override void OnUpdate()
        {
            if (m_pauseManager.IsPaused)
                return;

            base.OnUpdate();
        }
    }
}
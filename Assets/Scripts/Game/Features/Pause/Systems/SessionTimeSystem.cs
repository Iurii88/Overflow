using Game.Features.Pause.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Pause.Systems
{
    [UpdateInGroup(typeof(TimeSystemGroup))]
    public class SessionTimeSystem : SystemBase
    {
        [Inject]
        private ISessionTime m_sessionTime;

        public override void OnUpdate()
        {
            m_sessionTime.Update();
        }
    }
}
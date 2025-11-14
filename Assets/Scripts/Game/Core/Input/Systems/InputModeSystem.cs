using Game.Core.UI;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Core.Input.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InputModeSystem : SystemBase
    {
        public override SystemUpdateMask UpdateMask => SystemUpdateMask.Update;

        [Inject]
        private IInputModeManager m_inputModeManager;

        [Inject]
        private IWindowManager m_windowManager;

        private bool m_wasWindowOpen;

        public override void OnAwake()
        {
            m_wasWindowOpen = false;
        }

        public override void OnUpdate()
        {
            var hasOpenWindows = m_windowManager.OpenWindows.Count > 0;

            if (hasOpenWindows && !m_wasWindowOpen)
            {
                m_inputModeManager.PushMode("inputmode.ui");
            }
            else if (!hasOpenWindows && m_wasWindowOpen)
            {
                m_inputModeManager.PopMode();
            }

            m_wasWindowOpen = hasOpenWindows;
        }
    }
}

using Cysharp.Threading.Tasks;
using Game.Core.UI;
using VContainer;

namespace Game.Features.Pause.UI
{
    public class PauseMenu : AWindowViewComponent
    {
        [Inject]
        private IGameManager m_gameManager;

        protected override void Subscribe()
        {
        }

        protected override void OnWindowOpen()
        {
            base.OnWindowOpen();
        }

        protected override void OnWindowClose()
        {
            base.OnWindowClose();
        }

        public void ToMainMenu()
        {
            m_gameManager.GoToMainMenuAsync().Forget();
        }

        public void Restart()
        {
            m_gameManager.RestartAsync().Forget();
        }
    }
}
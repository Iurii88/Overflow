using Cysharp.Threading.Tasks;
using Game.Core.UI;
using Game.Features.Sessions;
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
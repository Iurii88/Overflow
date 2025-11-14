using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Game.Features.Pause.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Inject]
        private IGameManager m_gameManager;

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
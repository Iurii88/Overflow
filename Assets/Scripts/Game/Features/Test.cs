using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Game.Features
{
    public class Test : MonoBehaviour
    {
        [Inject]
        private IGameManager m_gameManager;
        
        public void ToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        public void Restart()
        {
            m_gameManager.RestartAsync();
        }
    }
}
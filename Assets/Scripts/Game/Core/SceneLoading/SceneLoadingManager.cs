using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core.SceneLoading
{
    public class SceneLoadingManager : MonoBehaviour, IGameSceneConfigurationProvider
    {
        private static SceneLoadingManager s_instance;

        public static SceneLoadingManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("SceneLoadingManager");
                    s_instance = go.AddComponent<SceneLoadingManager>();
                    DontDestroyOnLoad(go);
                }

                return s_instance;
            }
        }

        private readonly SceneLoadingService m_service = new();

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public GameSceneConfiguration GetConfiguration()
        {
            return m_service.GetConfiguration();
        }

        public async UniTask LoadGameSceneAsync(GameSceneConfiguration configuration, CancellationToken cancellationToken = default)
        {
            await m_service.LoadGameSceneAsync(configuration, cancellationToken);
        }

        public async UniTask UnloadGameSceneAsync(CancellationToken cancellationToken = default)
        {
            await m_service.UnloadGameSceneAsync(cancellationToken);
        }
    }
}

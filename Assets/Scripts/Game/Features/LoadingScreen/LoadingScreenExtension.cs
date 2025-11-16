using Cysharp.Threading.Tasks;
using Game.Features.LoadingScreen.Extensions;
using Game.Features.Sessions.Attributes;
using VContainer;

namespace Game.Features.LoadingScreen
{
    [AutoRegister]
    public class LoadingScreenExtension : IGameStartLoadingExtension, IGameFinishLoadingExtension, IGameLoadProgressExtension
    {
        [Inject]
        private readonly LoadingScreen m_loadingScreen;

        public UniTask OnGameStartLoading()
        {
            m_loadingScreen.gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public UniTask OnGameFinishLoading()
        {
            m_loadingScreen.gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void OnGameLoadProgress(float progress, string loaderName, int completed, int total)
        {
            m_loadingScreen.SetProgress(progress, loaderName, completed, total);
        }
    }
}
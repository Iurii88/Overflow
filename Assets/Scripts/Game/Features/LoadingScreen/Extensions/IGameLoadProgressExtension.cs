using Game.Core.Extensions;

namespace Game.Features.LoadingScreen.Extensions
{
    public interface IGameLoadProgressExtension : IExtension
    {
        void OnGameLoadProgress(float progress, string loaderName, int completed, int total);
    }
}

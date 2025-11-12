using Game.Core.Extensions;

namespace Game.Core.Lifecycle
{
    public interface IGameLoadProgressExtension : IExtension
    {
        void OnGameLoadProgress(float progress, string loaderName, int completed, int total);
    }
}

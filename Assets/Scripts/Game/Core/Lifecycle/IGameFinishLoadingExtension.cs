using Cysharp.Threading.Tasks;
using Game.Core.Extensions;

namespace Game.Core.Lifecycle
{
    public interface IGameFinishLoadingExtension : IExtension
    {
        UniTask OnGameFinishLoading();
    }
}

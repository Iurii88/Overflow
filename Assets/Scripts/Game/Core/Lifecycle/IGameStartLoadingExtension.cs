using Cysharp.Threading.Tasks;
using Game.Core.Extensions;

namespace Game.Core.Lifecycle
{
    public interface IGameStartLoadingExtension : IExtension
    {
        UniTask OnGameStartLoading();
    }
}

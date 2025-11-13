using Cysharp.Threading.Tasks;
using Game.Core.Extensions;

namespace Game.Features.LoadingScreen.Extensions
{
    public interface IGameStartLoadingExtension : IExtension
    {
        UniTask OnGameStartLoading();
    }
}

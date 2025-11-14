using Cysharp.Threading.Tasks;
using Game.Core.Extensions;

namespace Game.Features.LoadingScreen.Extensions
{
    public interface ISessionEndExtension : IExtension
    {
        UniTask OnSessionEnd();
    }
}

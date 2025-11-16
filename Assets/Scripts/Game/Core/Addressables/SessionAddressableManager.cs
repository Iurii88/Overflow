using Cysharp.Threading.Tasks;
using Game.Features.Sessions.Attributes;

namespace Game.Core.Addressables
{
    [AutoRegister]
    public class SessionAddressableManager : BaseAddressableManager, ISessionAddressableManager, IUniTaskAsyncDisposable
    {
        public UniTask DisposeAsync()
        {
            Dispose();
            return UniTask.CompletedTask;
        }
    }
}
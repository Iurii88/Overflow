using Cysharp.Threading.Tasks;
using Game.Core.Initialization;

namespace Game.Features.Bootstraps
{
    public interface IEcsBootstrap : IAsyncLoader, IUniTaskAsyncDisposable
    {
    }
}

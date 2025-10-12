using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Core.Bootstraps.Interfaces
{
    public interface IAsyncLoader
    {
        public UniTask LoadAsync(CancellationToken cancellationToken);
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Core.Initialization
{
    public interface IAsyncLoader
    {
        public UniTask LoadAsync(CancellationToken cancellationToken);
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Initialization;

namespace Game.Features.Maps
{
    public interface IMapLoader : IAsyncLoader, IUniTaskAsyncDisposable
    {
        void SetMapId(string mapId);
        UniTask UnloadAsync(CancellationToken cancellationToken = default);
    }
}
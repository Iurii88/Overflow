using Cysharp.Threading.Tasks;
using Game.Core.Extensions;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Lifecycle
{
    public interface IEntityDestroyedExtension : IFilterableExtension
    {
        UniTask OnEntityDestroyed(Entity entity, ContentEntity contentEntity);
    }
}
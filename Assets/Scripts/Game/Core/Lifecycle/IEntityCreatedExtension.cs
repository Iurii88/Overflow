using Cysharp.Threading.Tasks;
using Game.Core.Extensions;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Lifecycle
{
    public interface IEntityCreatedExtension : IExtension
    {
        UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity);
    }
}
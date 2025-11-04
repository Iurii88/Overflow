using Cysharp.Threading.Tasks;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Utils;

namespace Game.Core.Factories
{
    public interface IEntityFactory
    {
        UniTask<Entity> CreateEntityAsync(ReferenceWrapper<EntityManager> entityManager, string contentId);
    }
}
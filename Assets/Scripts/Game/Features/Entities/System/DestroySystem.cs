using System;
using Cysharp.Threading.Tasks;
using Game.Core.Factories;
using Game.Core.Logging;
using Game.Features.Entities.Components;
using Unity.Collections.LowLevel.Unsafe;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Entities.System
{
    [UpdateInGroup(typeof(CleanUpSystemGroup))]
    public class DestroySystem : SystemBase
    {
        public override SystemUpdateMask UpdateMask => SystemUpdateMask.Update;

        private EntityQuery m_query;

        [Inject]
        private IEntityFactory m_entityFactory;

        public override void OnAwake()
        {
            m_query = CreateQuery().With<Destroy>();
        }

        public override void OnUpdate()
        {
            var entities = m_query.Fetch();

            if (entities.Length == 0)
                return;

            DestroyEntitiesAsync(entities).Forget();
        }

        private async UniTask DestroyEntitiesAsync(UnsafeList<Entity> entities)
        {
            foreach (var entity in entities)
            {
                try
                {
                    await m_entityFactory.DestroyEntityAsync(world.entityManagerWrapper, entity);
                }
                catch (Exception e)
                {
                    GameLogger.Error($"Failed to destroy entity: {entity}, exception: {e}");
                    throw;
                }
            }
        }
    }
}
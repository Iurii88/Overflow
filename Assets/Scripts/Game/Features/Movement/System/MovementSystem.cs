using Cysharp.Threading.Tasks;
using Game.Core.Factories;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Movement.System
{
    [UpdateInGroup(typeof(AllWorldInitializationSystemGroup))]
    public class MovementSystem : SystemBase
    {
        private EntityQuery m_query;

        [Inject]
        private IEntityFactory m_entityFactory;

        public override void OnAwake()
        {
            m_query = CreateQuery();
            LoadPlayer().Forget();
        }

        private async UniTask LoadPlayer()
        {
            await m_entityFactory.CreateEntityAsync(world.entityManagerWrapper, "entity.player");
        }

        public override void OnUpdate()
        {
            //Debug.Log("MovementSystem");
            var entities = m_query.Fetch();

            new MovementJobParallel
                {
                    entities = entities,
                    deltaTime = world.deltaTime
                }
                .Schedule(entities.Length, 512).Complete();
        }

        [BurstCompile]
        private struct MovementJobParallel : IJobParallelFor
        {
            [ReadOnly]
            public UnsafeList<Entity> entities;

            public float deltaTime;

            public void Execute(int index)
            {
                var entity = entities[index];
                // ref var transform = ref transforms.Get(entity);
                // transform.Translate(new float3(0, 0, 1) * deltaTime);
            }
        }
    }
}
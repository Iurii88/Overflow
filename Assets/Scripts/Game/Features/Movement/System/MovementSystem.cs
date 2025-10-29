using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Content;
using Game.Features.Entities.Content;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
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
        private IContentManager m_contentManager;

        [Inject]
        private IAddressableManager m_addressableManager;

        public override void OnAwake()
        {
            m_query = CreateQuery();
            LoadPlayer().Forget();
        }

        private async UniTask LoadPlayer()
        {
            await UniTask.WaitUntil(() => m_contentManager.isInitialized);
            var contentPlayer = m_contentManager.Get<ContentEntity>("entity.player");
            var viewContentProperty = contentPlayer.GetProperty<ViewContentProperty>();
            var playerPrefab = await m_addressableManager.LoadAssetAsync<GameObject>(viewContentProperty.assetPath);
            var player = Object.Instantiate(playerPrefab);
            var playerEntity = entityManagerWrapper.Value.CreateEntity();
            var managedRef = world.managedStorage.Add(player);
            playerEntity.AddComponent(managedRef);
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
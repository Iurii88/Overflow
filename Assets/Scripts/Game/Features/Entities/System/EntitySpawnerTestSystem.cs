using Cysharp.Threading.Tasks;
using Game.Core.Factories;
using Game.Core.Logging;
using UnityEngine;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Entities.System
{
    [UpdateInGroup(typeof(AllWorldInitializationSystemGroup))]
    public class EntitySpawnerTestSystem : SystemBase
    {
        [Inject]
        private IEntityFactory m_entityFactory;

        public override void OnAwake()
        {
            TestSpawnEntities().Forget();
        }

        private async UniTask TestSpawnEntities()
        {
            GameLogger.Log("EntitySpawnerTestSystem: Starting entity spawn test...");

            await UniTask.Delay(300);

            var playerContentId = "entity.player";
            var playerEntity = await m_entityFactory.CreateEntityAsync(world.entityManagerWrapper, playerContentId);
            if (playerEntity != default)
            {
                GameLogger.Log($"Successfully spawned player entity: {playerContentId}");
            }

            await UniTask.Delay(300);

            var enemyContentId = "entity.enemy.triangle";
            var enemyEntity = await m_entityFactory.CreateEntityAsync(world.entityManagerWrapper, enemyContentId);
            if (enemyEntity != default)
            {
                GameLogger.Log($"Successfully spawned enemy entity: {enemyContentId}");

                if (enemyEntity.TryGetReference(out GameObject enemyGameObject))
                {
                    enemyGameObject.transform.position = new Vector3(3f, 0f, 0f);
                }
            }

            //enemyEntity.SetComponent<Destroy>();

            GameLogger.Log("EntitySpawnerTestSystem: Spawn test completed!");
        }

        public override void OnUpdate()
        {
        }
    }
}
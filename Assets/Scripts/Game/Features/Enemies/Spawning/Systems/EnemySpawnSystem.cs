using Cysharp.Threading.Tasks;
using Game.Core.Factories;
using Game.Core.Logging;
using Game.Features.Movement.Components;
using Game.Features.Pause;
using Unity.Mathematics;
using UnityEngine;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Systems;
using VContainer;
using Random = Unity.Mathematics.Random;

namespace Game.Features.Enemies.Spawning.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EnemySpawnSystem : SystemBase
    {
        [Inject]
        private IEntityFactory m_entityFactory;

        [Inject]
        private ISessionTime m_sessionTime;

        private Random m_random;
        private float m_spawnTimer;
        private const float SpawnInterval = 2f;
        private const string EnemyContentId = "entity.enemy.triangle";
        private const float SpawnDistance = 15f;

        public override SystemUpdateMask UpdateMask { get; set; } = SystemUpdateMask.None;

        public override void OnAwake()
        {
            m_random = new Random((uint)System.DateTime.Now.Ticks);
            SpawnInitialEnemies().Forget();
        }

        public override void OnUpdate()
        {
            m_spawnTimer += m_sessionTime.DeltaTime;

            if (m_spawnTimer >= SpawnInterval)
            {
                m_spawnTimer = 0f;
                SpawnEnemy().Forget();
            }
        }

        private async UniTask SpawnInitialEnemies()
        {
            for (int i = 0; i < 3; i++)
            {
                await SpawnEnemy();
            }
            UpdateMask = SystemUpdateMask.Update;
            GameLogger.Log("Initial enemies spawned");
        }

        private async UniTask SpawnEnemy()
        {
            var spawnPosition = GetRandomSpawnPosition();
            var enemyEntity = await m_entityFactory.CreateEntityAsync(world.entityManagerWrapper, EnemyContentId);

            if (enemyEntity != default)
            {
                enemyEntity.SetComponent(new Position { value = spawnPosition });
            }
        }

        private float2 GetRandomSpawnPosition()
        {
            var angle = m_random.NextFloat(0f, math.PI * 2f);
            var distance = SpawnDistance;

            var x = math.cos(angle) * distance;
            var y = math.sin(angle) * distance;

            return new float2(x, y);
        }
    }
}

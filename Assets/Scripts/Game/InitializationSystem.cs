using Cysharp.Threading.Tasks;
using Game.Core.Factories;
using Game.Core.Logging;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Entities.System
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializationSystem : SystemBase
    {
        [Inject]
        private IEntityFactory m_entityFactory;

        public override SystemUpdateMask UpdateMask { get; set; } = SystemUpdateMask.None;

        public override void OnAwake()
        {
            TestSpawnEntities().Forget();
        }

        private async UniTask TestSpawnEntities()
        {
            const string playerContentId = "entity.player";
            var playerEntity = await m_entityFactory.CreateEntityAsync(world.entityManagerWrapper, playerContentId);
            if (playerEntity != default)
            {
                GameLogger.Log($"Successfully spawned player entity: {playerContentId}");
            }
        }
    }
}
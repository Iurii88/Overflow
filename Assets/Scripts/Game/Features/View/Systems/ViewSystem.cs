using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Content;
using Game.Features.Entities.Content;
using Game.Features.View.Components;
using UnityEngine;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Movement.System
{
    [UpdateInGroup(typeof(AllWorldInitializationSystemGroup))]
    public class ViewSystem : SystemBase
    {
        public override SystemUpdateMask UpdateMask => SystemUpdateMask.Update;

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
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            m_query.ForEach((ref Entity entity, ref ViewComponent viewComponent) =>
            {
            });
        }
    }
}
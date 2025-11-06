using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content;
using Game.Core.Initialization;
using Game.Core.Logging;
using Game.Features.Maps.Content;
using UnityEngine.SceneManagement;

namespace Game.Features.Maps
{
    public class MapLoader : IAsyncLoader
    {
        private readonly IContentManager m_contentManager;
        private readonly string m_mapId;

        public MapLoader(IContentManager contentManager, string mapId)
        {
            m_contentManager = contentManager;
            m_mapId = mapId;
        }

        public async UniTask LoadAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(m_mapId))
            {
                GameLogger.Warning("[MapLoader] No map ID specified, skipping scene load");
                return;
            }

            var map = m_contentManager.Get<ContentMap>(m_mapId);
            if (map == null)
            {
                GameLogger.Error($"[MapLoader] Map not found: {m_mapId}");
                return;
            }

            if (string.IsNullOrEmpty(map.scene))
            {
                GameLogger.Warning($"[MapLoader] Map '{m_mapId}' has no scene specified");
                return;
            }

            GameLogger.Log($"[MapLoader] Loading scene: {map.scene}");

            var asyncOperation = SceneManager.LoadSceneAsync(map.scene, LoadSceneMode.Additive);
            if (asyncOperation == null)
            {
                GameLogger.Error($"[MapLoader] Failed to start loading scene: {map.scene}");
                return;
            }

            await asyncOperation.ToUniTask(cancellationToken: cancellationToken);

            GameLogger.Log($"[MapLoader] Scene loaded successfully: {map.scene}");
        }
    }
}
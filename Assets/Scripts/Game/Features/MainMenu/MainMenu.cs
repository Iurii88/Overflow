using System;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Attributes;
using Game.Core.Logging;
using Game.Core.SceneLoading;
using Game.Features.Maps.Content;
using UnityEngine;

namespace Game.Features.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        [ContentSelector(typeof(ContentMap))]
        public string mapId;

        public async void StartGame()
        {
            try
            {
                await StartGameAsync(mapId);
            }
            catch (Exception e)
            {
                GameLogger.Error($"[MainMenuManager] Failed to start game: {e.Message}");
            }
        }

        private static async UniTask StartGameAsync(string mapId)
        {
            GameLogger.Log($"[MainMenuManager] Starting game with map: {mapId}");

            var configuration = new GameSceneConfiguration(mapId);

            await SceneLoadingManager.Instance.LoadGameSceneAsync(configuration);
        }

        public void Shutdown()
        {
            Application.Quit();
        }
    }
}
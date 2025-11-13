using System;

namespace Game.Core.SceneLoading
{
    [Serializable]
    public class GameSceneConfiguration
    {
        public string mapId;

        public GameSceneConfiguration(string mapId)
        {
            this.mapId = mapId;
        }
    }
}

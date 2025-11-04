using System;
using System.Collections.Generic;

namespace Game.Core.Settings
{
    [Serializable]
    public class GameSettingsData
    {
        public Dictionary<string, string> moduleSettings = new();
    }
}
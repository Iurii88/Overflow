using System.IO;
using Game.Core.Logging;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Core.Settings.Editor
{
    public class GameSettingsBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var standalonePresetPath = GameSettingsManager.GetStandalonePresetPath();

            if (!File.Exists(standalonePresetPath))
            {
                Debug.LogWarning($"[GameSettings] Standalone preset not found at {standalonePresetPath}. Build will use default settings.");
                return;
            }

            var targetPath = Path.Combine(Application.streamingAssetsPath, "GameSettings_Default.json");

            try
            {
                if (!Directory.Exists(Application.streamingAssetsPath))
                    Directory.CreateDirectory(Application.streamingAssetsPath);

                File.Copy(standalonePresetPath, targetPath, true);
                Debug.Log($"[GameSettings] Copied standalone preset to StreamingAssets for build.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameSettings] Failed to copy standalone preset: {ex.Message}");
            }
        }
    }
}

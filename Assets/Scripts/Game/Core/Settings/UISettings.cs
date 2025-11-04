using Game.Core.Reflection;
using Game.Core.Settings.Attributes;

namespace Game.Core.Settings
{
    [GameSettings("UI", 10)]
    public class UISettings : AGameSettings
    {
        public bool showFPS = false;
        public bool showMinimap = true;
        public bool showHealthBars = true;

        [UnityEngine.Range(0.3f, 1f)]
        public float hudOpacity = 0.9f;

        public bool tooltipsEnabled = true;

        [UnityEngine.Range(0f, 2f)]
        public float tooltipDelay = 0.5f;

        [UnityEngine.Range(0.8f, 1.5f)]
        public float uiScale = 1f;

        public bool showTutorialHints = true;
        public bool animationsEnabled = true;

        public override void OnBeforeApply(IReflectionManager reflectionManager)
        {
            // Called before Apply() - use for initialization if needed
        }

        public override void Apply()
        {
            // TODO: Implement UI settings application logic
            // Example:
            // - Update FPS counter visibility
            // - Update minimap visibility
            // - Apply HUD opacity
            // - Configure tooltip system
            // - Apply UI scale to Canvas scalers
            // - Enable/disable UI animations

            // Example implementation:
            // UIManager.ShowFPS = showFPS;
            // UIManager.ShowMinimap = showMinimap;
            // UIManager.SetHudOpacity(hudOpacity);
            // UIManager.SetUIScale(uiScale);
            // TooltipManager.Enabled = tooltipsEnabled;
            // TooltipManager.Delay = tooltipDelay;
        }

#if UNITY_EDITOR
        // Optional: Override to customize editor UI for specific fields
        // public override bool DrawEditorField(System.Reflection.FieldInfo field, object defaultInstance, bool isStandalonePreset)
        // {
        //     if (field.Name == "customField")
        //     {
        //         // Draw custom UI for this field
        //         return true; // Return true to skip default drawing
        //     }
        //
        //     return false; // Return false to use default field drawer
        // }
#endif
    }
}
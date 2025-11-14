using System.Collections.Generic;
using Game.Core.Content;
using Game.Core.Input.Content;
using Game.Core.Logging;
using Game.Core.Reflection.Attributes;
using UnityEngine.InputSystem;
using VContainer;
using ZLinq;

namespace Game.Core.Input
{
    [AutoRegister]
    public class InputModeManager : IInputModeManager
    {
        private readonly InputActionAsset m_inputActionAsset;
        private readonly IContentManager m_contentManager;
        private readonly Stack<ContentInputMode> m_modeStack = new();
        private readonly Dictionary<string, InputActionMap> m_actionMaps = new();

        public ContentInputMode CurrentMode => m_modeStack.Count > 0 ? m_modeStack.Peek() : null;

        [Inject]
        public InputModeManager(InputActionAsset inputActionAsset, IContentManager contentManager)
        {
            m_inputActionAsset = inputActionAsset;
            m_contentManager = contentManager;

            CacheActionMaps();
            PushMode("inputmode.game");
        }

        public void PushMode(string modeId)
        {
            var mode = m_contentManager.Get<ContentInputMode>(modeId);
            if (mode == null)
            {
                GameLogger.Error($"Input mode '{modeId}' not found");
                return;
            }

            m_modeStack.Push(mode);
            ApplyCurrentMode();
        }

        public void PopMode()
        {
            if (m_modeStack.Count <= 1)
            {
                GameLogger.Warning("Cannot pop the last input mode from stack");
                return;
            }

            m_modeStack.Pop();
            ApplyCurrentMode();
        }

        private void CacheActionMaps()
        {
            foreach (var actionMap in m_inputActionAsset.actionMaps.AsValueEnumerable())
            {
                m_actionMaps[actionMap.name] = actionMap;
            }
        }

        private void ApplyCurrentMode()
        {
            foreach (var actionMap in m_actionMaps.Values.AsValueEnumerable())
            {
                actionMap?.Disable();
            }

            if (CurrentMode == null)
                return;

            foreach (var actionMapName in CurrentMode.actionMaps.AsValueEnumerable())
            {
                if (m_actionMaps.TryGetValue(actionMapName, out var actionMap))
                {
                    actionMap?.Enable();
                }
                else
                {
                    GameLogger.Warning($"Action map '{actionMapName}' not found in input mode '{CurrentMode.id}'");
                }
            }
        }
    }
}
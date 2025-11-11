using System.Collections.Generic;
using Game.Core.Common;
using Game.Core.Logging;
using UnityEngine;

namespace Game.Core.UI.Layers
{
    public class UILayerManager : BaseMonoBehaviour
    {
        private readonly Dictionary<string, Transform> m_layerByName = new();

        protected override void Awake()
        {
            base.Awake();
            for (var i = 0; i < transform.childCount; i++)
            {
                var v = transform.GetChild(i);
                m_layerByName[v.name] = v;
            }
        }

        public Transform Get(string layer)
        {
            if (m_layerByName.TryGetValue(layer, out var item))
                return item.transform;

            GameLogger.Warning($"UILayer {layer} not found. Make sure to register it first.");
            return null;
        }
    }
}
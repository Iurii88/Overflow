using System.Collections.Generic;
using Game.Core.Common;
using Game.Core.Logging;
using UnityEngine;

namespace Game.Core.UI.Layers
{
    public class UILayerManager : BaseMonoBehaviour
    {
        public Dictionary<UILayer, Transform> layerTransforms = new();

        public Transform GetLayerTransform(UILayer propertyLayer)
        {
            if (layerTransforms.TryGetValue(propertyLayer, out var layerTransform))
                return layerTransform;

            GameLogger.Warning($"UILayer {propertyLayer} not found. Make sure to register it first.");
            return null;
        }
    }
}
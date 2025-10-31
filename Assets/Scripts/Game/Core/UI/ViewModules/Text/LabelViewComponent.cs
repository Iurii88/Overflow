using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace Game.Core.UI.ViewModules.Text
{
    public abstract class LabelViewComponent<T> : AViewComponent
    {
        [SerializeField]
        private TMP_Text label;

        public BlackboardViewParameter<T> parameter;

        protected override void Reset()
        {
            base.Reset();
            label = GetComponent<TMP_Text>();
        }

        protected override void Subscribe()
        {
            parameter.OnVariableChanged += OnOnVariableChanged;
            UpdateLabel();
        }

        private void OnOnVariableChanged(BlackboardVariable<T> variable)
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (label == null)
                return;

            var value = parameter.Value;
            using var builder = ZString.CreateStringBuilder(true);
            builder.Append(value);
            var buffer = builder.AsArraySegment();
            label.SetText(buffer.Array, buffer.Offset, buffer.Count);
        }
    }
}
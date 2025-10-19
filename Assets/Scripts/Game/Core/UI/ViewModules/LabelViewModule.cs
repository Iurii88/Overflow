using TMPro;
using UnityEngine;

namespace Game.Core.ViewComponents.ViewModules
{
    public class IntLabelViewModule : LabelViewModule<int>
    {
    }

    public class FloatLabelViewModule : LabelViewModule<float>
    {
    }

    public class StringLabelViewModule : LabelViewModule<string>
    {
    }

    public class LabelViewModule<T> : AViewComponent
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
            parameter.OnValueChanged += OnOnValueChanged;
        }

        private void OnOnValueChanged(T value)
        {
            label.text = $"{value}";
        }
    }
}
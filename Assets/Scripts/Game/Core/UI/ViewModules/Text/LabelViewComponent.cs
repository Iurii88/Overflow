using Game.Core.ViewComponents;
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
        }

        private void OnOnVariableChanged(BlackboardVariable<T> variable)
        {
            label.text = $"{variable.value}";
        }
    }
}
using System.Text;
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

        [SerializeField]
        private int stringBuilderCapacity = 16;

        private readonly StringBuilder m_stringBuilder = new(16);

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
            m_stringBuilder.Clear();
            m_stringBuilder.Append(variable.value);
            label.SetText(m_stringBuilder);
        }
    }
}
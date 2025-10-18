using System.Reflection;
using UnityEngine;

namespace Game.Core.Blackboard
{
    public abstract class AViewComponent : MonoBehaviour
    {
        public Blackboard blackboard;

        protected virtual void Awake()
        {
            InitializeParameters();
        }

        private void InitializeParameters()
        {
            if (blackboard == null)
                return;

            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (IsBlackboardViewParameter(field.FieldType))
                {
                    var parameter = field.GetValue(this);
                    if (parameter != null)
                    {
                        var initMethod = field.FieldType.GetMethod("Initialize");
                        initMethod?.Invoke(parameter, new object[] { blackboard });
                    }
                }
            }
        }

        private bool IsBlackboardViewParameter(System.Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(BlackboardViewParameter<>);
        }

        public void Reset()
        {
            blackboard = GetComponent<Blackboard>();
        }
    }
}
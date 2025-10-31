using System;
using System.Reflection;
using Game.Core.Common;
using UnityEngine;

namespace Game.Core.UI
{
    [ExecuteAlways]
    public abstract class AViewComponent : BaseMonoBehaviour
    {
        public Blackboard blackboard;

        protected override void Awake()
        {
            base.Awake();
            InitializeParameters();
            Subscribe();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DisposeParameters();
        }

        protected virtual void Reset()
        {
            blackboard = GetComponent<Blackboard>();
            if (blackboard == null)
                blackboard = GetComponentInParent<Blackboard>();
        }

        protected abstract void Subscribe();

        private void InitializeParameters()
        {
            if (blackboard == null)
                return;

            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (!IsBlackboardViewParameter(field.FieldType))
                    continue;

                var parameter = field.GetValue(this);
                if (parameter == null)
                    continue;

                var initMethod = field.FieldType.GetMethod("Initialize");
                initMethod?.Invoke(parameter, new object[] { blackboard });
            }
        }

        private void DisposeParameters()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (!IsBlackboardViewParameter(field.FieldType))
                    continue;

                var parameter = field.GetValue(this);
                if (parameter == null)
                    continue;

                var disposeMethod = field.FieldType.GetMethod("Dispose");
                disposeMethod?.Invoke(parameter, null);
            }
        }

        private static bool IsBlackboardViewParameter(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(BlackboardViewParameter<>);
        }
    }
}
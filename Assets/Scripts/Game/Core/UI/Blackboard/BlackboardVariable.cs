using System;
using UnityEngine;

namespace Game.Core.UI.Blackboard
{
    [Serializable]
    public abstract class BlackboardVariable
    {
        [SerializeField]
        private string guid;

        public string key;

        public string Guid
        {
            get
            {
                if (string.IsNullOrEmpty(guid))
                {
                    guid = System.Guid.NewGuid().ToString();
                }
                return guid;
            }
        }

        public abstract Type GetValueType();
        public abstract object GetObjectValue();

#if UNITY_EDITOR
        /// <summary>
        /// Called by the editor to ensure GUID is generated when variable is created.
        /// </summary>
        public void EnsureGuid()
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = System.Guid.NewGuid().ToString();
            }
        }
#endif
    }

    [Serializable]
    public class BlackboardVariable<T> : BlackboardVariable
    {
        public T value;

        public BlackboardVariable()
        {
        }

        public BlackboardVariable(string key, T value)
        {
            this.key = key;
            this.value = value;
#if UNITY_EDITOR
            EnsureGuid();
#endif
        }

        public override Type GetValueType()
        {
            return typeof(T);
        }

        public override object GetObjectValue()
        {
            return value;
        }
    }
}
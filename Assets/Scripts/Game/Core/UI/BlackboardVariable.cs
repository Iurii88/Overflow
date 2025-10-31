using System;

namespace Game.Core.UI
{
    [Serializable]
    public abstract class BlackboardVariable
    {
        public string key;
        public abstract Type GetValueType();
        public abstract object GetObjectValue();
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
using System;

namespace Game.Core.Blackboard
{
    [Serializable]
    public abstract class BlackboardValue
    {
        public string key;
        public abstract Type GetValueType();
        public abstract object GetObjectValue();
    }

    [Serializable]
    public class BlackboardValue<T> : BlackboardValue
    {
        public T value;

        public BlackboardValue()
        {
        }

        public BlackboardValue(string key, T value)
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
using System;

namespace Game.Core.Blackboard
{
    [Serializable]
    public class BlackboardVariable
    {
        public Type type;
        public string key;
        public object value;
    }
}
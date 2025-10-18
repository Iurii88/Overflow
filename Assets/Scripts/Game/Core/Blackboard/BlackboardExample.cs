using UnityEngine;

namespace Game.Core.Blackboard
{
    public class BlackboardExample : MonoBehaviour
    {
        private Blackboard blackboard;

        private void Start()
        {
            blackboard = GetComponent<Blackboard>();

            // Работает с любыми типами без boxing
            blackboard.Set("health", 100);
            blackboard.Set("playerName", "Hero");
            blackboard.Set("position", transform.position);
            blackboard.Set("isAlive", true);

            // Получение значений
            var health = blackboard.Get<int>("health");
            var name = blackboard.Get<string>("playerName");
            var pos = blackboard.Get<Vector3>("position");

            Debug.Log($"Health: {health}, Name: {name}, Pos: {pos}");
        }
    }
}
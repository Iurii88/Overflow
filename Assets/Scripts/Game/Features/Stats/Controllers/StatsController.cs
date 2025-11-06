using System.Collections.Generic;
using Game.Core.EntityControllers;
using Game.Core.Logging;
using Game.Features.Stats.Data;

namespace Game.Features.Stats.Controllers
{
    public class StatsController : EntityController
    {
        private readonly Dictionary<string, float> m_stats = new();

        public void InitializeStats(Stat[] stats)
        {
            m_stats.Clear();

            if (stats == null)
                return;

            foreach (var stat in stats)
            {
                if (string.IsNullOrEmpty(stat.id))
                {
                    GameLogger.Warning($"Stat with empty id found on entity {Entity.id}");
                    continue;
                }

                m_stats[stat.id] = stat.value;
            }
        }

        public bool HasStat(string id)
        {
            return m_stats.ContainsKey(id);
        }

        public float GetStat(string id)
        {
            if (m_stats.TryGetValue(id, out var value))
                return value;

            GameLogger.Warning($"Stat '{id}' not found on entity {Entity.id}");
            return 0f;
        }

        public bool TryGetStat(string id, out float value)
        {
            return m_stats.TryGetValue(id, out value);
        }

        public void SetStat(string id, float value)
        {
            if (!m_stats.ContainsKey(id))
                GameLogger.Warning($"Stat '{id}' not found on entity {Entity.id}. Creating new stat.");

            m_stats[id] = value < 0f ? 0f : value;
        }

        public void ModifyStat(string id, float delta)
        {
            if (!m_stats.TryGetValue(id, out var currentValue))
            {
                GameLogger.Warning($"Stat '{id}' not found on entity {Entity.id}");
                return;
            }

            var newValue = currentValue + delta;
            m_stats[id] = newValue < 0f ? 0f : newValue;
        }

        public IReadOnlyDictionary<string, float> GetAllStats()
        {
            return m_stats;
        }
    }
}
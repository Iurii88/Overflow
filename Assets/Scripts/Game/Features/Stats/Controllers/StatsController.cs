using System.Collections.Generic;
using Game.Core.EntityControllers;
using Game.Core.Logging;
using Game.Features.Stats.Data;

namespace Game.Features.Stats.Controllers
{
    public class StatsController : EntityController
    {
        private readonly Dictionary<string, float> m_stats = new();
        private readonly Dictionary<string, float> m_maxStats = new();

        public void InitializeStats(Stat[] stats)
        {
            m_stats.Clear();
            m_maxStats.Clear();

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
                m_maxStats[stat.id] = stat.max > 0f ? stat.max : stat.value;
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

            var clampedValue = value < 0f ? 0f : value;

            if (m_maxStats.TryGetValue(id, out var max))
                clampedValue = clampedValue > max ? max : clampedValue;

            m_stats[id] = clampedValue;
        }

        public void ModifyStat(string id, float delta)
        {
            if (!m_stats.TryGetValue(id, out var currentValue))
            {
                GameLogger.Warning($"Stat '{id}' not found on entity {Entity.id}");
                return;
            }

            var newValue = currentValue + delta;
            var clampedValue = newValue < 0f ? 0f : newValue;

            if (m_maxStats.TryGetValue(id, out var max))
                clampedValue = clampedValue > max ? max : clampedValue;

            m_stats[id] = clampedValue;
        }

        public IReadOnlyDictionary<string, float> GetAllStats()
        {
            return m_stats;
        }

        public float GetMaxStat(string id)
        {
            if (m_maxStats.TryGetValue(id, out var max))
                return max;

            GameLogger.Warning($"Max stat '{id}' not found on entity {Entity.id}");
            return 0f;
        }

        public bool TryGetMaxStat(string id, out float max)
        {
            return m_maxStats.TryGetValue(id, out max);
        }

        public void ResetToMax(string id)
        {
            if (!m_stats.ContainsKey(id))
            {
                GameLogger.Warning($"Stat '{id}' not found on entity {Entity.id}");
                return;
            }

            if (m_maxStats.TryGetValue(id, out var max))
            {
                m_stats[id] = max;
            }
            else
            {
                GameLogger.Warning($"Max stat '{id}' not found on entity {Entity.id}. Cannot reset.");
            }
        }
    }
}
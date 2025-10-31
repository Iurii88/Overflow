using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Features.LoadingScreen
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        private Slider m_progressBar;

        [SerializeField]
        private TextMeshProUGUI m_statusText;

        [SerializeField]
        private TextMeshProUGUI m_percentageText;

        /// <summary>
        /// Updates the loading progress display.
        /// </summary>
        /// <param name="progress">Progress value between 0 and 1</param>
        /// <param name="statusMessage">Status message to display (e.g., "Loading ContentManager...")</param>
        public void SetProgress(float progress, string statusMessage)
        {
            if (m_progressBar != null)
                m_progressBar.value = progress;

            if (m_statusText != null)
                m_statusText.text = statusMessage;

            if (m_percentageText != null)
                m_percentageText.text = $"{progress:P0}";
        }

        /// <summary>
        /// Updates the loading progress display with completed/total counts.
        /// </summary>
        /// <param name="progress">Progress value between 0 and 1</param>
        /// <param name="loaderName">Name of the loader that just completed</param>
        /// <param name="completed">Number of completed loaders</param>
        /// <param name="total">Total number of loaders</param>
        public void SetProgress(float progress, string loaderName, int completed, int total)
        {
            SetProgress(progress, $"Loading {loaderName}... ({completed}/{total})");
        }
    }
}
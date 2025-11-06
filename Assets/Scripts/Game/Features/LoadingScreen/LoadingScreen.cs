using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Features.LoadingScreen
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        private Image progressBarFill;

        [SerializeField]
        private TextMeshProUGUI statusText;

        [SerializeField]
        private TextMeshProUGUI percentageText;

        public void SetProgress(float progress, string statusMessage)
        {
            if (progressBarFill != null)
                progressBarFill.fillAmount = progress;

            if (statusText != null)
                statusText.text = statusMessage;

            if (percentageText != null)
                percentageText.text = $"{progress:P0}";
        }

        public void SetProgress(float progress, string loaderName, int completed, int total)
        {
            SetProgress(progress, $"Loading {loaderName}... ({completed}/{total})");
        }
    }
}
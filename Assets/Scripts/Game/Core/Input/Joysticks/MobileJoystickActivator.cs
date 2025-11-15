using UnityEngine;

namespace Game.Core.Input.Joysticks
{
    public class MobileJoystickActivator : MonoBehaviour
    {
        [SerializeField]
        private GameObject joystickObject;

        public bool editorActive = true;

        private void Awake()
        {
            if (joystickObject == null)
                return;

            var isMobile = (editorActive && Application.isEditor)
                           || Application.platform == RuntimePlatform.Android
                           || Application.platform == RuntimePlatform.IPhonePlayer;

            joystickObject.SetActive(isMobile);
        }
    }
}
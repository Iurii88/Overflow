using Game.Features.Sessions.Attributes;

namespace Game.Core.Camera
{
    [AutoRegister]
    public class CameraManager : ICameraManager
    {
        private UnityEngine.Camera m_mainCamera;

        public UnityEngine.Camera MainCamera
        {
            get
            {
                if (m_mainCamera == null)
                    m_mainCamera = UnityEngine.Camera.main;

                return m_mainCamera;
            }
        }

        public void SetMainCamera(UnityEngine.Camera camera)
        {
            m_mainCamera = camera;
        }
    }
}
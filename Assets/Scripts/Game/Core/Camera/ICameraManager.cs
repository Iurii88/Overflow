namespace Game.Core.Camera
{
    public interface ICameraManager
    {
        UnityEngine.Camera MainCamera { get; }
        void SetMainCamera(UnityEngine.Camera camera);
    }
}
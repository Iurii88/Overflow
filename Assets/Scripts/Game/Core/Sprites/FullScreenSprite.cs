using Game.Core.Camera;
using R3;
using UnityEngine;
using VContainer;

namespace Game.Core.Sprites
{
    public class FullScreenSprite : MonoBehaviour
    {
        private readonly CompositeDisposable m_subscription = new();
        private UnityEngine.Camera m_cam;
        private SpriteRenderer m_sr;
        private ICameraManager m_cameraManager;

        [Inject]
        public void Construct(ICameraManager cameraManager)
        {
            m_cameraManager = cameraManager;
        }

        private void Start()
        {
            m_cam = m_cameraManager.MainCamera;
            m_sr = GetComponent<SpriteRenderer>();

            Observable.EveryUpdate()
                .Select(_ => new Vector2(Screen.width, Screen.height))
                .DistinctUntilChanged()
                .Subscribe(_ => UpdateSprite())
                .AddTo(m_subscription);

            UpdateSprite();
        }

        private void OnDestroy()
        {
            m_subscription?.Dispose();
        }

        private void UpdateSprite()
        {
            var screenHeight = m_cam.orthographicSize * 2f;
            var screenWidth = screenHeight * m_cam.aspect;

            Vector2 spriteSize = m_sr.sprite.bounds.size;

            transform.localScale = new Vector3(
                screenWidth / spriteSize.x,
                screenHeight / spriteSize.y,
                1f
            );
        }
    }
}
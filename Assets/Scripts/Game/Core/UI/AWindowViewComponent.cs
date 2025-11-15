using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Game.Core.UI
{
    public abstract class AWindowViewComponent : AViewComponent
    {
        public bool IsOpen { get; private set; }
        public bool IsTopWindow { get; private set; }

        public event Action<AWindowViewComponent> OnWindowOpened;
        public event Action<AWindowViewComponent> OnWindowClosed;
        public event Action<AWindowViewComponent, bool> OnTopWindowChanged;

        private IWindowManager m_windowManager;

        [Inject]
        public void Construct(IWindowManager windowManager)
        {
            m_windowManager = windowManager;
            windowManager.RegisterWindowInstance(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_windowManager.UnregisterWindowInstance(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            OpenInternal();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CloseInternal();
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual async UniTask OpenAsync()
        {
            gameObject.SetActive(true);
            await UniTask.Yield();
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }

        public virtual async UniTask CloseAsync()
        {
            gameObject.SetActive(false);
            await UniTask.Yield();
        }

        public void SetAsTopWindow(bool isTop)
        {
            if (IsTopWindow == isTop)
                return;

            IsTopWindow = isTop;
            OnTopWindowStatusChanged(isTop);
            OnTopWindowChanged?.Invoke(this, isTop);
        }

        protected virtual void OnWindowOpen()
        {
        }

        protected virtual void OnWindowClose()
        {
        }

        protected virtual void OnTopWindowStatusChanged(bool isTop)
        {
        }

        private void OpenInternal()
        {
            if (IsOpen)
                return;

            IsOpen = true;
            m_windowManager?.OpenWindow(this);
            OnWindowOpen();
            OnWindowOpened?.Invoke(this);
        }

        private void CloseInternal()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            m_windowManager?.CloseWindow(this);
            OnWindowClose();
            OnWindowClosed?.Invoke(this);
        }
    }
}
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Features.Sessions.Attributes;

namespace Game.Core.UI
{
    [AutoRegister]
    public class WindowManager : IWindowManager
    {
        private readonly List<AWindowViewComponent> m_windowInstances = new();
        private readonly List<AWindowViewComponent> m_openWindows = new();
        private readonly Stack<AWindowViewComponent> m_windowStack = new();

        public IReadOnlyList<AWindowViewComponent> OpenWindows => m_openWindows;
        public AWindowViewComponent TopWindow => m_windowStack.Count > 0 ? m_windowStack.Peek() : null;

        public void RegisterWindowInstance(AWindowViewComponent window)
        {
            if (m_windowInstances.Contains(window))
                return;

            m_windowInstances.Add(window);
        }

        public void UnregisterWindowInstance(AWindowViewComponent window)
        {
            m_windowInstances.Remove(window);
        }

        public void OpenWindow(AWindowViewComponent window)
        {
            if (m_openWindows.Contains(window))
                return;

            m_openWindows.Add(window);
            m_windowStack.Push(window);
            OnWindowStackChanged();
        }

        public void CloseWindow(AWindowViewComponent window)
        {
            m_openWindows.Remove(window);

            if (m_windowStack.Count > 0 && m_windowStack.Peek() == window)
            {
                m_windowStack.Pop();
            }
            else
            {
                var tempStack = new Stack<AWindowViewComponent>();
                while (m_windowStack.Count > 0)
                {
                    var w = m_windowStack.Pop();
                    if (w != window)
                        tempStack.Push(w);
                }

                while (tempStack.Count > 0)
                {
                    m_windowStack.Push(tempStack.Pop());
                }
            }

            OnWindowStackChanged();
        }

        public void CloseTopWindow()
        {
            if (m_windowStack.Count <= 0)
                return;

            var topWindow = m_windowStack.Peek();
            topWindow.Close();
        }

        public void CloseAllWindows()
        {
            var windowsCopy = new List<AWindowViewComponent>(m_openWindows);
            foreach (var window in windowsCopy)
            {
                window.Close();
            }
        }

        public async UniTask CloseAllWindowsAsync()
        {
            var windowsCopy = new List<AWindowViewComponent>(m_openWindows);
            foreach (var window in windowsCopy)
            {
                await window.CloseAsync();
            }
        }

        public T GetWindow<T>() where T : AWindowViewComponent
        {
            foreach (var window in m_openWindows)
            {
                if (window is T typedWindow)
                    return typedWindow;
            }

            return null;
        }

        public T GetWindowInstance<T>() where T : AWindowViewComponent
        {
            foreach (var window in m_windowInstances)
            {
                if (window is T typedWindow)
                    return typedWindow;
            }

            return null;
        }

        public bool IsWindowOpen<T>() where T : AWindowViewComponent
        {
            return GetWindow<T>() != null;
        }

        private void OnWindowStackChanged()
        {
            for (var i = 0; i < m_openWindows.Count; i++)
            {
                var isTop = m_windowStack.Count > 0 && m_openWindows[i] == m_windowStack.Peek();
                m_openWindows[i].SetAsTopWindow(isTop);
            }
        }
    }
}
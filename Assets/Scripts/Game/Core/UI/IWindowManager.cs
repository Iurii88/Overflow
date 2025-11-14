using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Game.Core.UI
{
    public interface IWindowManager
    {
        IReadOnlyList<AWindowViewComponent> OpenWindows { get; }
        AWindowViewComponent TopWindow { get; }

        void RegisterWindowInstance(AWindowViewComponent window);
        void UnregisterWindowInstance(AWindowViewComponent window);
        void RegisterWindow(AWindowViewComponent window);
        void UnregisterWindow(AWindowViewComponent window);
        void CloseTopWindow();
        void CloseAllWindows();
        UniTask CloseAllWindowsAsync();
        T GetWindow<T>() where T : AWindowViewComponent;
        T GetWindowInstance<T>() where T : AWindowViewComponent;
        bool IsWindowOpen<T>() where T : AWindowViewComponent;
    }
}

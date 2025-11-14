using Game.Core.Input.Content;

namespace Game.Core.Input
{
    public interface IInputModeManager
    {
        ContentInputMode CurrentMode { get; }
        void PushMode(string modeId);
        void PopMode();
    }
}
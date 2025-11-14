using Game.Core.Extensions;

namespace Game.Features.Pause.Extensions
{
    public interface IGamePausedExtension : IExtension
    {
        void OnGamePaused();
    }
}

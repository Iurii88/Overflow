using Game.Core.Extensions;

namespace Game.Features.Pause.Extensions
{
    public interface IGameResumedExtension : IExtension
    {
        void OnGameResumed();
    }
}
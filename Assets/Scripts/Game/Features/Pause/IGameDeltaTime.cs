namespace Game.Features.Pause
{
    public interface IGameDeltaTime
    {
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
    }
}

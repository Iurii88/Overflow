namespace Game.Features.Pause
{
    public interface IGameTime
    {
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
    }
}

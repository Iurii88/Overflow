namespace Game.Features.Pause
{
    public interface ISessionTime
    {
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
        float UnscaledDeltaTime { get; }
        float UnscaledFixedDeltaTime { get; }

        float ElapsedTime { get; }
        float UnscaledElapsedTime { get; }

        float TimeScale { get; set; }

        int FrameCount { get; }
    }
}

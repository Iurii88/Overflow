namespace Game.Features.Pause
{
    public interface ISessionTime
    {
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
        float ElapsedTime { get; }
        int FrameCount { get; }

        float TimeScale { get; set; }
        float UnscaledDeltaTime { get; }
        float UnscaledFixedDeltaTime { get; }
        float UnscaledElapsedTime { get; }

        void Update();
    }
}
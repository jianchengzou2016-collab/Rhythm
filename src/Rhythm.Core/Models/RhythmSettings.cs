namespace Rhythm.Core.Models;

public sealed record RhythmSettings(int WorkIntervalMinutes, int RestDurationSeconds)
{
    public static RhythmSettings Default { get; } = new(50, 600);

    public TimeSpan WorkInterval => TimeSpan.FromMinutes(WorkIntervalMinutes);

    public TimeSpan RestDuration => TimeSpan.FromSeconds(RestDurationSeconds);

    public RhythmSettings Normalize()
    {
        return new RhythmSettings(
            Math.Clamp(WorkIntervalMinutes, 1, 240),
            Math.Clamp(RestDurationSeconds, 5, 3600));
    }
}

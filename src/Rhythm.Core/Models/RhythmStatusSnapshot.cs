namespace Rhythm.Core.Models;

public sealed record RhythmStatusSnapshot(
    RhythmState State,
    bool IsLocked,
    RhythmSettings Settings,
    TimeSpan WorkElapsed,
    TimeSpan WorkRemaining,
    TimeSpan RestElapsed,
    TimeSpan RestRemaining,
    DateTimeOffset WorkCycleStartedAt,
    DateTimeOffset? CurrentRestStartedAt);

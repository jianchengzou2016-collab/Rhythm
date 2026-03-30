namespace Rhythm.Core.Models;

public sealed class RestSessionRecord
{
    public long Id { get; set; }

    public DateTimeOffset ScheduledAt { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset EndedAt { get; set; }

    public int PlannedRestSeconds { get; set; }

    public int ActualRestSeconds { get; set; }

    public RestSessionResult Result { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

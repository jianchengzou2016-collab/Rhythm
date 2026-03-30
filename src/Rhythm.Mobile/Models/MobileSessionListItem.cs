namespace Rhythm.Mobile.Models;

public sealed record MobileSessionListItem(
    string StartedAt,
    string ResultCode,
    int PlannedRestSeconds,
    int ActualRestSeconds);

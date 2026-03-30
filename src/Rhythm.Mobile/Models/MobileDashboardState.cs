using Rhythm.Core.Models;

namespace Rhythm.Mobile.Models;

public sealed record MobileDashboardState(
    RhythmStatusSnapshot Status,
    int TodayBreaks,
    int TodayCompletedBreaks,
    int AverageActualRestSeconds,
    IReadOnlyList<MobileSessionListItem> RecentSessions);

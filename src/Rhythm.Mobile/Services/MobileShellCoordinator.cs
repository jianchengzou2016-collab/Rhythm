using Rhythm.Core.Models;
using Rhythm.Core.Services;
using Rhythm.Mobile.Models;

namespace Rhythm.Mobile.Services;

public sealed class MobileShellCoordinator
{
    private readonly RhythmEngine _engine;

    public MobileShellCoordinator(RhythmEngine engine)
    {
        _engine = engine;
    }

    public RhythmStatusSnapshot CurrentStatus => _engine.CurrentStatus;

    public RhythmSettings CurrentSettings => _engine.CurrentSettings;

    public void Initialize()
    {
        _engine.Initialize();
    }

    public void Tick()
    {
        _engine.Tick();
    }

    public void SaveSettings(int workIntervalMinutes, int restDurationSeconds, string languageCode)
    {
        _engine.UpdateSettings(new RhythmSettings(workIntervalMinutes, restDurationSeconds, languageCode));
    }

    public void SkipCurrentRest()
    {
        _engine.SkipCurrentRest();
    }

    public void HandleSessionLocked()
    {
        _engine.HandleSessionLocked();
    }

    public void HandleSessionUnlocked()
    {
        _engine.HandleSessionUnlocked();
    }

    public MobileDashboardState BuildDashboardState(int recentSessionLimit = 12)
    {
        var todaySessions = _engine.GetTodaySessions();
        var recentSessions = _engine.GetRecentSessions(recentSessionLimit)
            .Select(session => new MobileSessionListItem(
                session.StartedAt.LocalDateTime.ToString("MM-dd HH:mm:ss"),
                session.Result.ToString(),
                session.PlannedRestSeconds,
                session.ActualRestSeconds))
            .ToList();

        var averageActualRestSeconds = todaySessions.Count == 0
            ? 0
            : (int)Math.Round(todaySessions.Average(session => session.ActualRestSeconds));

        return new MobileDashboardState(
            _engine.CurrentStatus,
            todaySessions.Count,
            todaySessions.Count(session => session.Result == RestSessionResult.Completed),
            averageActualRestSeconds,
            recentSessions);
    }
}

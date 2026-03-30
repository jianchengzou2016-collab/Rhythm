using Rhythm.Core.Abstractions;
using Rhythm.Core.Events;
using Rhythm.Core.Models;

namespace Rhythm.Core.Services;

public sealed class RhythmEngine
{
    private readonly IRhythmSettingsStore _settingsStore;
    private readonly IRestSessionRepository _sessionRepository;

    private RhythmSettings _settings = RhythmSettings.Default;
    private RhythmState _state = RhythmState.Working;
    private DateTimeOffset _workCycleStartedAt;
    private RestSessionRecord? _activeSession;
    private bool _isLocked;

    public RhythmEngine(IRhythmSettingsStore settingsStore, IRestSessionRepository sessionRepository)
    {
        _settingsStore = settingsStore;
        _sessionRepository = sessionRepository;
    }

    public event Action<RhythmStatusSnapshot>? StatusChanged;

    public event EventHandler? BreakStarted;

    public event EventHandler<RestSessionRecordedEventArgs>? BreakEnded;

    public RhythmSettings CurrentSettings => _settings;

    public RhythmStatusSnapshot CurrentStatus => BuildSnapshot(DateTimeOffset.Now);

    public void Initialize()
    {
        _settings = _settingsStore.LoadSettings().Normalize();
        _workCycleStartedAt = DateTimeOffset.Now;
        PublishStatus(DateTimeOffset.Now);
    }

    public void UpdateSettings(RhythmSettings settings)
    {
        _settings = settings.Normalize();
        _settingsStore.SaveSettings(_settings);

        if (_state == RhythmState.Working && !_isLocked)
        {
            _workCycleStartedAt = DateTimeOffset.Now;
        }

        PublishStatus(DateTimeOffset.Now);
    }

    public void Tick()
    {
        var now = DateTimeOffset.Now;

        if (_isLocked)
        {
            PublishStatus(now);
            return;
        }

        if (_state == RhythmState.Working)
        {
            if (now - _workCycleStartedAt >= _settings.WorkInterval)
            {
                StartRest(now);
                return;
            }
        }
        else if (_activeSession is not null && now - _activeSession.StartedAt >= _settings.RestDuration)
        {
            FinishRest(RestSessionResult.Completed, null, now);
            return;
        }

        PublishStatus(now);
    }

    public void SkipCurrentRest()
    {
        if (_state != RhythmState.Resting || _activeSession is null)
        {
            return;
        }

        FinishRest(RestSessionResult.Skipped, "esc", DateTimeOffset.Now);
    }

    public void HandleSessionLocked()
    {
        var now = DateTimeOffset.Now;

        if (_state == RhythmState.Resting && _activeSession is not null)
        {
            FinishRest(RestSessionResult.InterruptedByLock, null, now);
        }

        _isLocked = true;
        _state = RhythmState.Working;
        _workCycleStartedAt = now;
        PublishStatus(now);
    }

    public void HandleSessionUnlocked()
    {
        _isLocked = false;
        _state = RhythmState.Working;
        _workCycleStartedAt = DateTimeOffset.Now;
        PublishStatus(DateTimeOffset.Now);
    }

    public IReadOnlyList<RestSessionRecord> GetRecentSessions(int limit)
    {
        return _sessionRepository.GetRecentSessions(limit);
    }

    public IReadOnlyList<RestSessionRecord> GetTodaySessions()
    {
        var localNow = DateTimeOffset.Now;
        var startOfDay = new DateTimeOffset(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0, localNow.Offset);
        return _sessionRepository.GetSessionsSince(startOfDay);
    }

    private void StartRest(DateTimeOffset now)
    {
        _state = RhythmState.Resting;
        _activeSession = new RestSessionRecord
        {
            ScheduledAt = _workCycleStartedAt.Add(_settings.WorkInterval),
            StartedAt = now,
            PlannedRestSeconds = _settings.RestDurationSeconds,
            CreatedAt = now
        };

        BreakStarted?.Invoke(this, EventArgs.Empty);
        PublishStatus(now);
    }

    private void FinishRest(RestSessionResult result, string? skipReason, DateTimeOffset now)
    {
        if (_activeSession is null)
        {
            return;
        }

        _activeSession.EndedAt = now;
        _activeSession.ActualRestSeconds = Math.Max(0, (int)Math.Floor((now - _activeSession.StartedAt).TotalSeconds));
        _activeSession.Result = result;
        _activeSession.SkipReason = skipReason;

        _sessionRepository.SaveSession(_activeSession);

        var recordedSession = _activeSession;
        _activeSession = null;
        _state = RhythmState.Working;
        _workCycleStartedAt = now;

        BreakEnded?.Invoke(this, new RestSessionRecordedEventArgs(recordedSession));
        PublishStatus(now);
    }

    private RhythmStatusSnapshot BuildSnapshot(DateTimeOffset now)
    {
        if (_isLocked)
        {
            return new RhythmStatusSnapshot(
                RhythmState.Working,
                true,
                _settings,
                TimeSpan.Zero,
                _settings.WorkInterval,
                TimeSpan.Zero,
                TimeSpan.Zero,
                _workCycleStartedAt,
                null);
        }

        if (_state == RhythmState.Resting && _activeSession is not null)
        {
            var restElapsed = ClampPositive(now - _activeSession.StartedAt);
            var restRemaining = ClampPositive(_settings.RestDuration - restElapsed);

            return new RhythmStatusSnapshot(
                RhythmState.Resting,
                false,
                _settings,
                TimeSpan.Zero,
                TimeSpan.Zero,
                restElapsed,
                restRemaining,
                _workCycleStartedAt,
                _activeSession.StartedAt);
        }

        var workElapsed = ClampPositive(now - _workCycleStartedAt);
        var workRemaining = ClampPositive(_settings.WorkInterval - workElapsed);

        return new RhythmStatusSnapshot(
            RhythmState.Working,
            false,
            _settings,
            workElapsed,
            workRemaining,
            TimeSpan.Zero,
            TimeSpan.Zero,
            _workCycleStartedAt,
            null);
    }

    private void PublishStatus(DateTimeOffset now)
    {
        StatusChanged?.Invoke(BuildSnapshot(now));
    }

    private static TimeSpan ClampPositive(TimeSpan value)
    {
        return value < TimeSpan.Zero ? TimeSpan.Zero : value;
    }
}

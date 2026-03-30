using Microsoft.Data.Sqlite;
using Rhythm.Core.Abstractions;
using Rhythm.Core.Models;
using Rhythm.Core.Services;
using Rhythm.Infrastructure;
using Rhythm.Mobile.Services;

namespace Rhythm.Tests;

public sealed class UnitTest1
{
    [Fact]
    public void Tick_WhenWorkIntervalReached_StartsRestingCountdown()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 3, 30, 9, 0, 0, TimeSpan.Zero));
        var repository = new InMemoryRepository(new RhythmSettings(1, 30));
        var engine = new RhythmEngine(repository, repository, clock);

        engine.Initialize();
        clock.Advance(TimeSpan.FromSeconds(61));
        engine.Tick();

        var status = engine.CurrentStatus;

        Assert.Equal(RhythmState.Resting, status.State);
        Assert.Equal(30, (int)Math.Ceiling(status.RestRemaining.TotalSeconds));
    }

    [Fact]
    public void Tick_WhenRestDurationElapsed_CompletesAndPersistsSession()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 3, 30, 9, 0, 0, TimeSpan.Zero));
        var repository = new InMemoryRepository(new RhythmSettings(1, 30));
        var engine = new RhythmEngine(repository, repository, clock);

        engine.Initialize();
        clock.Advance(TimeSpan.FromSeconds(61));
        engine.Tick();
        clock.Advance(TimeSpan.FromSeconds(30));
        engine.Tick();

        Assert.Single(repository.Sessions);
        Assert.Equal(RestSessionResult.Completed, repository.Sessions[0].Result);
        Assert.Equal(30, repository.Sessions[0].ActualRestSeconds);
        Assert.Equal(RhythmState.Working, engine.CurrentStatus.State);
    }

    [Fact]
    public void SkipCurrentRest_RecordsEscSkip()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 3, 30, 9, 0, 0, TimeSpan.Zero));
        var repository = new InMemoryRepository(new RhythmSettings(1, 30));
        var engine = new RhythmEngine(repository, repository, clock);

        engine.Initialize();
        clock.Advance(TimeSpan.FromSeconds(61));
        engine.Tick();
        clock.Advance(TimeSpan.FromSeconds(7));
        engine.SkipCurrentRest();

        Assert.Single(repository.Sessions);
        Assert.Equal(RestSessionResult.Skipped, repository.Sessions[0].Result);
        Assert.Equal("esc", repository.Sessions[0].SkipReason);
        Assert.Equal(7, repository.Sessions[0].ActualRestSeconds);
    }

    [Fact]
    public void HandleSessionLocked_DuringRest_InterruptsAndResets()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 3, 30, 9, 0, 0, TimeSpan.Zero));
        var repository = new InMemoryRepository(new RhythmSettings(1, 30));
        var engine = new RhythmEngine(repository, repository, clock);

        engine.Initialize();
        clock.Advance(TimeSpan.FromSeconds(61));
        engine.Tick();
        clock.Advance(TimeSpan.FromSeconds(9));
        engine.HandleSessionLocked();

        Assert.Single(repository.Sessions);
        Assert.Equal(RestSessionResult.InterruptedByLock, repository.Sessions[0].Result);
        Assert.True(engine.CurrentStatus.IsLocked);

        clock.Advance(TimeSpan.FromSeconds(10));
        engine.HandleSessionUnlocked();
        Assert.False(engine.CurrentStatus.IsLocked);
        Assert.Equal(RhythmState.Working, engine.CurrentStatus.State);
    }

    [Fact]
    public void Database_CanRoundTripSettingsAndSessions()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"rhythm-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var database = new RhythmDatabase(Path.Combine(tempDirectory, "rhythm.db"));
            var settings = new RhythmSettings(45, 120, "en-US");
            database.SaveSettings(settings);

            database.SaveSession(new RestSessionRecord
            {
                ScheduledAt = new DateTimeOffset(2026, 3, 30, 9, 45, 0, TimeSpan.Zero),
                StartedAt = new DateTimeOffset(2026, 3, 30, 9, 45, 0, TimeSpan.Zero),
                EndedAt = new DateTimeOffset(2026, 3, 30, 9, 46, 30, TimeSpan.Zero),
                PlannedRestSeconds = 120,
                ActualRestSeconds = 90,
                Result = RestSessionResult.Completed,
                CreatedAt = new DateTimeOffset(2026, 3, 30, 9, 45, 0, TimeSpan.Zero)
            });

            var loadedSettings = database.LoadSettings();
            var sessions = database.GetRecentSessions(10);

            Assert.Equal(45, loadedSettings.WorkIntervalMinutes);
            Assert.Equal(120, loadedSettings.RestDurationSeconds);
            Assert.Equal("en-US", loadedSettings.LanguageCode);
            Assert.Single(sessions);
            Assert.Equal(90, sessions[0].ActualRestSeconds);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public void Normalize_WhenLanguageIsUnsupported_FallsBackToChinese()
    {
        var settings = new RhythmSettings(30, 120, "fr-FR");

        var normalized = settings.Normalize();

        Assert.Equal("zh-CN", normalized.LanguageCode);
    }

    [Fact]
    public void MobileShellCoordinator_BuildDashboardState_ReturnsTodaySummary()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 3, 30, 9, 0, 0, TimeSpan.Zero));
        var repository = new InMemoryRepository(new RhythmSettings(1, 30, "en-US"));
        var engine = new RhythmEngine(repository, repository, clock);
        var coordinator = new MobileShellCoordinator(engine);

        coordinator.Initialize();
        clock.Advance(TimeSpan.FromSeconds(61));
        coordinator.Tick();
        clock.Advance(TimeSpan.FromSeconds(30));
        coordinator.Tick();

        var dashboard = coordinator.BuildDashboardState();

        Assert.Equal(1, dashboard.TodayBreaks);
        Assert.Equal(1, dashboard.TodayCompletedBreaks);
        Assert.Equal(30, dashboard.AverageActualRestSeconds);
        Assert.Single(dashboard.RecentSessions);
        Assert.Equal(RestSessionResult.Completed.ToString(), dashboard.RecentSessions[0].ResultCode);
    }

    private sealed class FakeClock : IClock
    {
        public FakeClock(DateTimeOffset now)
        {
            Now = now;
        }

        public DateTimeOffset Now { get; private set; }

        public void Advance(TimeSpan timeSpan)
        {
            Now = Now.Add(timeSpan);
        }
    }

    private sealed class InMemoryRepository : IRhythmSettingsStore, IRestSessionRepository
    {
        private RhythmSettings _settings;

        public InMemoryRepository(RhythmSettings settings)
        {
            _settings = settings;
        }

        public List<RestSessionRecord> Sessions { get; } = [];

        public IReadOnlyList<RestSessionRecord> GetRecentSessions(int limit)
        {
            return Sessions.Take(limit).ToList();
        }

        public IReadOnlyList<RestSessionRecord> GetSessionsSince(DateTimeOffset since)
        {
            return Sessions.Where(session => session.StartedAt >= since).ToList();
        }

        public RhythmSettings LoadSettings()
        {
            return _settings;
        }

        public void SaveSession(RestSessionRecord session)
        {
            Sessions.Add(new RestSessionRecord
            {
                Id = session.Id,
                ScheduledAt = session.ScheduledAt,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                PlannedRestSeconds = session.PlannedRestSeconds,
                ActualRestSeconds = session.ActualRestSeconds,
                Result = session.Result,
                SkipReason = session.SkipReason,
                CreatedAt = session.CreatedAt
            });
        }

        public void SaveSettings(RhythmSettings settings)
        {
            _settings = settings;
        }
    }
}

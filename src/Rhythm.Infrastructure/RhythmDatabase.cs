using System.Globalization;
using Microsoft.Data.Sqlite;
using Rhythm.Core.Abstractions;
using Rhythm.Core.Models;

namespace Rhythm.Infrastructure;

public sealed class RhythmDatabase : IRhythmSettingsStore, IRestSessionRepository
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public RhythmDatabase(string databasePath)
    {
        _databasePath = databasePath;
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
        _connectionString = $"Data Source={_databasePath};Pooling=False";
        Initialize();
    }

    public RhythmSettings LoadSettings()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT work_interval_minutes, rest_duration_seconds, language_code
            FROM settings
            WHERE id = 1;
            """;

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return RhythmSettings.Default;
        }

        return new RhythmSettings(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2)).Normalize();
    }

    public void SaveSettings(RhythmSettings settings)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO settings (id, work_interval_minutes, rest_duration_seconds, language_code, updated_at)
            VALUES (1, $workIntervalMinutes, $restDurationSeconds, $languageCode, $updatedAt)
            ON CONFLICT(id) DO UPDATE SET
                work_interval_minutes = excluded.work_interval_minutes,
                rest_duration_seconds = excluded.rest_duration_seconds,
                language_code = excluded.language_code,
                updated_at = excluded.updated_at;
            """;
        command.Parameters.AddWithValue("$workIntervalMinutes", settings.WorkIntervalMinutes);
        command.Parameters.AddWithValue("$restDurationSeconds", settings.RestDurationSeconds);
        command.Parameters.AddWithValue("$languageCode", settings.LanguageCode);
        command.Parameters.AddWithValue("$updatedAt", DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture));
        command.ExecuteNonQuery();
    }

    public void SaveSession(RestSessionRecord session)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO rest_sessions (
                scheduled_at,
                started_at,
                ended_at,
                planned_rest_seconds,
                actual_rest_seconds,
                result,
                skip_reason,
                created_at)
            VALUES (
                $scheduledAt,
                $startedAt,
                $endedAt,
                $plannedRestSeconds,
                $actualRestSeconds,
                $result,
                $skipReason,
                $createdAt);
            """;
        command.Parameters.AddWithValue("$scheduledAt", session.ScheduledAt.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$startedAt", session.StartedAt.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$endedAt", session.EndedAt.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$plannedRestSeconds", session.PlannedRestSeconds);
        command.Parameters.AddWithValue("$actualRestSeconds", session.ActualRestSeconds);
        command.Parameters.AddWithValue("$result", session.Result.ToString());
        command.Parameters.AddWithValue("$skipReason", session.SkipReason ?? string.Empty);
        command.Parameters.AddWithValue("$createdAt", session.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<RestSessionRecord> GetRecentSessions(int limit)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, scheduled_at, started_at, ended_at, planned_rest_seconds, actual_rest_seconds, result, skip_reason, created_at
            FROM rest_sessions
            ORDER BY started_at DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limit);
        return ReadSessions(command);
    }

    public IReadOnlyList<RestSessionRecord> GetSessionsSince(DateTimeOffset since)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, scheduled_at, started_at, ended_at, planned_rest_seconds, actual_rest_seconds, result, skip_reason, created_at
            FROM rest_sessions
            WHERE started_at >= $since
            ORDER BY started_at DESC;
            """;
        command.Parameters.AddWithValue("$since", since.ToString("O", CultureInfo.InvariantCulture));
        return ReadSessions(command);
    }

    private void Initialize()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS settings (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                work_interval_minutes INTEGER NOT NULL,
                rest_duration_seconds INTEGER NOT NULL,
                language_code TEXT NOT NULL DEFAULT 'zh-CN',
                updated_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS rest_sessions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                scheduled_at TEXT NOT NULL,
                started_at TEXT NOT NULL,
                ended_at TEXT NOT NULL,
                planned_rest_seconds INTEGER NOT NULL,
                actual_rest_seconds INTEGER NOT NULL,
                result TEXT NOT NULL,
                skip_reason TEXT NOT NULL,
                created_at TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
        EnsureSettingsLanguageColumn(connection);
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static void EnsureSettingsLanguageColumn(SqliteConnection connection)
    {
        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "PRAGMA table_info(settings);";
        using var reader = checkCommand.ExecuteReader();

        var hasLanguageColumn = false;
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), "language_code", StringComparison.OrdinalIgnoreCase))
            {
                hasLanguageColumn = true;
                break;
            }
        }

        if (hasLanguageColumn)
        {
            return;
        }

        using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = "ALTER TABLE settings ADD COLUMN language_code TEXT NOT NULL DEFAULT 'zh-CN';";
        alterCommand.ExecuteNonQuery();
    }

    private static IReadOnlyList<RestSessionRecord> ReadSessions(SqliteCommand command)
    {
        var sessions = new List<RestSessionRecord>();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            sessions.Add(new RestSessionRecord
            {
                Id = reader.GetInt64(0),
                ScheduledAt = ParseDateTimeOffset(reader.GetString(1)),
                StartedAt = ParseDateTimeOffset(reader.GetString(2)),
                EndedAt = ParseDateTimeOffset(reader.GetString(3)),
                PlannedRestSeconds = reader.GetInt32(4),
                ActualRestSeconds = reader.GetInt32(5),
                Result = Enum.Parse<RestSessionResult>(reader.GetString(6)),
                SkipReason = string.IsNullOrWhiteSpace(reader.GetString(7)) ? null : reader.GetString(7),
                CreatedAt = ParseDateTimeOffset(reader.GetString(8))
            });
        }

        return sessions;
    }

    private static DateTimeOffset ParseDateTimeOffset(string value)
    {
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}

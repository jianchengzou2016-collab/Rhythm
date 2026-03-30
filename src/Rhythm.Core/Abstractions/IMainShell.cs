using Rhythm.Core.Models;

namespace Rhythm.Core.Abstractions;

public interface IMainShell
{
    void InitializeShell();

    void ShowShell();

    void HideShell();

    void RestoreAndActivateShell();

    void CloseShell();

    void UpdateStatus(RhythmStatusSnapshot snapshot);

    void UpdateSettings(RhythmSettings settings);

    void ApplyLocalization(string languageCode, RhythmStatusSnapshot snapshot);

    void UpdateHistory(IReadOnlyList<RestSessionRecord> recentSessions, IReadOnlyList<RestSessionRecord> todaySessions, string languageCode);
}

namespace Rhythm.Core.Abstractions;

public interface IMainShellCoordinator
{
    bool IsExiting { get; }

    bool SaveSettings(int workIntervalMinutes, int restDurationSeconds, string languageCode, out string errorMessage);

    void HideMainShell();
}

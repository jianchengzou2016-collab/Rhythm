using Rhythm.Core.Models;

namespace Rhythm.Core.Abstractions;

public interface ITrayService : IDisposable
{
    event EventHandler? OpenRequested;

    event EventHandler? SkipRequested;

    event EventHandler? ExitRequested;

    void Show();

    void Update(RhythmStatusSnapshot snapshot);

    void Hide();
}

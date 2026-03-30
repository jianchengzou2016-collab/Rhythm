using Rhythm.Core.Models;

namespace Rhythm.Core.Abstractions;

public interface IBreakOverlayPresenter : IDisposable
{
    event EventHandler? SkipRequested;

    void Show(RhythmStatusSnapshot snapshot);

    void Update(RhythmStatusSnapshot snapshot);

    void Close();
}

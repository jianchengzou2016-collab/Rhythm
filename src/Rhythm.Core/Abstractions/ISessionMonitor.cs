namespace Rhythm.Core.Abstractions;

public interface ISessionMonitor : IDisposable
{
    event EventHandler? SessionLocked;

    event EventHandler? SessionUnlocked;
}

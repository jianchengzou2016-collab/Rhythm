using Microsoft.Win32;
using Rhythm.Core.Abstractions;

namespace Rhythm.App.Services;

public sealed class WindowsSessionMonitor : ISessionMonitor
{
    public WindowsSessionMonitor()
    {
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    public event EventHandler? SessionLocked;

    public event EventHandler? SessionUnlocked;

    public void Dispose()
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionLock)
        {
            SessionLocked?.Invoke(this, EventArgs.Empty);
        }
        else if (e.Reason == SessionSwitchReason.SessionUnlock)
        {
            SessionUnlocked?.Invoke(this, EventArgs.Empty);
        }
    }
}

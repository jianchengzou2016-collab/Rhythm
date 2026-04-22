using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;
using Rhythm.Core.Abstractions;
using Rhythm.Core.Models;
using WpfApplication = System.Windows.Application;
using WpfDispatcherPriority = System.Windows.Threading.DispatcherPriority;

namespace Rhythm.App.Services;

public sealed class WindowsBreakOverlayPresenter : IBreakOverlayPresenter
{
    private readonly List<RestOverlayWindow> _overlayWindows = [];
    private string? _currentLanguageCode;

    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);

    public WindowsBreakOverlayPresenter()
    {
    }

    public event EventHandler? SkipRequested;

    public void Show(RhythmStatusSnapshot snapshot)
    {
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
            CloseCore();

            foreach (var screen in Forms.Screen.AllScreens)
            {
                var overlayWindow = new RestOverlayWindow(OnSkipRequested, screen);
                overlayWindow.ApplyLocalization(snapshot.Settings.LanguageCode);
                overlayWindow.UpdateCountdown(snapshot.RestRemaining);
                overlayWindow.Show();
                _overlayWindows.Add(overlayWindow);
            }

            _currentLanguageCode = snapshot.Settings.LanguageCode;
            _overlayWindows.FirstOrDefault()?.Activate();
        });
    }

    public void Update(RhythmStatusSnapshot snapshot)
    {
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
            if (_overlayWindows.Count == 0)
            {
                return;
            }

            foreach (var overlayWindow in _overlayWindows)
            {
                if (!string.Equals(_currentLanguageCode, snapshot.Settings.LanguageCode, StringComparison.Ordinal))
                {
                    overlayWindow.ApplyLocalization(snapshot.Settings.LanguageCode);
                }

                overlayWindow.UpdateCountdown(snapshot.RestRemaining);
            }

            _currentLanguageCode = snapshot.Settings.LanguageCode;
        });
    }

    public void Close()
    {
        WpfApplication.Current.Dispatcher.Invoke(CloseCore);
    }

    public void Dispose()
    {
        Close();
    }

    private void OnSkipRequested()
    {
        SkipRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CloseCore()
    {
        foreach (var overlayWindow in _overlayWindows)
        {
            overlayWindow.Close();
        }

        _overlayWindows.Clear();
        _currentLanguageCode = null;

        WpfApplication.Current.Dispatcher.BeginInvoke(WpfDispatcherPriority.ApplicationIdle, new Action(ReleaseOverlayMemory));
    }

    private static void ReleaseOverlayMemory()
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        using var process = Process.GetCurrentProcess();
        EmptyWorkingSet(process.Handle);
    }
}

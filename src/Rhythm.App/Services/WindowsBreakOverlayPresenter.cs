using Forms = System.Windows.Forms;
using Rhythm.Core.Abstractions;
using Rhythm.Core.Models;
using WpfApplication = System.Windows.Application;

namespace Rhythm.App.Services;

public sealed class WindowsBreakOverlayPresenter : IBreakOverlayPresenter
{
    private readonly List<RestOverlayWindow> _overlayWindows = [];

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
                overlayWindow.ApplyLocalization(snapshot.Settings.LanguageCode);
                overlayWindow.UpdateCountdown(snapshot.RestRemaining);
            }
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
    }
}

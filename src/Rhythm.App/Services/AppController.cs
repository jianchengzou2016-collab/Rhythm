using System.Windows;
using System.Windows.Threading;
using Rhythm.App.Localization;
using Rhythm.Core.Abstractions;
using Rhythm.Core.Events;
using Rhythm.Core.Models;
using Rhythm.Core.Services;
using WpfApplication = System.Windows.Application;

namespace Rhythm.App.Services;

public sealed class AppController : IDisposable
    , IMainShellCoordinator
{
    private readonly RhythmEngine _engine;
    private readonly ISessionMonitor _sessionMonitor;
    private readonly ITrayService _trayService;
    private readonly IBreakOverlayPresenter _breakOverlayPresenter;
    private readonly DispatcherTimer _timer;

    private IMainShell? _mainShell;

    public AppController(
        RhythmEngine engine,
        ISessionMonitor sessionMonitor,
        ITrayService trayService,
        IBreakOverlayPresenter breakOverlayPresenter)
    {
        _engine = engine;
        _sessionMonitor = sessionMonitor;
        _trayService = trayService;
        _breakOverlayPresenter = breakOverlayPresenter;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_OnTick;
    }

    public bool IsExiting { get; private set; }

    public void Initialize()
    {
        _engine.StatusChanged += Engine_OnStatusChanged;
        _engine.BreakStarted += Engine_OnBreakStarted;
        _engine.BreakEnded += Engine_OnBreakEnded;
        _sessionMonitor.SessionLocked += SessionMonitor_OnSessionLocked;
        _sessionMonitor.SessionUnlocked += SessionMonitor_OnSessionUnlocked;
        _trayService.OpenRequested += TrayService_OnOpenRequested;
        _trayService.SkipRequested += TrayService_OnSkipRequested;
        _trayService.ExitRequested += TrayService_OnExitRequested;
        _breakOverlayPresenter.SkipRequested += BreakOverlayPresenter_OnSkipRequested;
        _engine.Initialize();
        _trayService.Show();
        _timer.Start();
    }

    public void AttachMainShell(IMainShell mainShell)
    {
        _mainShell = mainShell;
        _mainShell.UpdateSettings(_engine.CurrentSettings);
        _mainShell.ApplyLocalization(_engine.CurrentSettings.LanguageCode, _engine.CurrentStatus);
        _mainShell.UpdateStatus(_engine.CurrentStatus);
        RefreshHistory();
        _trayService.Update(_engine.CurrentStatus);
    }

    public void ShowMainWindow()
    {
        if (_mainShell is null)
        {
            return;
        }

        _mainShell.RestoreAndActivateShell();
    }

    public void HideMainShell()
    {
        _mainShell?.HideShell();
    }

    public void SkipCurrentRest()
    {
        _engine.SkipCurrentRest();
    }

    public bool SaveSettings(int workIntervalMinutes, int restDurationSeconds, string languageCode, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (workIntervalMinutes is < 1 or > 240)
        {
            errorMessage = AppText.Get(languageCode, "InvalidWorkInterval");
            return false;
        }

        if (restDurationSeconds is < 5 or > 3600)
        {
            errorMessage = AppText.Get(languageCode, "InvalidRestDuration");
            return false;
        }

        _engine.UpdateSettings(new RhythmSettings(workIntervalMinutes, restDurationSeconds, languageCode));
        _mainShell?.UpdateSettings(_engine.CurrentSettings);
        _mainShell?.ApplyLocalization(_engine.CurrentSettings.LanguageCode, _engine.CurrentStatus);
        RefreshHistory();
        _trayService.Update(_engine.CurrentStatus);
        return true;
    }

    public void Exit()
    {
        IsExiting = true;
        _timer.Stop();
        _breakOverlayPresenter.Close();
        _trayService.Hide();
        _mainShell?.CloseShell();
        WpfApplication.Current.Shutdown();
    }

    public void Dispose()
    {
        _timer.Stop();
        _trayService.Dispose();
        _breakOverlayPresenter.Dispose();
        _sessionMonitor.Dispose();
    }

    private void Timer_OnTick(object? sender, EventArgs e)
    {
        _engine.Tick();
    }

    private void Engine_OnStatusChanged(RhythmStatusSnapshot snapshot)
    {
        if (WpfApplication.Current.Dispatcher.CheckAccess())
        {
            ApplyStatus(snapshot);
            return;
        }

        WpfApplication.Current.Dispatcher.Invoke(() => ApplyStatus(snapshot));
    }

    private void Engine_OnBreakStarted(object? sender, EventArgs e)
    {
        _breakOverlayPresenter.Show(_engine.CurrentStatus);
    }

    private void Engine_OnBreakEnded(object? sender, RestSessionRecordedEventArgs e)
    {
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
            _breakOverlayPresenter.Close();
            RefreshHistory();
        });
    }

    private void SessionMonitor_OnSessionLocked(object? sender, EventArgs e)
    {
        WpfApplication.Current.Dispatcher.Invoke(_engine.HandleSessionLocked);
    }

    private void SessionMonitor_OnSessionUnlocked(object? sender, EventArgs e)
    {
        WpfApplication.Current.Dispatcher.Invoke(_engine.HandleSessionUnlocked);
    }

    private void ApplyStatus(RhythmStatusSnapshot snapshot)
    {
        _mainShell?.UpdateStatus(snapshot);

        if (snapshot.State == RhythmState.Resting)
        {
            _breakOverlayPresenter.Update(snapshot);
        }

        _trayService.Update(snapshot);
    }

    private void RefreshHistory()
    {
        _mainShell?.UpdateHistory(_engine.GetRecentSessions(12), _engine.GetTodaySessions(), _engine.CurrentSettings.LanguageCode);
    }

    private void TrayService_OnOpenRequested(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void TrayService_OnSkipRequested(object? sender, EventArgs e)
    {
        SkipCurrentRest();
    }

    private void TrayService_OnExitRequested(object? sender, EventArgs e)
    {
        Exit();
    }

    private void BreakOverlayPresenter_OnSkipRequested(object? sender, EventArgs e)
    {
        SkipCurrentRest();
    }
}

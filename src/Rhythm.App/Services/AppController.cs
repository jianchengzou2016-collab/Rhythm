using System.Windows;
using System.Windows.Threading;
using Rhythm.Core.Events;
using Rhythm.Core.Models;
using Rhythm.Core.Services;

namespace Rhythm.App.Services;

public sealed class AppController : IDisposable
{
    private readonly RhythmEngine _engine;
    private readonly WindowsSessionMonitor _sessionMonitor;
    private readonly DispatcherTimer _timer;

    private MainWindow? _mainWindow;
    private RestOverlayWindow? _restOverlayWindow;

    public AppController(RhythmEngine engine, WindowsSessionMonitor sessionMonitor)
    {
        _engine = engine;
        _sessionMonitor = sessionMonitor;
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
        _engine.Initialize();
        _timer.Start();
    }

    public void AttachMainWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _mainWindow.UpdateSettings(_engine.CurrentSettings);
        _mainWindow.UpdateStatus(_engine.CurrentStatus);
        RefreshHistory();
    }

    public void SkipCurrentRest()
    {
        _engine.SkipCurrentRest();
    }

    public bool SaveSettings(int workIntervalMinutes, int restDurationSeconds, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (workIntervalMinutes is < 1 or > 240)
        {
            errorMessage = "工作间隔请填写 1 到 240 分钟。";
            return false;
        }

        if (restDurationSeconds is < 5 or > 3600)
        {
            errorMessage = "休息时长请填写 5 到 3600 秒。";
            return false;
        }

        _engine.UpdateSettings(new RhythmSettings(workIntervalMinutes, restDurationSeconds));
        _mainWindow?.UpdateSettings(_engine.CurrentSettings);
        RefreshHistory();
        return true;
    }

    public void Exit()
    {
        IsExiting = true;
        _timer.Stop();
        _restOverlayWindow?.Close();
        _mainWindow?.Close();
        Application.Current.Shutdown();
    }

    public void Dispose()
    {
        _timer.Stop();
        _sessionMonitor.Dispose();
    }

    private void Timer_OnTick(object? sender, EventArgs e)
    {
        _engine.Tick();
    }

    private void Engine_OnStatusChanged(RhythmStatusSnapshot snapshot)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ApplyStatus(snapshot);
            return;
        }

        Application.Current.Dispatcher.Invoke(() => ApplyStatus(snapshot));
    }

    private void Engine_OnBreakStarted(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _restOverlayWindow ??= new RestOverlayWindow(this);
            _restOverlayWindow.UpdateCountdown(_engine.CurrentStatus.RestRemaining);
            _restOverlayWindow.Show();
            _restOverlayWindow.Activate();
        });
    }

    private void Engine_OnBreakEnded(object? sender, RestSessionRecordedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_restOverlayWindow is not null)
            {
                _restOverlayWindow.Close();
                _restOverlayWindow = null;
            }

            RefreshHistory();
        });
    }

    private void SessionMonitor_OnSessionLocked(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(_engine.HandleSessionLocked);
    }

    private void SessionMonitor_OnSessionUnlocked(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(_engine.HandleSessionUnlocked);
    }

    private void ApplyStatus(RhythmStatusSnapshot snapshot)
    {
        _mainWindow?.UpdateStatus(snapshot);
        _mainWindow?.UpdateSettings(snapshot.Settings);

        if (_restOverlayWindow is not null && snapshot.State == RhythmState.Resting)
        {
            _restOverlayWindow.UpdateCountdown(snapshot.RestRemaining);
        }
    }

    private void RefreshHistory()
    {
        _mainWindow?.UpdateHistory(_engine.GetRecentSessions(12), _engine.GetTodaySessions());
    }

}

using System.IO;
using System.Windows;
using Rhythm.App.Services;
using Rhythm.Core.Services;
using Rhythm.Infrastructure;

namespace Rhythm.App;

public partial class App
{
    internal AppController? Controller { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Rhythm");

        var database = new RhythmDatabase(Path.Combine(dataDirectory, "rhythm.db"));
        var engine = new RhythmEngine(database, database);
        var sessionMonitor = new WindowsSessionMonitor();
        var trayService = new WindowsTrayService();
        var breakOverlayPresenter = new WindowsBreakOverlayPresenter();

        Controller = new AppController(engine, sessionMonitor, trayService, breakOverlayPresenter);
        Controller.Initialize();

        base.OnStartup(e);

        var mainWindow = new MainWindow(Controller);
        mainWindow.InitializeShell();
        MainWindow = mainWindow;
        Controller.AttachMainShell(mainWindow);
        mainWindow.ShowShell();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Controller?.Dispose();
        base.OnExit(e);
    }
}

using System.Drawing;
using System.IO;
using Forms = System.Windows.Forms;
using Rhythm.App.Localization;
using Rhythm.Core.Abstractions;
using Rhythm.Core.Models;

namespace Rhythm.App.Services;

public sealed class WindowsTrayService : ITrayService
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ToolStripMenuItem _openMenuItem;
    private readonly Forms.ToolStripMenuItem _skipMenuItem;
    private readonly Forms.ToolStripMenuItem _exitMenuItem;

    public WindowsTrayService()
    {
        var contextMenu = new Forms.ContextMenuStrip();
        _openMenuItem = new Forms.ToolStripMenuItem("Rhythm");
        _skipMenuItem = new Forms.ToolStripMenuItem("Rhythm");
        _exitMenuItem = new Forms.ToolStripMenuItem("Rhythm");

        _openMenuItem.Click += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);
        _skipMenuItem.Click += (_, _) => SkipRequested?.Invoke(this, EventArgs.Empty);
        _exitMenuItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        contextMenu.Items.Add(_openMenuItem);
        contextMenu.Items.Add(_skipMenuItem);
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add(_exitMenuItem);

        var iconPath = Path.Combine(AppContext.BaseDirectory, "Rhythm.App.exe");
        var notifyIcon = File.Exists(iconPath)
            ? Icon.ExtractAssociatedIcon(iconPath)
            : SystemIcons.Application;

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = notifyIcon ?? SystemIcons.Application,
            Text = "Rhythm",
            ContextMenuStrip = contextMenu,
            Visible = false
        };
        _notifyIcon.DoubleClick += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? OpenRequested;

    public event EventHandler? SkipRequested;

    public event EventHandler? ExitRequested;

    public void Show()
    {
        _notifyIcon.Visible = true;
    }

    public void Update(RhythmStatusSnapshot snapshot)
    {
        var languageCode = snapshot.Settings.LanguageCode;
        _openMenuItem.Text = AppText.Get(languageCode, "TrayOpen");
        _skipMenuItem.Text = AppText.Get(languageCode, "TraySkip");
        _exitMenuItem.Text = AppText.Get(languageCode, "TrayExit");
        _notifyIcon.Text = AppText.Get(languageCode, "AppName");
    }

    public void Hide()
    {
        _notifyIcon.Visible = false;
    }

    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}

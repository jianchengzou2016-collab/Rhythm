using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Rhythm.App.Models;
using Rhythm.App.Services;
using Rhythm.Core.Models;

namespace Rhythm.App;

public partial class MainWindow
{
    private readonly ObservableCollection<HistoryListItem> _historyItems = [];
    private TextBox? _workIntervalTextBox;
    private TextBox? _restDurationTextBox;
    private TextBlock? _stateTextBlock;
    private TextBlock? _statusDescriptionTextBlock;
    private TextBlock? _lockStateTextBlock;
    private TextBlock? _todayTotalTextBlock;
    private TextBlock? _todayCompletedTextBlock;
    private TextBlock? _todayAverageRestTextBlock;

    private AppController Controller => ((App)Application.Current).Controller!;

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        Title = "Rhythm";
        Width = 1120;
        Height = 760;
        MinWidth = 960;
        MinHeight = 680;
        Background = BrushFromHex("#FFF6F1E8");
        FontFamily = new FontFamily("Bahnschrift");
        Closing += Window_OnClosing;
        Content = BuildLayout();
    }

    public void UpdateStatus(RhythmStatusSnapshot snapshot)
    {
        if (_stateTextBlock is null || _statusDescriptionTextBlock is null || _lockStateTextBlock is null)
        {
            return;
        }

        _stateTextBlock.Text = snapshot.IsLocked
            ? "已锁屏"
            : snapshot.State == RhythmState.Working ? "工作中" : "休息中";

        _statusDescriptionTextBlock.Text = snapshot.IsLocked
            ? "解锁后将重新开始本轮工作计时"
            : snapshot.State == RhythmState.Working
                ? $"距离下一次休息还有 {FormatDuration(snapshot.WorkRemaining)}"
                : $"本次休息还剩 {FormatDuration(snapshot.RestRemaining)}";

        _lockStateTextBlock.Text = snapshot.IsLocked
            ? "锁屏已触发重置，本轮工作时长不会继续累计。"
            : "锁屏后会自动重置当前计时。";
    }

    public void UpdateSettings(RhythmSettings settings)
    {
        if (_workIntervalTextBox is not null && !_workIntervalTextBox.IsKeyboardFocusWithin)
        {
            _workIntervalTextBox.Text = settings.WorkIntervalMinutes.ToString();
        }

        if (_restDurationTextBox is not null && !_restDurationTextBox.IsKeyboardFocusWithin)
        {
            _restDurationTextBox.Text = settings.RestDurationSeconds.ToString();
        }
    }

    public void UpdateHistory(IReadOnlyList<RestSessionRecord> recentSessions, IReadOnlyList<RestSessionRecord> todaySessions)
    {
        _historyItems.Clear();
        foreach (var session in recentSessions)
        {
            _historyItems.Add(new HistoryListItem(
                session.StartedAt.LocalDateTime.ToString("MM-dd HH:mm:ss"),
                FormatResult(session.Result),
                FormatDuration(TimeSpan.FromSeconds(session.PlannedRestSeconds)),
                FormatDuration(TimeSpan.FromSeconds(session.ActualRestSeconds))));
        }

        if (_todayTotalTextBlock is not null)
        {
            _todayTotalTextBlock.Text = todaySessions.Count.ToString();
        }

        if (_todayCompletedTextBlock is not null)
        {
            _todayCompletedTextBlock.Text = todaySessions.Count(session => session.Result == RestSessionResult.Completed).ToString();
        }

        if (_todayAverageRestTextBlock is not null)
        {
            var averageActualSeconds = todaySessions.Count == 0
                ? 0
                : (int)Math.Round(todaySessions.Average(session => session.ActualRestSeconds));

            _todayAverageRestTextBlock.Text = FormatDuration(TimeSpan.FromSeconds(averageActualSeconds));
        }
    }

    private UIElement BuildLayout()
    {
        var root = new Grid { Margin = new Thickness(28) };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        root.ColumnDefinitions.Add(new ColumnDefinition());

        var leftPanel = new Grid();
        leftPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        leftPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
        leftPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        leftPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(28) });
        leftPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        leftPanel.Children.Add(BuildHeader());

        var statusCard = BuildStatusCard();
        Grid.SetRow(statusCard, 2);
        leftPanel.Children.Add(statusCard);

        var settingsCard = BuildSettingsCard();
        Grid.SetRow(settingsCard, 4);
        leftPanel.Children.Add(settingsCard);

        var leftBorder = CreateCardContainer(new Thickness(24), leftPanel);
        Grid.SetColumn(leftBorder, 0);
        root.Children.Add(leftBorder);

        var rightPanel = new Grid();
        rightPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rightPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
        rightPanel.RowDefinitions.Add(new RowDefinition());

        rightPanel.Children.Add(BuildStatsCards());

        var historyCard = BuildHistoryCard();
        Grid.SetRow(historyCard, 2);
        rightPanel.Children.Add(historyCard);

        Grid.SetColumn(rightPanel, 2);
        root.Children.Add(rightPanel);

        return root;
    }

    private UIElement BuildHeader()
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            FontSize = 36,
            FontWeight = FontWeights.Bold,
            Foreground = BrushFromHex("#FF1B1E1C"),
            Text = "Rhythm"
        });
        stack.Children.Add(new TextBlock
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 15,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = "帮你在工作与休息之间，找到更稳定的电脑使用节奏。"
        });
        return stack;
    }

    private UIElement BuildStatusCard()
    {
        _stateTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Foreground = BrushFromHex("#FF17624E"),
            Text = "工作中"
        };
        _statusDescriptionTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 15,
            Foreground = BrushFromHex("#FF1B1E1C"),
            Text = "距离下一次休息还有 00:00"
        };
        _lockStateTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 10, 0, 0),
            FontSize = 13,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = "锁屏后会自动重置当前计时。"
        };

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = "当前状态"
        });
        stack.Children.Add(_stateTextBlock);
        stack.Children.Add(_statusDescriptionTextBlock);
        stack.Children.Add(_lockStateTextBlock);

        return CreateColoredCard("#FFF1E8D8", stack);
    }

    private UIElement BuildSettingsCard()
    {
        _workIntervalTextBox = CreateInputTextBox();
        _restDurationTextBox = CreateInputTextBox();

        var saveButton = new Button
        {
            Width = 112,
            Height = 42,
            Margin = new Thickness(0, 0, 12, 0),
            Background = BrushFromHex("#FF17624E"),
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            Content = "保存设置"
        };
        saveButton.Click += SaveSettingsButton_OnClick;

        var hideButton = new Button
        {
            Width = 92,
            Height = 42,
            Background = Brushes.Transparent,
            BorderBrush = BrushFromHex("#FF17624E"),
            BorderThickness = new Thickness(1.5),
            Foreground = BrushFromHex("#FF17624E"),
            Content = "最小化"
        };
        hideButton.Click += MinimizeButton_OnClick;

        var buttonRow = new StackPanel
        {
            Margin = new Thickness(0, 18, 0, 0),
            Orientation = Orientation.Horizontal
        };
        buttonRow.Children.Add(saveButton);
        buttonRow.Children.Add(hideButton);

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = "节奏设置"
        });
        stack.Children.Add(CreateLabel("每隔多久休息一次（分钟）", new Thickness(0, 16, 0, 6)));
        stack.Children.Add(_workIntervalTextBox);
        stack.Children.Add(CreateLabel("每次休息时长（秒）", new Thickness(0, 16, 0, 6)));
        stack.Children.Add(_restDurationTextBox);
        stack.Children.Add(buttonRow);

        return CreateColoredCard("#FFE7F0EB", stack);
    }

    private UIElement BuildStatsCards()
    {
        _todayTotalTextBlock = CreateStatValue();
        _todayCompletedTextBlock = CreateStatValue();
        _todayAverageRestTextBlock = CreateStatValue("00:00");

        var grid = new UniformGrid { Rows = 1, Columns = 3 };
        grid.Children.Add(CreateStatCard("今日提醒", _todayTotalTextBlock, new Thickness(0, 0, 16, 0)));
        grid.Children.Add(CreateStatCard("今日完成休息", _todayCompletedTextBlock, new Thickness(0, 0, 16, 0)));
        grid.Children.Add(CreateStatCard("今日平均实际休息", _todayAverageRestTextBlock, new Thickness(0)));
        return grid;
    }

    private UIElement BuildHistoryCard()
    {
        var listView = new ListView
        {
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            ItemsSource = _historyItems
        };

        var gridView = new GridView { AllowsColumnReorder = false };
        gridView.Columns.Add(CreateColumn("开始时间", "StartedAt", 200));
        gridView.Columns.Add(CreateColumn("结果", "Result", 180));
        gridView.Columns.Add(CreateColumn("计划休息", "Planned", 160));
        gridView.Columns.Add(CreateColumn("实际休息", "Actual", 160));
        listView.View = gridView;

        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(18) });
        contentGrid.RowDefinitions.Add(new RowDefinition());

        var header = new StackPanel();
        header.Children.Add(new TextBlock
        {
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = BrushFromHex("#FF1B1E1C"),
            Text = "最近休息记录"
        });
        header.Children.Add(new TextBlock
        {
            Margin = new Thickness(0, 6, 0, 0),
            FontSize = 14,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = "记录每次提醒的结果、计划时长和实际休息时长。"
        });

        contentGrid.Children.Add(header);
        Grid.SetRow(listView, 2);
        contentGrid.Children.Add(listView);

        return CreateCardContainer(new Thickness(24), contentGrid);
    }

    private void SaveSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_workIntervalTextBox is null || _restDurationTextBox is null)
        {
            return;
        }

        if (!int.TryParse(_workIntervalTextBox.Text, out var workIntervalMinutes) ||
            !int.TryParse(_restDurationTextBox.Text, out var restDurationSeconds))
        {
            MessageBox.Show(this, "请输入有效的整数。", "Rhythm", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Controller.SaveSettings(workIntervalMinutes, restDurationSeconds, out var errorMessage))
        {
            MessageBox.Show(this, errorMessage, "Rhythm", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show(this, "节奏设置已保存，并从现在开始重新计时。", "Rhythm", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        if (!Controller.IsExiting)
        {
            Controller.Exit();
        }
    }

    private static Border CreateCardContainer(Thickness padding, UIElement child)
    {
        return new Border
        {
            Padding = padding,
            Background = BrushFromHex("#FFFDFBF7"),
            CornerRadius = new CornerRadius(24),
            Child = child
        };
    }

    private static Border CreateColoredCard(string backgroundHex, UIElement child)
    {
        return new Border
        {
            Padding = new Thickness(18),
            Background = BrushFromHex(backgroundHex),
            CornerRadius = new CornerRadius(18),
            Child = child
        };
    }

    private static Border CreateStatCard(string title, TextBlock value, Thickness margin)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            FontSize = 14,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = title
        });
        stack.Children.Add(value);

        return new Border
        {
            Margin = margin,
            Padding = new Thickness(22),
            Background = BrushFromHex("#FFFDFBF7"),
            CornerRadius = new CornerRadius(24),
            Child = stack
        };
    }

    private static TextBox CreateInputTextBox()
    {
        return new TextBox
        {
            Padding = new Thickness(12, 10, 12, 10),
            FontSize = 16,
            BorderBrush = BrushFromHex("#FF92B8A8"),
            BorderThickness = new Thickness(1.5)
        };
    }

    private static TextBlock CreateStatValue(string text = "0")
    {
        return new TextBlock
        {
            Margin = new Thickness(0, 10, 0, 0),
            FontSize = 34,
            FontWeight = FontWeights.Bold,
            Foreground = BrushFromHex("#FF1B1E1C"),
            Text = text
        };
    }

    private static TextBlock CreateLabel(string text, Thickness margin)
    {
        return new TextBlock
        {
            Margin = margin,
            FontSize = 14,
            Foreground = BrushFromHex("#FF1B1E1C"),
            Text = text
        };
    }

    private static GridViewColumn CreateColumn(string header, string bindingPath, double width)
    {
        return new GridViewColumn
        {
            Header = header,
            Width = width,
            DisplayMemberBinding = new Binding(bindingPath)
        };
    }

    private static SolidColorBrush BrushFromHex(string value)
    {
        return (SolidColorBrush)new BrushConverter().ConvertFrom(value)!;
    }

    private static string FormatResult(RestSessionResult result)
    {
        return result switch
        {
            RestSessionResult.Completed => "已完成",
            RestSessionResult.Skipped => "已跳过（ESC）",
            RestSessionResult.InterruptedByLock => "因锁屏中断",
            _ => result.ToString()
        };
    }

    private static string FormatDuration(TimeSpan duration)
    {
        duration = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        var totalHours = (int)duration.TotalHours;
        return totalHours > 0
            ? $"{totalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}"
            : $"{duration.Minutes:00}:{duration.Seconds:00}";
    }
}

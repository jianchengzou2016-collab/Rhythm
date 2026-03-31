using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Rhythm.App.Localization;
using Rhythm.App.Models;
using Rhythm.Core.Abstractions;
using Rhythm.Core.Models;
using WpfBitmapImage = System.Windows.Media.Imaging.BitmapImage;
using WpfBinding = System.Windows.Data.Binding;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfButton = System.Windows.Controls.Button;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfListView = System.Windows.Controls.ListView;
using WpfMessageBox = System.Windows.MessageBox;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Rhythm.App;

public partial class MainWindow : IMainShell
{
    private readonly ObservableCollection<HistoryListItem> _historyItems = [];
    private bool _runtimeLayoutInitialized;
    private string _languageCode = "zh-CN";
    private WpfTextBox? _workIntervalTextBox;
    private WpfTextBox? _restDurationTextBox;
    private WpfComboBox? _languageComboBox;
    private TextBlock? _appTitleTextBlock;
    private TextBlock? _appDescriptionTextBlock;
    private TextBlock? _currentStateLabelTextBlock;
    private TextBlock? _stateTextBlock;
    private TextBlock? _statusDescriptionTextBlock;
    private TextBlock? _lockStateTextBlock;
    private TextBlock? _settingsTitleTextBlock;
    private TextBlock? _workIntervalLabelTextBlock;
    private TextBlock? _restDurationLabelTextBlock;
    private TextBlock? _languageLabelTextBlock;
    private WpfButton? _saveButton;
    private WpfButton? _minimizeButton;
    private TextBlock? _todayTotalLabelTextBlock;
    private TextBlock? _todayCompletedLabelTextBlock;
    private TextBlock? _todayAverageLabelTextBlock;
    private TextBlock? _recentHistoryTitleTextBlock;
    private TextBlock? _historyDescriptionTextBlock;
    private GridViewColumn? _historyStartedColumn;
    private GridViewColumn? _historyResultColumn;
    private GridViewColumn? _historyPlannedColumn;
    private GridViewColumn? _historyActualColumn;
    private TextBlock? _todayTotalTextBlock;
    private TextBlock? _todayCompletedTextBlock;
    private TextBlock? _todayAverageRestTextBlock;

    public void InitializeShell()
    {
        if (_runtimeLayoutInitialized)
        {
            return;
        }

        _runtimeLayoutInitialized = true;

        Title = "Rhythm";
        Width = 1120;
        Height = 760;
        MinWidth = 960;
        MinHeight = 680;
        Background = BrushFromHex("#FFF6F1E8");
        FontFamily = new WpfFontFamily("Bahnschrift");
        Icon = new WpfBitmapImage(new Uri("pack://application:,,,/Assets/Rhythm.png"));
        Closing += Window_OnClosing;
        Content = BuildLayout();
    }

    public void ShowShell()
    {
        Show();
    }

    public void HideShell()
    {
        Hide();
    }

    public void RestoreAndActivateShell()
    {
        Show();
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
    }

    public void CloseShell()
    {
        Close();
    }

    public void UpdateStatus(RhythmStatusSnapshot snapshot)
    {
        if (_stateTextBlock is null || _statusDescriptionTextBlock is null || _lockStateTextBlock is null)
        {
            return;
        }

        _languageCode = snapshot.Settings.LanguageCode;
        _stateTextBlock.Text = snapshot.IsLocked
            ? AppText.Get(_languageCode, "StateLocked")
            : snapshot.State == RhythmState.Working
                ? AppText.Get(_languageCode, "StateWorking")
                : AppText.Get(_languageCode, "StateResting");

        _statusDescriptionTextBlock.Text = snapshot.IsLocked
            ? AppText.Get(_languageCode, "StatusLocked")
            : snapshot.State == RhythmState.Working
                ? AppText.Format(_languageCode, "StatusWorking", FormatDuration(snapshot.WorkRemaining))
                : AppText.Format(_languageCode, "StatusResting", FormatDuration(snapshot.RestRemaining));

        _lockStateTextBlock.Text = snapshot.IsLocked
            ? AppText.Get(_languageCode, "LockHintLocked")
            : AppText.Get(_languageCode, "LockHint");
    }

    public void UpdateSettings(RhythmSettings settings)
    {
        _languageCode = settings.LanguageCode;
        if (_workIntervalTextBox is not null && !_workIntervalTextBox.IsKeyboardFocusWithin)
        {
            _workIntervalTextBox.Text = settings.WorkIntervalMinutes.ToString();
        }

        if (_restDurationTextBox is not null && !_restDurationTextBox.IsKeyboardFocusWithin)
        {
            _restDurationTextBox.Text = settings.RestDurationSeconds.ToString();
        }

        if (_languageComboBox is not null && !_languageComboBox.IsKeyboardFocusWithin && !_languageComboBox.IsDropDownOpen)
        {
            _languageComboBox.SelectedValue = settings.LanguageCode;
        }
    }

    public void ApplyLocalization(string languageCode, RhythmStatusSnapshot snapshot)
    {
        _languageCode = languageCode;
        Title = AppText.Get(languageCode, "AppName");

        if (_appTitleTextBlock is not null)
        {
            _appTitleTextBlock.Text = AppText.Get(languageCode, "AppName");
        }

        if (_appDescriptionTextBlock is not null)
        {
            _appDescriptionTextBlock.Text = AppText.Get(languageCode, "AppDescription");
        }

        if (_currentStateLabelTextBlock is not null)
        {
            _currentStateLabelTextBlock.Text = AppText.Get(languageCode, "CurrentState");
        }

        if (_settingsTitleTextBlock is not null)
        {
            _settingsTitleTextBlock.Text = AppText.Get(languageCode, "SettingsTitle");
        }

        if (_workIntervalLabelTextBlock is not null)
        {
            _workIntervalLabelTextBlock.Text = AppText.Get(languageCode, "WorkIntervalLabel");
        }

        if (_restDurationLabelTextBlock is not null)
        {
            _restDurationLabelTextBlock.Text = AppText.Get(languageCode, "RestDurationLabel");
        }

        if (_languageLabelTextBlock is not null)
        {
            _languageLabelTextBlock.Text = AppText.Get(languageCode, "LanguageLabel");
        }

        if (_saveButton is not null)
        {
            _saveButton.Content = AppText.Get(languageCode, "SaveSettings");
        }

        if (_minimizeButton is not null)
        {
            _minimizeButton.Content = AppText.Get(languageCode, "Minimize");
        }

        if (_todayTotalLabelTextBlock is not null)
        {
            _todayTotalLabelTextBlock.Text = AppText.Get(languageCode, "TodayTotal");
        }

        if (_todayCompletedLabelTextBlock is not null)
        {
            _todayCompletedLabelTextBlock.Text = AppText.Get(languageCode, "TodayCompleted");
        }

        if (_todayAverageLabelTextBlock is not null)
        {
            _todayAverageLabelTextBlock.Text = AppText.Get(languageCode, "TodayAverage");
        }

        if (_recentHistoryTitleTextBlock is not null)
        {
            _recentHistoryTitleTextBlock.Text = AppText.Get(languageCode, "RecentHistory");
        }

        if (_historyDescriptionTextBlock is not null)
        {
            _historyDescriptionTextBlock.Text = AppText.Get(languageCode, "HistoryDescription");
        }

        if (_historyStartedColumn is not null)
        {
            _historyStartedColumn.Header = AppText.Get(languageCode, "HistoryStart");
        }

        if (_historyResultColumn is not null)
        {
            _historyResultColumn.Header = AppText.Get(languageCode, "HistoryResult");
        }

        if (_historyPlannedColumn is not null)
        {
            _historyPlannedColumn.Header = AppText.Get(languageCode, "HistoryPlanned");
        }

        if (_historyActualColumn is not null)
        {
            _historyActualColumn.Header = AppText.Get(languageCode, "HistoryActual");
        }

        UpdateStatus(snapshot);
    }

    public void UpdateHistory(IReadOnlyList<RestSessionRecord> recentSessions, IReadOnlyList<RestSessionRecord> todaySessions, string languageCode)
    {
        _languageCode = languageCode;
        _historyItems.Clear();
        foreach (var session in recentSessions)
        {
            _historyItems.Add(new HistoryListItem(
                session.StartedAt.LocalDateTime.ToString("MM-dd HH:mm:ss"),
                FormatResult(languageCode, session.Result),
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
        _appTitleTextBlock = new TextBlock
        {
            FontSize = 36,
            FontWeight = FontWeights.Bold,
            Foreground = BrushFromHex("#FF1B1E1C"),
            Text = AppText.Get(_languageCode, "AppName")
        };
        stack.Children.Add(_appTitleTextBlock);
        _appDescriptionTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 15,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = AppText.Get(_languageCode, "AppDescription")
        };
        stack.Children.Add(_appDescriptionTextBlock);
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
            Text = AppText.Get(_languageCode, "StateWorking")
        };
        _statusDescriptionTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 15,
            Foreground = BrushFromHex("#FF1B1E1C"),
            Text = AppText.Format(_languageCode, "StatusWorking", "00:00")
        };
        _lockStateTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 10, 0, 0),
            FontSize = 13,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = AppText.Get(_languageCode, "LockHint")
        };

        var stack = new StackPanel();
        _currentStateLabelTextBlock = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = AppText.Get(_languageCode, "CurrentState")
        };
        stack.Children.Add(_currentStateLabelTextBlock);
        stack.Children.Add(_stateTextBlock);
        stack.Children.Add(_statusDescriptionTextBlock);
        stack.Children.Add(_lockStateTextBlock);

        return CreateColoredCard("#FFF1E8D8", stack);
    }

    private UIElement BuildSettingsCard()
    {
        _workIntervalTextBox = CreateInputTextBox();
        _restDurationTextBox = CreateInputTextBox();
        _languageComboBox = new WpfComboBox
        {
            Padding = new Thickness(12, 8, 12, 8),
            FontSize = 16,
            BorderBrush = BrushFromHex("#FF92B8A8"),
            BorderThickness = new Thickness(1.5),
            ItemsSource = AppText.SupportedLanguages,
            DisplayMemberPath = nameof(LanguageOption.DisplayName),
            SelectedValuePath = nameof(LanguageOption.Code),
            SelectedValue = _languageCode
        };

        _saveButton = new WpfButton
        {
            Width = 112,
            Height = 42,
            Margin = new Thickness(0, 0, 12, 0),
            Background = BrushFromHex("#FF17624E"),
            BorderThickness = new Thickness(0),
            Foreground = WpfBrushes.White,
            Content = AppText.Get(_languageCode, "SaveSettings")
        };
        _saveButton.Click += SaveSettingsButton_OnClick;

        _minimizeButton = new WpfButton
        {
            Width = 92,
            Height = 42,
            Background = WpfBrushes.Transparent,
            BorderBrush = BrushFromHex("#FF17624E"),
            BorderThickness = new Thickness(1.5),
            Foreground = BrushFromHex("#FF17624E"),
            Content = AppText.Get(_languageCode, "Minimize")
        };
        _minimizeButton.Click += MinimizeButton_OnClick;

        var buttonRow = new StackPanel
        {
            Margin = new Thickness(0, 18, 0, 0),
            Orientation = WpfOrientation.Horizontal
        };
        buttonRow.Children.Add(_saveButton);
        buttonRow.Children.Add(_minimizeButton);

        var stack = new StackPanel();
        _settingsTitleTextBlock = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = AppText.Get(_languageCode, "SettingsTitle")
        };
        stack.Children.Add(_settingsTitleTextBlock);
        _workIntervalLabelTextBlock = CreateLabel(AppText.Get(_languageCode, "WorkIntervalLabel"), new Thickness(0, 16, 0, 6));
        stack.Children.Add(_workIntervalLabelTextBlock);
        stack.Children.Add(_workIntervalTextBox);
        _restDurationLabelTextBlock = CreateLabel(AppText.Get(_languageCode, "RestDurationLabel"), new Thickness(0, 16, 0, 6));
        stack.Children.Add(_restDurationLabelTextBlock);
        stack.Children.Add(_restDurationTextBox);
        _languageLabelTextBlock = CreateLabel(AppText.Get(_languageCode, "LanguageLabel"), new Thickness(0, 16, 0, 6));
        stack.Children.Add(_languageLabelTextBlock);
        stack.Children.Add(_languageComboBox);
        stack.Children.Add(buttonRow);

        return CreateColoredCard("#FFE7F0EB", stack);
    }

    private UIElement BuildStatsCards()
    {
        _todayTotalTextBlock = CreateStatValue();
        _todayCompletedTextBlock = CreateStatValue();
        _todayAverageRestTextBlock = CreateStatValue("00:00");

        var grid = new UniformGrid { Rows = 1, Columns = 3 };
        _todayTotalLabelTextBlock = CreateStatLabel(AppText.Get(_languageCode, "TodayTotal"));
        _todayCompletedLabelTextBlock = CreateStatLabel(AppText.Get(_languageCode, "TodayCompleted"));
        _todayAverageLabelTextBlock = CreateStatLabel(AppText.Get(_languageCode, "TodayAverage"));
        grid.Children.Add(CreateStatCard(_todayTotalLabelTextBlock, _todayTotalTextBlock, new Thickness(0, 0, 16, 0)));
        grid.Children.Add(CreateStatCard(_todayCompletedLabelTextBlock, _todayCompletedTextBlock, new Thickness(0, 0, 16, 0)));
        grid.Children.Add(CreateStatCard(_todayAverageLabelTextBlock, _todayAverageRestTextBlock, new Thickness(0)));
        return grid;
    }

    private UIElement BuildHistoryCard()
    {
        var listView = new WpfListView
        {
            BorderThickness = new Thickness(0),
            Background = WpfBrushes.Transparent,
            ItemsSource = _historyItems
        };

        var gridView = new GridView { AllowsColumnReorder = false };
        _historyStartedColumn = CreateColumn(AppText.Get(_languageCode, "HistoryStart"), "StartedAt", 200);
        _historyResultColumn = CreateColumn(AppText.Get(_languageCode, "HistoryResult"), "Result", 180);
        _historyPlannedColumn = CreateColumn(AppText.Get(_languageCode, "HistoryPlanned"), "Planned", 160);
        _historyActualColumn = CreateColumn(AppText.Get(_languageCode, "HistoryActual"), "Actual", 160);
        gridView.Columns.Add(_historyStartedColumn);
        gridView.Columns.Add(_historyResultColumn);
        gridView.Columns.Add(_historyPlannedColumn);
        gridView.Columns.Add(_historyActualColumn);
        listView.View = gridView;

        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(18) });
        contentGrid.RowDefinitions.Add(new RowDefinition());

        var header = new StackPanel();
        _recentHistoryTitleTextBlock = new TextBlock
        {
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = BrushFromHex("#FF1B1E1C"),
            Text = AppText.Get(_languageCode, "RecentHistory")
        };
        header.Children.Add(_recentHistoryTitleTextBlock);
        _historyDescriptionTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 6, 0, 0),
            FontSize = 14,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = AppText.Get(_languageCode, "HistoryDescription")
        };
        header.Children.Add(_historyDescriptionTextBlock);

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
            WpfMessageBox.Show(this, AppText.Get(_languageCode, "InvalidInteger"), AppText.Get(_languageCode, "AppName"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var languageCode = _languageComboBox?.SelectedValue as string ?? _languageCode;

        if (!Coordinator.SaveSettings(workIntervalMinutes, restDurationSeconds, languageCode, out var errorMessage))
        {
            WpfMessageBox.Show(this, errorMessage, AppText.Get(languageCode, "AppName"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        WpfMessageBox.Show(this, AppText.Get(languageCode, "SettingsSaved"), AppText.Get(languageCode, "AppName"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        Coordinator.HideMainShell();
    }

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        if (!Coordinator.IsExiting)
        {
            e.Cancel = true;
            Coordinator.HideMainShell();
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

    private static Border CreateStatCard(TextBlock title, TextBlock value, Thickness margin)
    {
        var stack = new StackPanel();
        stack.Children.Add(title);
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

    private static TextBlock CreateStatLabel(string text)
    {
        return new TextBlock
        {
            FontSize = 14,
            Foreground = BrushFromHex("#FF5E655F"),
            Text = text
        };
    }

    private static WpfTextBox CreateInputTextBox()
    {
        return new WpfTextBox
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
            DisplayMemberBinding = new WpfBinding(bindingPath)
        };
    }

    private static SolidColorBrush BrushFromHex(string value)
    {
        return (SolidColorBrush)new BrushConverter().ConvertFrom(value)!;
    }

    private static string FormatResult(string languageCode, RestSessionResult result)
    {
        return result switch
        {
            RestSessionResult.Completed => AppText.Get(languageCode, "ResultCompleted"),
            RestSessionResult.Skipped => AppText.Get(languageCode, "ResultSkipped"),
            RestSessionResult.InterruptedByLock => AppText.Get(languageCode, "ResultInterrupted"),
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

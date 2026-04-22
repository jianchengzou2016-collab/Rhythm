using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rhythm.App.Localization;
using WpfColor = System.Windows.Media.Color;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;

namespace Rhythm.App;

public sealed class RestOverlayWindow : Window
{
    private readonly Action _skipCurrentRest;
    private string _languageCode = "zh-CN";
    private readonly TextBlock _titleTextBlock;
    private readonly TextBlock _subtitleTextBlock;
    private readonly TextBlock _countdownTextBlock;
    private readonly TextBlock _hintTextBlock;

    public RestOverlayWindow(Action skipCurrentRest, System.Windows.Forms.Screen screen)
    {
        _skipCurrentRest = skipCurrentRest;

        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = new SolidColorBrush(WpfColor.FromArgb(153, 12, 18, 16));
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        WindowStartupLocation = WindowStartupLocation.Manual;
        FontFamily = new WpfFontFamily("Bahnschrift");
        PreviewKeyDown += Window_OnPreviewKeyDown;
        Loaded += Window_OnLoaded;
        Closed += Window_OnClosed;

        Left = screen.Bounds.Left;
        Top = screen.Bounds.Top;
        Width = screen.Bounds.Width;
        Height = screen.Bounds.Height;

        _countdownTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 18, 0, 0),
            HorizontalAlignment = WpfHorizontalAlignment.Center,
            FontSize = 84,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(WpfColor.FromRgb(23, 98, 78)),
            Text = "10:00"
        };
        _titleTextBlock = new TextBlock();
        _subtitleTextBlock = new TextBlock();
        _hintTextBlock = new TextBlock();

        Content = BuildContent();
        ApplyLocalization(_languageCode);
    }

    public void ApplyLocalization(string languageCode)
    {
        _languageCode = languageCode;
        Title = AppText.Get(languageCode, "AppName");
        _titleTextBlock.Text = AppText.Get(languageCode, "OverlayTitle");
        _subtitleTextBlock.Text = AppText.Get(languageCode, "OverlaySubtitle");
        _hintTextBlock.Text = AppText.Get(languageCode, "OverlayHint");
    }

    public void UpdateCountdown(TimeSpan remaining)
    {
        _countdownTextBlock.Text = FormatDuration(remaining);
    }

    private UIElement BuildContent()
    {
        var container = new Border
        {
            Width = 540,
            Padding = new Thickness(36),
            HorizontalAlignment = WpfHorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(WpfColor.FromArgb(204, 248, 244, 236)),
            CornerRadius = new CornerRadius(28)
        };

        _titleTextBlock.HorizontalAlignment = WpfHorizontalAlignment.Center;
        _titleTextBlock.FontSize = 18;
        _titleTextBlock.Foreground = new SolidColorBrush(WpfColor.FromRgb(75, 91, 83));

        _subtitleTextBlock.Margin = new Thickness(0, 18, 0, 0);
        _subtitleTextBlock.HorizontalAlignment = WpfHorizontalAlignment.Center;
        _subtitleTextBlock.FontSize = 56;
        _subtitleTextBlock.FontWeight = FontWeights.Bold;
        _subtitleTextBlock.Foreground = new SolidColorBrush(WpfColor.FromRgb(16, 35, 28));

        _hintTextBlock.Margin = new Thickness(0, 18, 0, 0);
        _hintTextBlock.HorizontalAlignment = WpfHorizontalAlignment.Center;
        _hintTextBlock.FontSize = 16;
        _hintTextBlock.Foreground = new SolidColorBrush(WpfColor.FromRgb(75, 91, 83));

        var stack = new StackPanel();
        stack.Children.Add(_titleTextBlock);
        stack.Children.Add(_subtitleTextBlock);
        stack.Children.Add(_countdownTextBlock);
        stack.Children.Add(_hintTextBlock);

        container.Child = stack;

        var root = new Grid();
        root.Children.Add(container);
        return root;
    }

    private void Window_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        _skipCurrentRest();
    }

    private void Window_OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus();
        Keyboard.Focus(this);
    }

    private void Window_OnClosed(object? sender, EventArgs e)
    {
        PreviewKeyDown -= Window_OnPreviewKeyDown;
        Loaded -= Window_OnLoaded;
        Closed -= Window_OnClosed;
        Content = null;
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

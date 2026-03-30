using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rhythm.App.Services;

namespace Rhythm.App;

public sealed class RestOverlayWindow : Window
{
    private readonly AppController _controller;
    private readonly TextBlock _countdownTextBlock;

    public RestOverlayWindow(AppController controller)
    {
        _controller = controller;

        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = new SolidColorBrush(Color.FromArgb(153, 12, 18, 16));
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        WindowState = WindowState.Maximized;
        WindowStartupLocation = WindowStartupLocation.Manual;
        FontFamily = new FontFamily("Bahnschrift");
        PreviewKeyDown += Window_OnPreviewKeyDown;
        Loaded += Window_OnLoaded;

        _countdownTextBlock = new TextBlock
        {
            Margin = new Thickness(0, 18, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 84,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(23, 98, 78)),
            Text = "10:00"
        };

        Content = BuildContent();
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
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(204, 248, 244, 236)),
            CornerRadius = new CornerRadius(28)
        };

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(75, 91, 83)),
            Text = "该休息了"
        });
        stack.Children.Add(new TextBlock
        {
            Margin = new Thickness(0, 18, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 56,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(16, 35, 28)),
            Text = "放松一下眼睛和肩膀"
        });
        stack.Children.Add(_countdownTextBlock);
        stack.Children.Add(new TextBlock
        {
            Margin = new Thickness(0, 18, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.FromRgb(75, 91, 83)),
            Text = "按 ESC 跳过这次休息提醒"
        });

        container.Child = stack;

        var root = new Grid();
        root.Children.Add(container);
        return root;
    }

    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        _controller.SkipCurrentRest();
    }

    private void Window_OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus();
        Keyboard.Focus(this);
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

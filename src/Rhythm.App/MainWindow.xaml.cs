using System.Windows;
using Rhythm.Core.Abstractions;

namespace Rhythm.App;

public partial class MainWindow : Window
{
    public MainWindow(IMainShellCoordinator coordinator)
    {
        Coordinator = coordinator;
        InitializeComponent();
    }

    private IMainShellCoordinator Coordinator { get; }
}

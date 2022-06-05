using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PurpleExplorer.Models;
using Splat;

namespace PurpleExplorer.Views;

public partial class AppSettingsWindow : Window
{
    public AppSettingsWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
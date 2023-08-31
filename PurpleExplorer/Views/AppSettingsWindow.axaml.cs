using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
    
    public void OnClose(object? sender, RoutedEventArgs args)
    {
        this.Close();
    }
}
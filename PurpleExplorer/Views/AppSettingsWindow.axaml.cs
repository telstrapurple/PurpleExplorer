using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

        // Even though we do as we do in ConnectionStringWindow it does not work to set focus.
        // See https://stackoverflow.com/questions/7050559/how-to-set-focus-on-numericupdown-control
        var ctrl = this.FindControl<NumericUpDown>("QueueListFetchCount")
                   ?? throw new Exception("Control not found.");
        SetFocus(ctrl);
    }

    public void OnClose(object? sender, RoutedEventArgs args)
    {
        this.Close();
    }
    /// <summary>This method sets the focus to the control at start time.
    /// I would prefer to do it *in xaml*,
    /// like in this issue: <see cref="https://github.com/AvaloniaUI/Avalonia/issues/4835#issuecomment-707590940"/>
    /// but I cannot get the code to work.
    /// I added
    /// "Avalonia.Xaml.Behaviors" Version="11.0.2"
    /// "Avalonia.Xaml.Interactions" Version="11.0.2"
    /// "Avalonia.Xaml.Interactions.Responsive" Version="11.0.2"
    /// "Avalonia.Xaml.Interactivity" Version="11.0.2"
    /// and updated the xaml as the link says; but to no avail.
    ///
    /// I keep this information here for future me, or someone else, to stumble over and implement.
    /// </summary>
    /// <param name="ctrl"></param>
    private static void SetFocus(InputElement ctrl)
    {
        ctrl.Focusable = true;
        ctrl.AttachedToVisualTree += (_,_) => ctrl.Focus();
    }
}
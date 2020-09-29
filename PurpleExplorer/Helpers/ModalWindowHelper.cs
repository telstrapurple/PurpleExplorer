﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PurpleExplorer.ViewModels;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers
{
    public class ModalWindowHelper
    {
        public static async Task<U> ShowModalWindow<T, U>(ViewModelBase viewModel) where T : Window, new() where U : ViewModelBase
        {
            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                .Windows[0];

            T window = new T();
            window.DataContext = viewModel;
            window.Icon = new WindowIcon("/Assets/avalonia-logo.ico");

            await window.ShowDialog(mainWindow);
            return window.DataContext as U;
        }
    }
}

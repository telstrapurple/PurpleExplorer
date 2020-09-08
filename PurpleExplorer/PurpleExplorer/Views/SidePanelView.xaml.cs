using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PurpleExplorer.Views
{
    public class SidePanelView : UserControl
    {
        public SidePanelView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
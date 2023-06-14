using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Scans_Mover.Views
{
    public partial class MessageBoxView : Window
    {
        public MessageBoxView()
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
}

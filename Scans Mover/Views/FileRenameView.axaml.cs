using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Scans_Mover.Views
{
    public partial class FileRenameView : Window
    {
        public FileRenameView()
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

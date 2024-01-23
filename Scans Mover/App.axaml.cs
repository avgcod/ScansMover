using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Scans_Mover.Services;
using Scans_Mover.ViewModels;
using Scans_Mover.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace Scans_Mover
{
    public partial class App : Application
    {
        MoverViewModel? mvModel;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MoverView();
                desktop.MainWindow.Closing += MainWindow_Closing;
                mvModel = new MoverViewModel(desktop.MainWindow, StrongReferenceMessenger.Default, "settings.json");
                mvModel.IsActive = true;
                desktop.MainWindow.DataContext = mvModel;
            }

            base.OnFrameworkInitializationCompleted();

        }

        private void MainWindow_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
        {
            mvModel!.IsActive = false;
        }
    }
}
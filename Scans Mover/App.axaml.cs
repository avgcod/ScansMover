using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scans_Mover.Components;
using Scans_Mover.Services;
using Scans_Mover.ViewModels;
using Scans_Mover.Views;

namespace Scans_Mover
{
    public partial class App : Application
    {
        IHost? _host;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            _host = CreateHostBuilder([]).Build();
            _host.Start();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = _host.Services.GetRequiredService<MoverView>();
                desktop.MainWindow.DataContext = _host.Services.GetRequiredService<MoverViewModel>();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<MoverView>();
                services.AddSingleton<MoverViewModel>();

                //services.AddSingleton<FileMovingTab>();
                //services.AddSingleton<FileMovingTabViewModel>();

                //services.AddSingleton<DetailsTab>();
                //services.AddSingleton<DetailsTabViewModel>();

                //services.AddSingleton<LocationsTab>();
                //services.AddSingleton<LocationsTabViewModel>();

                services.AddSingleton<FileRenameService>();

                services.AddSingleton<StrongReferenceMessenger>();
                services.AddSingleton<IMessenger, StrongReferenceMessenger>(provider => provider.GetRequiredService<StrongReferenceMessenger>());

                services.AddScoped<MessageBoxView>();
                services.AddScoped<MessageBoxViewModel>();

                services.AddScoped<ErrorMessageBoxView>();
                services.AddScoped<ErrorMessageBoxViewModel>();

                services.AddScoped<FileRenameView>();
                services.AddScoped<FileRenameViewModel>();
            });
    }
}
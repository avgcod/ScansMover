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
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MoverView();
                desktop.MainWindow.DataContext = new MoverViewModel(desktop.MainWindow, StrongReferenceMessenger.Default, new FileRenameService(StrongReferenceMessenger.Default), "settings.json");
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
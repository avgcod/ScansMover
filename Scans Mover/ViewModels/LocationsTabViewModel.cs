using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;
using Scans_Mover.Services;
using Scans_Mover.Views;
using System;
using System.Threading.Tasks;

namespace Scans_Mover.ViewModels
{
    public partial class LocationsTabViewModel : ViewModelBase, IRecipient<ScanTypeChangedMessage>
    {
        private readonly Window _parentWindow;

        [ObservableProperty]
        private LocationType _locationType = LocationType.Scans;

        [ObservableProperty]
        private string _mainFolder = string.Empty;

        [ObservableProperty]
        private string _deliveriesFolder = string.Empty;

        [ObservableProperty]
        public string _rMAsFolder = string.Empty;

        [ObservableProperty]
        public string _shippingLogsFolder = string.Empty;

        [ObservableProperty]
        public string _serviceFolder = string.Empty;

        public LocationsTabViewModel(MoverView parentWindow, IMessenger theMessenger) : base(theMessenger)
        {
            _parentWindow = parentWindow;
            IsActive = true;

            _parentWindow.Closing += ParentWindow_Closing;
        }

        private void ParentWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            IsActive = false;
        }

        protected override void OnActivated()
        {
            Messenger.RegisterAll(this);

            base.OnActivated();
        }

        protected override void OnDeactivated()
        {
            Messenger.UnregisterAll(this);

            base.OnDeactivated();
        }

        [RelayCommand]
        public void LocationChecked(object? parameter)
        {
            string typeText = parameter?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(typeText))
            {
                LocationType = (LocationType)Enum.Parse(typeof(LocationType), typeText);
            }
        }

        [RelayCommand]
        public async Task ChangeLocation()
        {
            IStorageFolder? selectedFolder = await FileAccessService.ChooseLocationAsync(_parentWindow, Messenger);

            if (selectedFolder != null)
            {
                if (selectedFolder.CanBookmark)
                {
                    if (LocationType == LocationType.Scans)
                    {
                        MainFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                    else if (LocationType == LocationType.Deliveries)
                    {
                        DeliveriesFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                    else if (LocationType == LocationType.Shipping)
                    {
                        ShippingLogsFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                    else if (LocationType == LocationType.RMAs)
                    {
                        RMAsFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                    else
                    {
                        ServiceFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                }
            }
        }

        partial void OnMainFolderChanged(string value)
        {
            Messenger.Send<MainFolderMessage>(new MainFolderMessage(value));
        }

        partial void OnDeliveriesFolderChanged(string value)
        {
            Messenger.Send<DeliveriesFolderMessage>(new DeliveriesFolderMessage(value));
        }

        partial void OnRMAsFolderChanged(string value)
        {
            Messenger.Send<RMAsFolderMessage>(new RMAsFolderMessage(value));
        }

        partial void OnServiceFolderChanged(string value)
        {
            Messenger.Send<ServiceFolderMessage>(new ServiceFolderMessage(value));
        }

        partial void OnShippingLogsFolderChanged(string value)
        {
            Messenger.Send<ShippingLogsFolderMessage>(new ShippingLogsFolderMessage(value));
        }

        private async Task RequestInformation()
        {
            MainFolder = await Messenger.Send<MainFolderRequestMessage>();
            DeliveriesFolder = await Messenger.Send<DeliveriesFolderRequestMessage>();
            ShippingLogsFolder = await Messenger.Send<ShippingLogsFolderRequestMessage>();
            RMAsFolder = await Messenger.Send<RMAsFolderRequestMessage>();
            ServiceFolder = await Messenger.Send<ServiceFolderRequestMessage>();
        }

        public async void Receive(ScanTypeChangedMessage message)
        {
            await RequestInformation();
        }
    }
}

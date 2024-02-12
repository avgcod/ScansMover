using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;
using Scans_Mover.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scans_Mover.ViewModels
{
    public partial class LocationsTabViewModel(Window parentWindow, IMessenger theMessenger) : ViewModelBase(theMessenger)
    {
        private readonly Window _parentWindow = parentWindow;

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
    }
}

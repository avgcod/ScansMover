using Scans_Mover.ViewModels;
using Avalonia.Controls;
using Scans_Mover.Models;

namespace Scans_Mover.Commands
{
    public class ChangeLocationCommand : CommandBase
    {
        private readonly MoverViewModel _moverViewModel;
        private readonly Window _currentWindow;

        public ChangeLocationCommand(Window currentWindow, MoverViewModel moverViewModel)
        {
            _currentWindow = currentWindow;
            _moverViewModel = moverViewModel;
        }

        public override bool CanExecute(object? parameter)
        {
            return base.CanExecute(parameter);
        }
        public override void Execute(object? parameter)
        {
            OpenFolderDialog fbdChooser = new OpenFolderDialog();
            string selectedFolder = fbdChooser.ShowAsync(_currentWindow)?.Result ?? string.Empty;
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                if (_moverViewModel.LocType == LocationType.Scans)
                {
                    _moverViewModel.MainFolder = selectedFolder;
                }
                else if (_moverViewModel.LocType == LocationType.Deliveries)
                {
                    _moverViewModel.DeliveriesFolder = selectedFolder;
                }
                else if (_moverViewModel.LocType == LocationType.Shipping)
                {
                    _moverViewModel.ShippingLogsFolder = selectedFolder;
                }
                else if (_moverViewModel.LocType == LocationType.RMAs)
                {
                    _moverViewModel.RMAsFolder = selectedFolder;
                }
                else
                {
                    _moverViewModel.ServiceFolder = selectedFolder;
                }
            }

        }
    }
}

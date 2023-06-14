using Avalonia.Controls;
using Scans_Mover.ViewModels;

namespace Scans_Mover.Commands
{
    public class CancelCommand : CommandBase
    {
        private readonly FileRenameViewModel _frViewModel;
        private readonly Window _currentWindow;
        public CancelCommand(Window currentWindow, FileRenameViewModel frViewModel)
        {
            _currentWindow = currentWindow;
            _frViewModel = frViewModel;
        }

        public override bool CanExecute(object? parameter)
        {
            return base.CanExecute(parameter);
        }
        public override void Execute(object? parameter)
        {
            _frViewModel.Response = Models.ScanStatus.Cancel;
            _currentWindow.Close();
        }
    }
}

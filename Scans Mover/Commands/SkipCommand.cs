using Avalonia.Controls;
using Scans_Mover.ViewModels;

namespace Scans_Mover.Commands
{
    public class SkipCommand : CommandBase
    {
        private readonly FileRenameViewModel _frViewModel;
        private readonly Window _currentWindow;
        public SkipCommand(Window currentWindow, FileRenameViewModel frViewModel)
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
            _frViewModel.Response = Models.ScanStatus.Skip;
            _currentWindow.Close();
        }
    }
}

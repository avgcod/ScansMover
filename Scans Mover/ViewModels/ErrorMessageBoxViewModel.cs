using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scans_Mover.Models;

namespace Scans_Mover.ViewModels
{
    public partial class ErrorMessageBoxViewModel : ViewModelBase
    {
        private readonly Window _currentWindow;

        [ObservableProperty]
        private string _errorType = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public ErrorMessageBoxViewModel(Window theWindow, OperationErrorMessage theMessage)
        {
            _currentWindow = theWindow;
            _errorType = theMessage.ErrorType;
            _errorMessage = theMessage.ErrorMessage;
        }

        [RelayCommand]
        public void OK()
        {
            _currentWindow.Close();
        }

    }
}

using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;

namespace Scans_Mover.ViewModels
{
    public partial class ErrorMessageBoxViewModel(Window theWindow, IMessenger theMessenger) : ViewModelBase(theMessenger), IRecipient<OperationErrorInfoMessage>
    {
        private readonly Window _currentWindow = theWindow;

        [ObservableProperty]
        private string _errorType = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [RelayCommand]
        public void OK()
        {
            _currentWindow.Close();
        }

        public void Receive(OperationErrorInfoMessage message)
        {
            ErrorType = message.ErrorType;
            ErrorMessage = message.ErrorMessage;
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
    }
}

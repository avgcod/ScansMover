using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;
using Scans_Mover.Views;

namespace Scans_Mover.ViewModels
{
    public partial class ErrorMessageBoxViewModel : ViewModelBase, IRecipient<OperationErrorInfoMessage>
    {
        private readonly Window _currentWindow;

        [ObservableProperty]
        private string _errorType = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public ErrorMessageBoxViewModel(ErrorMessageBoxView theWindow, IMessenger theMessenger) : base(theMessenger)
        {
            _currentWindow = theWindow;
            _currentWindow.Closing += CurrentWindow_Closing;
            IsActive = true;
        }

        private void CurrentWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            IsActive = false;
        }

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

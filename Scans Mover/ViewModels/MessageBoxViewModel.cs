using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;

namespace Scans_Mover.ViewModels
{
    public partial class MessageBoxViewModel(Window currentWindow, IMessenger theMessenger) : ViewModelBase(theMessenger), IRecipient<NotificationMessage>
    {
        private readonly Window _currentWindow = currentWindow;

        [ObservableProperty]
        private string _messageText = string.Empty;

        [RelayCommand]
        public void OK()
        {
            _currentWindow.Close();
        }

        public void Receive(NotificationMessage message)
        {
            MessageText = message.MessageText;
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

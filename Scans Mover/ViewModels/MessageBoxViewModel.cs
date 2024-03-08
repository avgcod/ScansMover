using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;
using Scans_Mover.Views;

namespace Scans_Mover.ViewModels
{
    public partial class MessageBoxViewModel : ViewModelBase, IRecipient<NotificationMessage>
    {
        private readonly Window _currentWindow;

        [ObservableProperty]
        private string _messageText = string.Empty;

        public MessageBoxViewModel(MessageBoxView theWindow, IMessenger theMessenger) : base(theMessenger)
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

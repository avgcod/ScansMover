using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Scans_Mover.ViewModels
{
    public partial class MessageBoxViewModel : ObservableObject
    {
        private readonly Window _currentWindow;

        [ObservableProperty]
        private string _messageText = string.Empty;

        public MessageBoxViewModel(Window currentWindow,string messageText)
        {
            MessageText = messageText;
            _currentWindow = currentWindow;
        }

        [RelayCommand]
        public void OK()
        {
            _currentWindow.Close();
        }
    }
}

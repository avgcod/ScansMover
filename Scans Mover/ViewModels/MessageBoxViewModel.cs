using Avalonia.Controls;
using Scans_Mover.Commands;
using ReactiveUI;
using System.Windows.Input;

namespace Scans_Mover.ViewModels
{
    public  class MessageBoxViewModel : ReactiveObject
    {

        private string _messageText = string.Empty;
        public string MessageText
        {
            get
            {
                return _messageText;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _messageText,value);
            }
        }

        public ICommand OKCommand { get; }

        public MessageBoxViewModel(Window currentWindow,string messageText)
        {
            MessageText = messageText;
            OKCommand = new OKCommand(currentWindow);
        }
    }
}

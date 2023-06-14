using Avalonia.Controls;

namespace Scans_Mover.Commands
{
    public class OKCommand : CommandBase
    {
        private readonly Window _currentWindow;

        public OKCommand(Window currentWindow)
        {
            _currentWindow = currentWindow;
        }

        public override void Execute(object? parameter)
        {
            _currentWindow.Close();
        }
    }
}

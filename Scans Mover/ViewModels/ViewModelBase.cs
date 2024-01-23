using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Scans_Mover.ViewModels
{
    public class ViewModelBase(IMessenger theMessenger) : ObservableRecipient(theMessenger);
}

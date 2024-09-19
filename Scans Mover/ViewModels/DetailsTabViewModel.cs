using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;
using Scans_Mover.Views;
using System.Threading.Tasks;

namespace Scans_Mover.ViewModels
{
    public partial class DetailsTabViewModel : ViewModelBase, IRecipient<ScanTypeChangedMessage>
    {
        private readonly Window _parentWindow;

        [ObservableProperty]
        private bool _documentHasMinimum = false;

        [ObservableProperty]
        public double _documentMinimum = 1;

        [ObservableProperty]
        public int _pagesPerDocument = 0;

        [ObservableProperty]
        public string _prefix = string.Empty;

        [ObservableProperty]
        public string _prefixExample = string.Empty;

        [ObservableProperty]
        public double _tolerance = 75;

        public DetailsTabViewModel(MoverView parentWindow, IMessenger theMessenger) : base(theMessenger)
        {
            _parentWindow = parentWindow;
            IsActive = true;

            _parentWindow.Closing += ParentWindow_Closing;
        }

        private void ParentWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            IsActive = false;
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

        private async Task RequestInformation()
        {
            DocumentHasMinimum = await Messenger.Send<DocumentHasMinimumRequestMessage>();
            DocumentMinimum = await Messenger.Send<DocumentMinimumRequestMessage>();

            Prefix = await Messenger.Send<PrefixRequestMessage>();
            PrefixExample = Prefix + " batch";

            PagesPerDocument = await Messenger.Send<PagesPerDocumentRequestMessage>();

            Tolerance = await Messenger.Send<ToleranceRequestMessage>();
        }

        partial void OnDocumentMinimumChanged(double value)
        {
            Messenger.Send<DocumentMinimumMessage>(new DocumentMinimumMessage(value));
        }

        partial void OnPagesPerDocumentChanged(int value)
        {
            Messenger.Send<PagesPerDocumentMessage>(new PagesPerDocumentMessage(value));
        }

        partial void OnPrefixChanged(string value)
        {
            PrefixExample = Prefix + " batch";
            Messenger.Send<PrefixMessage>(new PrefixMessage(value));
        }

        partial void OnToleranceChanged(double value)
        {
            Messenger.Send<ToleranceMessage>(new ToleranceMessage(value));
        }

        public async void Receive(ScanTypeChangedMessage message)
        {
            await RequestInformation();
        }
    }
}

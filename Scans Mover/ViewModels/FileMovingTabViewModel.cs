using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Scans_Mover.Models;
using Scans_Mover.Services;
using Scans_Mover.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scans_Mover.ViewModels
{
    public partial class FileMovingTabViewModel(Window parentWindow, IMessenger theMessenger) : ViewModelBase(theMessenger), IRecipient<LocationsMessage>, IRecipient<PrefixMessage>
    {
        private readonly Window _parentWindow = parentWindow;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        private bool _busy = false;

        [ObservableProperty]
        private bool _documentHasDate = false;

        private bool _filesMoved = false;

        private bool _hasSkippedFiles = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        private string _mainFolder = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        private string _deliveriesFolder = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        public string _rMAsFolder = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        public string _shippingLogsFolder = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        public string _serviceFolder = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        public string _prefix = string.Empty;

        [ObservableProperty]
        private DateTime _specifiedDate = DateTime.Now.AddDays(-1);

        [ObservableProperty]
        private string _prefixExample = string.Empty;

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

        #region Commands
        public bool CanSplit => !Busy && !string.IsNullOrEmpty(MainFolder) && !string.IsNullOrEmpty(Prefix);

        public bool CanMove => !Busy && !string.IsNullOrEmpty(MainFolder) && !string.IsNullOrEmpty(DeliveriesFolder)
            && !string.IsNullOrEmpty(ShippingLogsFolder) && !string.IsNullOrEmpty(RMAsFolder)
            && !string.IsNullOrEmpty(ServiceFolder);

        [RelayCommand(CanExecute = nameof(CanSplit))]
        public async Task BatchSplit()
        {
            Busy = true;
            _hasSkippedFiles = false;

            //IEnumerable<string> pdfFileNames = await PDFSplitterService.SplitBatchPDFsAsync(this, Messenger);
            //await FileRenameService.RenamePDFsAsync(pdfFileNames, this, Messenger, _parentWindow);

            if (_hasSkippedFiles)
            {
                await DisplayMessageBoxAsync("Finished Splitting." + Environment.NewLine + "Review Skipped Files.");
            }
            else
            {
                await DisplayMessageBoxAsync("Finished Splitting.");
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanMove))]
        public async Task MoveDeliveries()
        {
            Busy = true;

            _filesMoved = false;

            //IEnumerable<string> filesNeedingFolders = await DocumentMoverService.MoveToFolderAsync(this, Messenger);

            //if (_filesMoved)
            //{
            //    if (filesNeedingFolders.Any())
            //    {
            //        await DisplayMessageBoxAsync("Finished Moving Files." + Environment.NewLine + "Some Files Need Folders.");
            //    }
            //    else
            //    {
            //        await DisplayMessageBoxAsync("Finished Moving Files.");
            //    }
            //}
            //else
            //{
            //    await DisplayMessageBoxAsync("No Files Found To Move or Folders Don't Exist for Found Files.");
            //}

            Busy = false;
        }
        #endregion

        /// <summary>
        /// Displays a simple message box with a specified message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <returns>Task</returns>
        private async Task DisplayMessageBoxAsync(string message)
        {
            MessageBoxView mboxView = new();
            MessageBoxViewModel mbvModel = new(mboxView, Messenger);
            mboxView.DataContext = mbvModel;
            mbvModel.IsActive = true;
            Messenger.Send(new NotificationMessage(message));
            await mboxView.ShowDialog(_parentWindow);
            mbvModel.IsActive = false;
        }

        public void Receive(LocationsMessage message)
        {
            MainFolder = message.MainFolder;
            DeliveriesFolder = message.DeliveriesFolder;
            RMAsFolder = message.RMAsFolder;
            ServiceFolder = message.ServiceFolder;
        }

        public void Receive(PrefixMessage message)
        {
            Prefix = message.Prefix;
            PrefixExample = Prefix + " batch";
        }
    }
}

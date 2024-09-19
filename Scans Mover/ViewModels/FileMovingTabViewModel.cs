using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Scans_Mover.Models;
using Scans_Mover.Services;
using Scans_Mover.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scans_Mover.ViewModels
{
    public partial class FileMovingTabViewModel : ViewModelBase, IRecipient<SkippedFileMessage>, IRecipient<MoveLogMessage>, IRecipient<ScanTypeChangedMessage>
    {
        #region Variables
        private double _documentMinimum;
        private bool _filesMoved = false;
        private readonly FileRenameService _fileRenameService;
        private bool _hasSkippedFiles = false;
        private readonly Window _parentWindow;
        private ScanType _selectedScanType;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        #endregion

        #region Properties
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        private bool _busy = false;

        [ObservableProperty]
        private bool _documentHasDate = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        private string _destinationFolder = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        private string _mainFolder = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        private string _prefix = string.Empty;

        [ObservableProperty]
        private DateTime _specifiedDate = DateTime.Now.AddDays(-1);

        [ObservableProperty]
        public double _tolerance = 75;
        #endregion

        public FileMovingTabViewModel(MoverView parentWindow, FileRenameService fileRenameService, IMessenger theMessenger, IServiceScopeFactory serviceScopeFactory) : base(theMessenger)
        {
            _fileRenameService = fileRenameService;
            _parentWindow = parentWindow;
            _serviceScopeFactory = serviceScopeFactory;
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

        #region Commands
        public bool CanSplit => !Busy && !string.IsNullOrEmpty(MainFolder) && !string.IsNullOrEmpty(Prefix);

        public bool CanMove => !Busy && !string.IsNullOrEmpty(MainFolder) && !string.IsNullOrEmpty(DestinationFolder);

        [RelayCommand(CanExecute = nameof(CanSplit))]
        public async Task BatchSplit()
        {
            Messenger.Send<BusyStatusMessage>(new BusyStatusMessage(true));
            _hasSkippedFiles = false;

            SplitSettings splitSettings = await CreateSplitSettings();

            IEnumerable<string> pdfFileNames = await SplitterService.SplitBatchDocumentsAsync(splitSettings, Messenger);

            RenameSettings renameSettings = new()
            {
                DocumentMinimum = _documentMinimum,
                Prefix = Prefix,
                SelectedScanType = _selectedScanType
            };

            await _fileRenameService.RenamePDFsAsync(pdfFileNames, renameSettings, _parentWindow);

            if (_hasSkippedFiles)
            {
                await DisplayMessageBoxAsync("Finished Splitting." + Environment.NewLine + "Review Skipped Files.");
            }
            else
            {
                await DisplayMessageBoxAsync("Finished Splitting.");
            }

            Messenger.Send<BusyStatusMessage>(new BusyStatusMessage(false));
        }

        [RelayCommand(CanExecute = nameof(CanMove))]
        public async Task MoveDeliveries()
        {
            Messenger.Send<BusyStatusMessage>(new BusyStatusMessage(true));

            _filesMoved = false;

            MoveSettings moveSettings = new()
            {
                Date = DateOnly.FromDateTime(SpecifiedDate),
                MainFolder = MainFolder,
                Prefix = Prefix,
                SelectedScanType = _selectedScanType,
                RootDestination = await GetRootDestination(_selectedScanType)
            };

            IEnumerable<string> filesNeedingFolders = await DocumentMoverService.MoveToFolderAsync(moveSettings, Messenger);

            if (_filesMoved)
            {
                if (filesNeedingFolders.Any())
                {
                    await DisplayMessageBoxAsync("Finished Moving Files." + Environment.NewLine + "Some Files Need Folders.");
                }
                else
                {
                    await DisplayMessageBoxAsync("Finished Moving Files.");
                }
            }
            else
            {
                await DisplayMessageBoxAsync("No Files Found To Move or Folders Don't Exist for Found Files.");
            }

            Messenger.Send<BusyStatusMessage>(new BusyStatusMessage(false));
        }
        #endregion

        /// <summary>
        /// Displays a simple message box with a specified message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <returns>Task</returns>
        private async Task DisplayMessageBoxAsync(string message)
        {
            using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
            MessageBoxView mboxView = serviceScope.ServiceProvider.GetRequiredService<MessageBoxView>();
            mboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<MessageBoxViewModel>();
            Messenger.Send(new NotificationMessage(message));
            mboxView.SizeToContent = SizeToContent.WidthAndHeight;
            await mboxView.ShowDialog(_parentWindow);
        }

        private async Task<string> GetRootDestination(ScanType selectedScanType)
        {
            return selectedScanType switch
            {
                ScanType.Delivery => await Messenger.Send<DeliveriesFolderRequestMessage>(),
                ScanType.RMA => await Messenger.Send<RMAsFolderRequestMessage>(),
                ScanType.Shipping => await Messenger.Send<ShippingLogsFolderRequestMessage>(),
                ScanType.PO => await Messenger.Send<DeliveriesFolderRequestMessage>(),
                ScanType.Service => await Messenger.Send<DeliveriesFolderRequestMessage>(),
                _ => throw new ArgumentException("Invalid ScanType", nameof(selectedScanType))
            };
        }

        private async Task<SplitSettings> CreateSplitSettings()
        {
            return new SplitSettings()
            {
                DocumentHasMinimum = await Messenger.Send<DocumentHasMinimumRequestMessage>(),
                DocumentMinimum = _documentMinimum,
                MainFolder = MainFolder,
                PagesPerDocument = await Messenger.Send<PagesPerDocumentRequestMessage>(),
                Prefix = Prefix,
                SelectedScanType = _selectedScanType,
                Tolerance = await Messenger.Send<ToleranceRequestMessage>()
            };
        }

        #region Message Handling
        /// <summary>
        /// Handles SkippedFileMessages that are sent.
        /// </summary>
        /// <param name="message">The RenameMessage that was sent.</param>
        public void Receive(SkippedFileMessage message)
        {
            _hasSkippedFiles = true;
        }

        /// <summary>
        /// Handles LogMessages that are sent.
        /// </summary>
        /// <param name="message">The LogMessage that was sent.</param>
        public async void Receive(MoveLogMessage message)
        {
            await HandleMoveLogMessage(message);
        }

        /// <summary>
        /// Processes LogMessage messages.
        /// </summary>
        /// <param name="message">The LogMessage to process.</param>
        private async Task HandleMoveLogMessage(MoveLogMessage message)
        {
            _filesMoved = true;
            await FileAccessService.SaveLogAsync(message.LogInfo, message.LogFile, Messenger);
            await FileAccessService.LoadDefaultApplicationAsync(message.LogFile, Messenger);
        }

        public async void Receive(ScanTypeChangedMessage message)
        {
            await RequestInformation();
        }
        #endregion

        private async Task RequestInformation()
        {
            DocumentHasDate = await Messenger.Send<DocumentHasDateRequestMessage>();
            _documentMinimum = await Messenger.Send<DocumentMinimumRequestMessage>();
            DestinationFolder = await Messenger.Send<DestinationFolderRequestMessage>();
            MainFolder = await Messenger.Send<MainFolderRequestMessage>();
            Prefix = await Messenger.Send<PrefixRequestMessage>();
            _selectedScanType = await Messenger.Send<SelectedScanTypeRequestMessage>();
        }
    }
}

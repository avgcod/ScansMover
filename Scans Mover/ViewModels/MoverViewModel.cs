using System;
using Avalonia.Controls;
using Scans_Mover.Models;
using Scans_Mover.Services;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using Scans_Mover.Views;
using System.Linq;
using UglyToad.PdfPig.Fonts.TrueType.Names;

namespace Scans_Mover.ViewModels
{
    public partial class MoverViewModel(Window parentWindow, IMessenger theMessenger, string settingsFile) : ObservableRecipient(theMessenger), IRecipient<RenameMessage>, IRecipient<MoveLogMessage>, IRecipient<PagesPerDocumentErrorMessage>, IRecipient<OperationErrorMessage>
    {
        #region Variables
        private readonly string _settingsFile = settingsFile;
        private readonly Window _parentWindow = parentWindow;
        #endregion

        #region Properties
        [ObservableProperty]
        public int _pagesPerDocument = 0;
        public Settings Settings { get; set; } = new Settings();
        public ScanStatus CurrentScanStatus = ScanStatus.OK;
        public string CurrentScanNewFileName = string.Empty;
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
        public double _tolerance = 75;
        [ObservableProperty]
        public double _documentMinimum = 1;
        [ObservableProperty]
        private ScanType _selectedScanType = ScanType.Shipping;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        private bool _busy = false;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DocumentMinimum))]
        private bool _documentHasMinimum = false;
        [ObservableProperty]
        private bool _documentHasDate = false;
        [ObservableProperty]
        private LocationType _locationType = LocationType.Scans;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PrefixExample))]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        public string _prefix = string.Empty;
        public string PrefixExample => Prefix + " batch";
        [ObservableProperty]
        private DateTime _specifiedDate = DateTime.Now.AddDays(-1);
        public bool HasSkippedFiles { get; set; } = false;
        public bool FilesMoved { get; set; } = false;
        [ObservableProperty]
        private string _processingText = "Processing. Please wait.";

        #endregion

        #region Commands
        public bool CanSplit => !Busy && !string.IsNullOrEmpty(MainFolder) && !string.IsNullOrEmpty(Prefix);
        public bool CanMove => !Busy && !string.IsNullOrEmpty(MainFolder) && !string.IsNullOrEmpty(DeliveriesFolder)
            && !string.IsNullOrEmpty(ShippingLogsFolder) && !string.IsNullOrEmpty(RMAsFolder)
            && !string.IsNullOrEmpty(ServiceFolder);

        [RelayCommand]
        public async Task ChangeLocation()
        {
            IStorageFolder? selectedFolder = await FileAccessService.ChooseLocationAsync(_parentWindow, Messenger);

            if (selectedFolder != null)
            {
                if (selectedFolder.CanBookmark)
                {
                    if (LocationType == LocationType.Scans)
                    {
                        MainFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                    else if (LocationType == LocationType.Deliveries)
                    {
                        DeliveriesFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                    else if (LocationType == LocationType.Shipping)
                    {
                        ShippingLogsFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                    else if (LocationType == LocationType.RMAs)
                    {
                        RMAsFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                    else
                    {
                        ServiceFolder = await selectedFolder.SaveBookmarkAsync() ?? string.Empty;
                    }
                }
            }
        }
        [RelayCommand]
        public void SplitTypeChecked(object? parameter)
        {
            string typeText = parameter?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(typeText))
            {
                SavePrefix();
                SaveDocumentMinimum();
                SavePagesPerDocument();

                SelectedScanType = (ScanType)Enum.Parse(typeof(ScanType), typeText);

                ChangeHasDate();
                ChangeHasMinimum();
                ChangePrefix();
                ChangeHasMinimum();
                ChangeDocumentMinimum();
                ChangePagesPerDocument();
            }
        }
        [RelayCommand]
        public void LocationChecked(object? parameter)
        {
            string typeText = parameter?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(typeText))
            {
                LocationType = (LocationType)Enum.Parse(typeof(LocationType), typeText);
            }
        }
        [RelayCommand(CanExecute = nameof(CanSplit))]
        public async Task BatchSplit()
        {
            Busy = true;
            HasSkippedFiles = false;

            IEnumerable<string> pdfFileNames = await PDFSplitterService.SplitBatchPDFsAsync(this, Messenger);
            await FileRenameService.RenamePDFsAsync(pdfFileNames, this, Messenger, _parentWindow);

            if (HasSkippedFiles)
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

            FilesMoved = false;

            IEnumerable<string> filesNeedingFolders = await DocumentMoverService.MoveToFolderAsync(this, Messenger);

            if (FilesMoved)
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

            Busy = false;
        }
        #endregion

        protected override async void OnActivated()
        {
            Messenger.RegisterAll(this);
            await LoadSettingsAsync();
            base.OnActivated();
        }

        protected override async void OnDeactivated()
        {
            Messenger.UnregisterAll(this);
            await SaveSettingsAsync();
            base.OnDeactivated();
        }

        /// <summary>
        /// Loads the program settings.
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            Busy = true;

            Settings = await FileAccessService.LoadSettingsAsync(_settingsFile, Messenger);

            Busy = false;

            UpdateProperties();
        }

        #region Save Methods
        /// <summary>
        /// Sets the appropriate Settings object PagesPerDocument property based on the SelectedScanType property.
        /// </summary>
        private void SavePagesPerDocument()
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                Settings.DeliveriesPagesPerDocument = PagesPerDocument;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                Settings.RMAsPagesPerDocument = PagesPerDocument;
            }
            else if (SelectedScanType == ScanType.Shipping)
            {
                Settings.ShippingLogsPagesPerDocument = PagesPerDocument;
            }
            else if (SelectedScanType == ScanType.PO)
            {
                Settings.POsPagesPerDocument = PagesPerDocument;
            }
            else
            {
                Settings.ServicePagesPerDocument = PagesPerDocument;
            }
        }

        /// <summary>
        /// Sets the appropriate Settings object Prefix property based on the SelectedScanType property.
        /// </summary>
        private void SavePrefix()
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                Settings.DeliveriesPrefix = Prefix;
            }
            else if (SelectedScanType == ScanType.Shipping)
            {
                Settings.ShippingLogsPrefix = Prefix;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                Settings.RMAsPrefix = Prefix;
            }
            else if (SelectedScanType == ScanType.PO)
            {
                Settings.POsPrefix = Prefix;
            }
            else
            {
                Settings.ServicePrefix = Prefix;
            }
        }

        /// <summary>
        /// Sets the appropriate Settings object DeliveriesMinimum property based on the SelectedScanType property.
        /// </summary>
        private void SaveDocumentMinimum()
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                Settings.DeliveriesMinimum = DocumentMinimum;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                Settings.RMAsMinimum = DocumentMinimum;
            }
        }

        /// <summary>
        /// Saves the Settings property to a file.
        /// </summary>
        /// <returns>If the operation succeeded.</returns>
        public async Task SaveSettingsAsync()
        {
            await FileAccessService.SaveSettingsAsync(Settings, _settingsFile, Messenger);
        }
        #endregion

        #region Change Methods
        /// <summary>
        /// Changes the DocumentHasMinimum property based on the SelectedScanType.
        /// </summary>
        private void ChangeHasMinimum()
        {
            DocumentHasMinimum = SelectedScanType == ScanType.Delivery || SelectedScanType == ScanType.RMA;
        }

        /// <summary>
        /// Changes the PagesPerDocument property based on the SelectedScanType property.
        /// </summary>
        private void ChangePagesPerDocument()
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                PagesPerDocument = Settings.DeliveriesPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                PagesPerDocument = Settings.RMAsPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.Shipping)
            {
                PagesPerDocument = Settings.ShippingLogsPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.PO)
            {
                PagesPerDocument = Settings.POsPagesPerDocument;
            }
            else
            {
                PagesPerDocument = Settings.ServicePagesPerDocument;
            }
        }

        /// <summary>
        /// Changed the DocumentHasDate property based on the SelectedScanType property.
        /// </summary>
        private void ChangeHasDate()
        {
            DocumentHasDate = SelectedScanType == ScanType.Delivery;
        }

        /// <summary>
        /// Changes the Prefix property based on the SelectedScanType property.
        /// </summary>
        private void ChangePrefix()
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                Prefix = Settings.DeliveriesPrefix;
            }
            else if (SelectedScanType == ScanType.Shipping)
            {
                Prefix = Settings.ShippingLogsPrefix;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                Prefix = Settings.RMAsPrefix;
            }
            else if (SelectedScanType == ScanType.PO)
            {
                Prefix = Settings.POsPrefix;
            }
            else
            {
                Prefix = Settings.ServicePrefix;
            }
        }

        /// <summary>
        /// Changes the DocumentMinimum property based on the SelectedScanType property.
        /// </summary>
        private void ChangeDocumentMinimum()
        {
            if (SelectedScanType == ScanType.RMA)
            {
                DocumentMinimum = Settings.RMAsMinimum;
            }
            else if (SelectedScanType == ScanType.Delivery)
            {
                DocumentMinimum = Settings.DeliveriesMinimum;
            }
        }
        #endregion

        /// <summary>
        /// Updates all observable properties.
        /// </summary>
        private void UpdateProperties()
        {
            if (!string.IsNullOrEmpty(Settings.MainFolder))
            {
                MainFolder = Settings.MainFolder;
                DeliveriesFolder = Settings.DeliveriesFolder;
                RMAsFolder = Settings.RMAsFolder;
                ShippingLogsFolder = Settings.ShippingLogsFolder;
                ServiceFolder = Settings.ServiceFolder;
                Tolerance = Settings.Tolerance;
                Prefix = Settings.DeliveriesPrefix;
            }
            else
            {
                ChangePrefix();
            }

            SelectedScanType = ScanType.Delivery;
            ChangeHasDate();
            ChangePagesPerDocument();
            ChangeHasMinimum();
            ChangeDocumentMinimum();
        }

        #region Message Handling
        /// <summary>
        /// Displays a message box with message information.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <returns>Task</returns>
        private async Task HandleOperationErrorMessage(OperationErrorMessage message)
        {
            ErrorMessageBoxView emboxView = new();
            ErrorMessageBoxViewModel embvModel = new (emboxView, Messenger);
            emboxView.DataContext = embvModel;
            embvModel.IsActive = true;
            Messenger.Send(new OperationErrorInfoMessage(message.ErrorType, message.ErrorMessage));
            await emboxView.ShowDialog(_parentWindow);
            embvModel.IsActive = false;
        }

        /// <summary>
        /// Processes RenameMessage messages.
        /// </summary>
        /// <param name="message">The RenameMessage to process.</param>
        private void HandleRenameMessage(RenameMessage message)
        {
            CurrentScanStatus = message.ScanStatus;
            if (CurrentScanStatus == ScanStatus.OK)
            {
                CurrentScanNewFileName = message.NewFileName;
            }
            else if (CurrentScanStatus == ScanStatus.Skip)
            {
                HasSkippedFiles = true;
            }
        }

        /// <summary>
        /// Processes LogMessage messages.
        /// </summary>
        /// <param name="message">The LogMessage to process.</param>
        private async Task HandleMoveLogMessage(MoveLogMessage message)
        {
            FilesMoved = true;
            await FileAccessService.SaveLogAsync(message.LogInfo, message.LogFile, Messenger);
            await FileAccessService.LoadDefaultApplicationAsync(message.LogFile, Messenger);
        }

        /// <summary>
        /// Processes PagesPerDocumentErrorMessage messages.
        /// </summary>
        /// <param name="message">The PagesPerDocumentErrorMessage to process.</param>
        private async Task HandlePagesPerDocumentErrorMessage(PagesPerDocumentErrorMessage message)
        {
            await DisplayMessageBoxAsync(message.Info);
        }

        /// <summary>
        /// Handles RenameMessages that are sent.
        /// </summary>
        /// <param name="message">The RenameMessage that was sent.</param>
        public void Receive(RenameMessage message)
        {
            HandleRenameMessage(message);
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
        /// Handles PagesPerDocumentErrorMessage that are sent.
        /// </summary>
        /// <param name="message">The PagesPerDocumentErrorMessage that was sent.</param>
        public async void Receive(PagesPerDocumentErrorMessage message)
        {
            await HandlePagesPerDocumentErrorMessage(message);
        }

        /// <summary>
        /// Handles OperationErrorMessage that are sent.
        /// </summary>
        /// <param name="message">The OperationErrorMessage that was sent.</param>
        public async void Receive(OperationErrorMessage message)
        {
            await HandleOperationErrorMessage(message);
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
            MessageBoxViewModel mbvModel = new (mboxView, Messenger);
            mboxView.DataContext = mbvModel;
            mbvModel.IsActive = true;
            Messenger.Send(new NotificationMessage(message));
            await mboxView.ShowDialog(_parentWindow);
            mbvModel.IsActive = false;
        }
    }
}

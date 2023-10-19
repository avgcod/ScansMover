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

namespace Scans_Mover.ViewModels
{
    public partial class MoverViewModel : ObservableObject, IRecipient<RenameMessage>, IRecipient<LogMessage>, IRecipient<PagesPerDocumentErrorMessage>
    {
        #region Variables
        private readonly string _settingsFile;
        private readonly Window _parentWindow;
        private readonly IMessenger _theMessenger;
        #endregion

        #region Properties
        private bool _filesMoved = false;
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
        public double _documentMinimum = 0;
        [ObservableProperty]
        private ScanType _selectedScanType = ScanType.Shipping;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveDeliveriesCommand))]
        private bool _busy = true;
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
        private string PrefixExample => Prefix + " batch";

        [ObservableProperty]
        private DateTime _specifiedDate = DateTime.Now.AddDays(-1);
        public bool HasSkippedFiles { get; set; } = false;
        #endregion

        public MoverViewModel(Window parentWindow, IMessenger theMessenger, string settingsFile)
        {
            _parentWindow = parentWindow;
            _theMessenger = theMessenger;
            _settingsFile = settingsFile;


            _theMessenger.Register<RenameMessage>(this);
            _theMessenger.Register<LogMessage>(this);
            _theMessenger.Register<PagesPerDocumentErrorMessage>(this);
            _parentWindow.Opened += OnWindowOpened;
            _parentWindow.Closing += OnWindowClosing;
        }

        #region Commands
        public bool CanSplit => !Busy && !string.IsNullOrEmpty(MainFolder) && !string.IsNullOrEmpty(Prefix);
        public bool CanMove => !Busy && !string.IsNullOrEmpty(MainFolder) && !string.IsNullOrEmpty(DeliveriesFolder)
            && !string.IsNullOrEmpty(ShippingLogsFolder) && !string.IsNullOrEmpty(RMAsFolder)
            && !string.IsNullOrEmpty(ServiceFolder);

        [RelayCommand]
        public async Task ChangeLocation()
        {
            IStorageFolder? selectedFolder = await FileAccessService.ChooseLocationAsync(_parentWindow);

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
                SaveDocumentMinimum();


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

            IEnumerable<string> pdfFileNames = await PDFSplitterService.SplitBatchPDFsAsync(this, _theMessenger);
            await FileRenameService.RenamePDFsAsync(pdfFileNames, this, _theMessenger, _parentWindow);

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
            _filesMoved = false;

            IEnumerable<string> filesNeedingFolders = await DocumentMoverService.MoveToFolderAsync(this, _theMessenger, MainFolder);

            if (filesNeedingFolders.Any())
            {
                await DisplayMessageBoxAsync("Finished Moving Files." + Environment.NewLine + "Some Files Need Folders.");
            }
            else
            {
                await DisplayMessageBoxAsync("Finished Moving Files.");
            }

            Busy = false;
        }

        #endregion

        /// <summary>
        /// Loads the program settings.
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            Settings = await FileAccessService.LoadSettingsAsync(_settingsFile);

            if (!string.IsNullOrEmpty(Settings.MainFolder))
            {
                UpdateProperties();
            }
        }

        /// <summary>
        /// Loads all information when the window is opened.
        /// </summary>
        /// <param name="sender">Window sender.</param>
        /// <param name="e">Event arguments.</param>
        public async void OnWindowOpened(object? sender, EventArgs e)
        {
            await LoadSettingsAsync();
        }

        /// <summary>
        /// Saves all information when the window is closed.
        /// </summary>
        /// <param name="sender">Window sender.</param>
        /// <param name="e">Cancel event arguments.</param>
        public async void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _theMessenger.UnregisterAll(this);
            UpdateSettingsFolders();
            await SaveSettingsAsync();
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
        public async Task<bool> SaveSettingsAsync()
        {
            return await FileAccessService.SaveSettingsAsync(Settings, _settingsFile);
        }

        #endregion

        #region Change Methods
        /// <summary>
        /// Changes the DocumentHasMinimum property based on the SelectedScanType.
        /// </summary>
        private void ChangeHasMinimum()
        {
            if (SelectedScanType == ScanType.Delivery || SelectedScanType == ScanType.RMA)
            {
                DocumentHasMinimum = true;
            }
            else
            {
                DocumentHasMinimum = false;
            }
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
            if (SelectedScanType == ScanType.Delivery)
            {
                DocumentHasDate = true;
            }
            else
            {
                DocumentHasDate = false;
            }
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

        #region Update Methods
        /// <summary>
        /// Updates the folders on the Settings object to be what is current on the folder properties on the view model.
        /// </summary>
        private void UpdateSettingsFolders()
        {
            Settings.MainFolder = MainFolder;
            Settings.DeliveriesFolder = DeliveriesFolder;
            Settings.RMAsFolder = RMAsFolder;
            Settings.ShippingLogsFolder = ShippingLogsFolder;
            Settings.ServiceFolder = ServiceFolder;
        }
        /// <summary>
        /// Updates all observable properties.
        /// </summary>
        private void UpdateProperties()
        {
            MainFolder = Settings.MainFolder;
            DeliveriesFolder = Settings.DeliveriesFolder;
            RMAsFolder = Settings.RMAsFolder;
            ShippingLogsFolder = Settings.ShippingLogsFolder;
            ServiceFolder = Settings.ServiceFolder;
            SelectedScanType = ScanType.Delivery;
            Tolerance = Settings.Tolerance;
            ChangePrefix();
            ChangeHasDate();
            ChangeHasMinimum();
            ChangePagesPerDocument();
            ChangeDocumentMinimum();
            ChangeHasMinimum();
            Busy = false;

        }

        #endregion

        #region Message Handling
        /// <summary>
        /// Processes RenameMessage messages.
        /// </summary>
        /// <param name="theMessage">The RenameMessage to process.</param>
        private void HandleRenameMessage(RenameMessage theMessage)
        {
            CurrentScanStatus = theMessage.scanStatus;
            if (theMessage.scanStatus == ScanStatus.OK)
            {

                CurrentScanNewFileName = theMessage.newFileName;

            }
            else if (theMessage.scanStatus == ScanStatus.Skip)
            {
                HasSkippedFiles = true;
            }
        }
        /// <summary>
        /// Processes LogMessage messages.
        /// </summary>
        /// <param name="theMessage">The LogMessage to process.</param>
        private async Task HandleLogMessage(LogMessage theMessage)
        {
            _filesMoved = true;
            await FileAccessService.SaveLogAsync(theMessage.logInfo, theMessage.logFile);
            await FileAccessService.LoadDefaultApplicationAsync(theMessage.logFile);
        }
        /// <summary>
        /// Processes PagesPerDocumentErrorMessage messages.
        /// </summary>
        /// <param name="theMessage">The PagesPerDocumentErrorMessage to process.</param>
        private async Task HandlePagesPerDocumentErrorMessage(PagesPerDocumentErrorMessage theMessage)
        {
            await DisplayMessageBoxAsync(theMessage.info);
        }

        /// <summary>
        /// Handles RenameMessages that are sent.
        /// </summary>
        /// <param name="theMessage">The RenameMessage that was sent.</param>        
        public void Receive(RenameMessage theMessage)
        {
            HandleRenameMessage(theMessage);
        }

        /// <summary>
        /// Handles LogMessages that are sent.
        /// </summary>
        /// <param name="theMessage">The LogMessage that was sent.</param>
        public async void Receive(LogMessage theMessage)
        {
            await HandleLogMessage(theMessage);
        }

        /// <summary>
        /// Handles PagesPerDocumentErrorMessage that are sent.
        /// </summary>
        /// <param name="theMessage"></param>
        public async void Receive(PagesPerDocumentErrorMessage theMessage)
        {
            await HandlePagesPerDocumentErrorMessage(theMessage);
        }
        #endregion

        /// <summary>
        /// Displays a simple message box with a specified message.
        /// </summary>
        /// <param name="theMessage">The message to show</param>
        /// <returns>Task</returns>
        private async Task DisplayMessageBoxAsync(string theMessage)
        {
            MessageBoxView mboxView = new MessageBoxView();

            mboxView.DataContext = new MessageBoxViewModel(mboxView, theMessage);

            await mboxView.ShowDialog(_parentWindow);
        }

    }
}

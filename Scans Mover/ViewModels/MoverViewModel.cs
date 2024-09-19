using Avalonia.Controls;
using Avalonia.Platform.Storage;
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
    public partial class MoverViewModel : ObservableRecipient, IRecipient<PagesPerDocumentErrorMessage>, IRecipient<OperationErrorMessage>,
        IRecipient<DocumentMinimumMessage>, IRecipient<PagesPerDocumentMessage>, IRecipient<PrefixMessage>, IRecipient<ToleranceMessage>,
        IRecipient<MainFolderMessage>, IRecipient<DeliveriesFolderMessage>, IRecipient<RMAsFolderMessage>, IRecipient<ServiceFolderMessage>,
        IRecipient<ShippingLogsFolderMessage>, IRecipient<BusyStatusMessage>, IRecipient<MoveLogMessage>
    {
        #region Variables
        private readonly string _settingsFile;
        private readonly Window _parentWindow;
        private readonly FileRenameService _fileRenameService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        #endregion

        #region Properties
        //public FileMovingTabViewModel FileMovingTabVM { get; private set; }
        //public DetailsTabViewModel DetailsTabVM { get; private set; }
        //public LocationsTabViewModel LocationsTabVM { get; private set; }
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
        [NotifyCanExecuteChangedFor(nameof(BatchSplitCommand))]
        public string _prefix = string.Empty;
        [ObservableProperty]
        public string _prefixExample = string.Empty;
        [ObservableProperty]
        private DateTime _specifiedDate = DateTime.Now.AddDays(-1);
        public bool HasSkippedFiles { get; set; } = false;
        public bool FilesMoved { get; set; } = false;
        [ObservableProperty]
        private string _processingText = "Processing. Please wait.";

        #endregion //FileMovingTabViewModel fileMovingTabVM, DetailsTabViewModel detailsTabVM, LocationsTabViewModel locationsTabVM

        public MoverViewModel(MoverView parentWindow, IMessenger theMessenger, IServiceScopeFactory serviceScopeFactory, FileRenameService fileRenameService) : base(theMessenger)
        {
            _settingsFile = "settings.json";
            _parentWindow = parentWindow;
            _fileRenameService = fileRenameService;
            _serviceScopeFactory = serviceScopeFactory;
            //FileMovingTabVM = fileMovingTabVM;
            //DetailsTabVM = detailsTabVM;
            //LocationsTabVM = locationsTabVM;

            IsActive = true;

            _parentWindow.Closing += ParentWindow_Closing;
        }

        private void RegisterRequestMessageHandling()
        {
            Messenger.Register<MoverViewModel, RenameServiceRequestMessage>(this, (receiver, message) => message.Reply(new FileRenameService(receiver.Messenger)));
            Messenger.Register<MoverViewModel, ParentWindowRequestMessage>(this, (receiver, message) => message.Reply(receiver._parentWindow));
            Messenger.Register<MoverViewModel, MessengerRequestMessage>(this, (receiver, message) => message.Reply(receiver.Messenger));

            Messenger.Register<MoverViewModel, MainFolderRequestMessage>(this, (receiver, message) => message.Reply(receiver.Settings.MainFolder));
            Messenger.Register<MoverViewModel, DeliveriesFolderRequestMessage>(this, (receiver, message) => message.Reply(receiver.Settings.DeliveriesFolder));
            Messenger.Register<MoverViewModel, RMAsFolderRequestMessage>(this, (receiver, message) => message.Reply(receiver.Settings.RMAsFolder));
            Messenger.Register<MoverViewModel, ServiceFolderRequestMessage>(this, (receiver, message) => message.Reply(receiver.Settings.ServiceFolder));
            Messenger.Register<MoverViewModel, ShippingLogsFolderRequestMessage>(this, (receiver, message) => message.Reply(receiver.Settings.ShippingLogsFolder));
            Messenger.Register<MoverViewModel, DestinationFolderRequestMessage>(this, (receiver, message) => message.Reply(receiver.GetRootDestination()));

            Messenger.Register<MoverViewModel, PrefixRequestMessage>(this, (receiver, message) => message.Reply(receiver.GetPrefix()));
            Messenger.Register<MoverViewModel, DocumentMinimumRequestMessage>(this, (receiver, message) => message.Reply(receiver.GetDocumentMinimum()));
            Messenger.Register<MoverViewModel, DocumentHasDateRequestMessage>(this, (receiver, message) => message.Reply(receiver.GetDocumentHasDate()));
            Messenger.Register<MoverViewModel, DocumentHasMinimumRequestMessage>(this, (receiver, message) => message.Reply(receiver.GetDocumentHasMinimum()));
            Messenger.Register<MoverViewModel, ToleranceRequestMessage>(this, (receiver, message) => message.Reply(receiver.Settings.Tolerance));
            Messenger.Register<MoverViewModel, PagesPerDocumentRequestMessage>(this, (receiver, message) => message.Reply(receiver.GetPagesPerDocument()));
            Messenger.Register<MoverViewModel, SelectedScanTypeRequestMessage>(this, (receiver, message) => message.Reply(receiver.SelectedScanType));
        }

        private void ParentWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            IsActive = false;
        }

        private string GetPrefix()
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                return Settings.DeliveriesPrefix;
            }
            else if (SelectedScanType == ScanType.Shipping)
            {
                return Settings.ShippingLogsPrefix;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                return Settings.RMAsPrefix;
            }
            else if (SelectedScanType == ScanType.PO)
            {
                return Settings.POsPrefix;
            }
            else
            {
                return Settings.ServicePrefix;
            }
        }

        private bool GetDocumentHasDate()
        {
            return SelectedScanType == ScanType.Delivery;
        }

        private bool GetDocumentHasMinimum()
        {
            return SelectedScanType == ScanType.Delivery || SelectedScanType == ScanType.RMA;
        }

        private double GetDocumentMinimum()
        {
            if(SelectedScanType == ScanType.Delivery)
            {
                return Settings.DeliveriesMinimum;
            }
            else if(SelectedScanType == ScanType.RMA)
            {
                return Settings.RMAsMinimum;
            }
            else
            {
                return -1;
            }
        }

        private int GetPagesPerDocument()
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                return Settings.DeliveriesPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                return Settings.RMAsPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.Shipping)
            {
                return Settings.ShippingLogsPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.PO)
            {
                return Settings.POsPagesPerDocument;
            }
            else
            {
                return Settings.ServicePagesPerDocument;
            }
        }

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

                    SaveFolders();
                }
            }
        }

        [RelayCommand]
        public void SplitTypeChecked(object? parameter)
        {
            string typeText = parameter?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(typeText))
            {
                SavePrefix(Prefix);
                SaveDocumentMinimum(DocumentMinimum);
                SavePagesPerDocument(PagesPerDocument);

                SelectedScanType = (ScanType)Enum.Parse(typeof(ScanType), typeText);

                ChangeHasDate();
                ChangeHasMinimum();
                ChangePrefix();
                ChangeHasMinimum();
                ChangeDocumentMinimum();
                ChangePagesPerDocument();

                Messenger.Send<ScanTypeChangedMessage>();
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

            SplitSettings splitSettings = new()
            {
                DocumentHasMinimum = DocumentHasMinimum,
                DocumentMinimum = DocumentMinimum,
                MainFolder = MainFolder,
                PagesPerDocument = PagesPerDocument,
                Prefix = Prefix,
                SelectedScanType = SelectedScanType,
                Tolerance = Tolerance
            };

            IEnumerable<string> pdfFileNames = await SplitterService.SplitBatchDocumentsAsync(splitSettings, Messenger);

            RenameSettings renameSettings = new()
            {
                DocumentMinimum = DocumentMinimum,
                Prefix = Prefix,
                SelectedScanType = SelectedScanType
            };

            await _fileRenameService.RenamePDFsAsync(pdfFileNames, renameSettings, _parentWindow);

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

            MoveSettings moveSettings = new()
            {
                Date = DateOnly.FromDateTime(SpecifiedDate),
                MainFolder = Settings.MainFolder,
                Prefix = Prefix,
                SelectedScanType = SelectedScanType,
                RootDestination = GetRootDestination()
            };

            IEnumerable<string> filesNeedingFolders = await DocumentMoverService.MoveToFolderAsync(moveSettings, Messenger);

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

            RegisterRequestMessageHandling();

            await LoadSettingsAsync();

            SelectedScanType = ScanType.Delivery;

            base.OnActivated();
        }

        protected override async void OnDeactivated()
        {
            Messenger.UnregisterAll(this);

            await SaveSettingsAsync();

            base.OnDeactivated();
        }

        private string GetRootDestination()
        {
            return SelectedScanType switch
            {
                ScanType.Delivery => Settings.DeliveriesFolder,
                ScanType.RMA => Settings.RMAsFolder,
                ScanType.Shipping => Settings.ShippingLogsFolder,
                ScanType.PO => Settings.DeliveriesFolder,
                ScanType.Service => Settings.ServiceFolder,
                _ => throw new ArgumentException(),
            };
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
        private void SavePagesPerDocument(int newPagesPerDocument)
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                Settings.DeliveriesPagesPerDocument = newPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                Settings.RMAsPagesPerDocument = newPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.Shipping)
            {
                Settings.ShippingLogsPagesPerDocument = newPagesPerDocument;
            }
            else if (SelectedScanType == ScanType.PO)
            {
                Settings.POsPagesPerDocument = newPagesPerDocument;
            }
            else
            {
                Settings.ServicePagesPerDocument = newPagesPerDocument;
            }
        }

        /// <summary>
        /// Sets the appropriate Settings object Prefix property based on the SelectedScanType property.
        /// </summary>
        private void SavePrefix(string newPrefix)
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                Settings.DeliveriesPrefix = newPrefix;
            }
            else if (SelectedScanType == ScanType.Shipping)
            {
                Settings.ShippingLogsPrefix = newPrefix;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                Settings.RMAsPrefix = newPrefix;
            }
            else if (SelectedScanType == ScanType.PO)
            {
                Settings.POsPrefix = newPrefix;
            }
            else
            {
                Settings.ServicePrefix = newPrefix;
            }
        }

        /// <summary>
        /// Sets the appropriate Settings object DeliveriesMinimum property based on the SelectedScanType property.
        /// </summary>
        private void SaveDocumentMinimum(double newDocumentMinimum)
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                Settings.DeliveriesMinimum = newDocumentMinimum;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                Settings.RMAsMinimum = newDocumentMinimum;
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

        /// <summary>
        /// Saves the current folders to the Settings object.
        /// </summary>
        private void SaveFolders()
        {
            Settings.MainFolder = MainFolder;
            Settings.RMAsFolder = RMAsFolder;
            Settings.DeliveriesFolder = DeliveriesFolder;
            Settings.ServiceFolder = ServiceFolder;
            Settings.ShippingLogsFolder = ShippingLogsFolder;
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

            PrefixExample = Prefix + " batch";
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
                PrefixExample = Prefix + " batch";
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
        /// Handles SkippedFileMessages that are sent.
        /// </summary>
        /// <param name="message">The RenameMessage that was sent.</param>
        public void Receive(SkippedFileMessage message)
        {
            HasSkippedFiles = true;
        }

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
            FilesMoved = true;
            await FileAccessService.SaveLogAsync(message.LogInfo, message.LogFile, Messenger);
            await FileAccessService.LoadDefaultApplicationAsync(message.LogFile, Messenger);
        }

        /// <summary>
        /// Handles OperationErrorMessage that are sent.
        /// </summary>
        /// <param name="message">The OperationErrorMessage that was sent.</param>
        public async void Receive(OperationErrorMessage message)
        {
            await HandleOperationErrorMessage(message);
        }

        /// <summary>
        /// Displays a message box with message information.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <returns>Task</returns>
        private async Task HandleOperationErrorMessage(OperationErrorMessage message)
        {
            using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
            ErrorMessageBoxView emboxView = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxView>();
            //ErrorMessageBoxViewModel embvModel = new(emboxView, Messenger);
            emboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxViewModel>();
            Messenger.Send<OperationErrorInfoMessage>(new OperationErrorInfoMessage(message.ErrorType, message.ErrorMessage));
            emboxView.SizeToContent = SizeToContent.WidthAndHeight;
            await emboxView.ShowDialog(_parentWindow);
        }

        /// <summary>
        /// Handles PagesPerDocumentErrorMessage that are sent.
        /// </summary>
        /// <param name="message">The PagesPerDocumentErrorMessage that was sent.</param>
        public async void Receive(PagesPerDocumentErrorMessage message)
        {
            await DisplayMessageBoxAsync(message.Info);
        }

        public void Receive(PagesPerDocumentMessage message)
        {
            SavePagesPerDocument(message.PagesPerDocument);
        }

        public void Receive(DocumentMinimumMessage message)
        {
            SaveDocumentMinimum(message.DocumentMinimum);
        }

        public void Receive(PrefixMessage message)
        {
            SavePrefix(message.Prefix);
        }

        public void Receive(ToleranceMessage message)
        {
            Settings.Tolerance = message.Tolerance;
        }

        public void Receive(MainFolderMessage message)
        {
            Settings.MainFolder = message.MainFolder;
        }

        public void Receive(DeliveriesFolderMessage message)
        {
            Settings.DeliveriesFolder = message.DeliveriesFolder;
        }

        public void Receive(RMAsFolderMessage message)
        {
            Settings.RMAsFolder = message.RMAsFolder;
        }

        public void Receive(ServiceFolderMessage message)
        {
            Settings.ServiceFolder = message.ServiceFolder;
        }

        public void Receive(ShippingLogsFolderMessage message)
        {
            Settings.ShippingLogsFolder = message.ShippingLogsFolder;
        }

        public void Receive(BusyStatusMessage message)
        {
            Busy = message.Busy;
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
            //MessageBoxViewModel mbvModel = new(mboxView, Messenger);
            mboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<MessageBoxViewModel>();
            Messenger.Send(new NotificationMessage(message));
            mboxView.SizeToContent = SizeToContent.WidthAndHeight;
            await mboxView.ShowDialog(_parentWindow);
        }

        /// <summary>
        /// Handle when the DocumentMinium has changed.
        /// </summary>
        /// <param name="value">The new DocumentMinium value.</param>
        partial void OnDocumentMinimumChanged(double value)
        {
            Messenger.Send<DocumentMinimumMessage>(new DocumentMinimumMessage(value));
        }

        /// <summary>
        /// Handle when the SelectedScanType has changed.
        /// </summary>
        /// <param name="value">The new SelectedScanType value.</param>
        partial void OnSelectedScanTypeChanged(ScanType value)
        {
            Messenger.Send<ScanTypeChangedMessage>(new ScanTypeChangedMessage());
        }
    }
}

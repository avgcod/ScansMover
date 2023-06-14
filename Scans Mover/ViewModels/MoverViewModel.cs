using System;
using Avalonia.Controls;
using Scans_Mover.Models;
using Scans_Mover.Services;
using System.Windows.Input;
using Scans_Mover.Commands;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Scans_Mover.ViewModels
{
    public partial class MoverViewModel : ObservableObject
    {
        #region Properties
        [ObservableProperty]
        public string _moveLog = string.Empty;
        public int PagesPerDocument
        {
            get
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
            set
            {
                if (PagesPerDocument != value)
                {
                    if (SelectedScanType == ScanType.Delivery)
                    {
                        Settings.DeliveriesPagesPerDocument = value;
                    }
                    else if (SelectedScanType == ScanType.RMA)
                    {
                        Settings.RMAsPagesPerDocument = value;
                    }
                    else if (SelectedScanType == ScanType.Shipping)
                    {
                        Settings.ShippingLogsPagesPerDocument = value;
                    }
                    else if (SelectedScanType == ScanType.PO)
                    {
                        Settings.POsPagesPerDocument = value;
                    }
                    else
                    {
                        Settings.ServicePagesPerDocument = value;
                    }
                    OnPropertyChanged(nameof(PagesPerDocument));
                }
            }
        }
        [ObservableProperty]
        private Settings _settings = new Settings();
        public string MainFolder
        {
            get
            {
                return Settings.MainFolder;
            }
            set
            {
                if (Settings.MainFolder != value)
                {
                    Settings.MainFolder = value;
                    OnPropertyChanged(nameof(MainFolder));
                }
            }
        }
        public string DeliveriesFolder
        {
            get
            {
                return Settings.DeliveriesFolder;
            }
            set
            {
                if (Settings.DeliveriesFolder != value)
                {
                    Settings.DeliveriesFolder = value;
                    OnPropertyChanged(nameof(DeliveriesFolder));
                }
            }
        }
        public string RMAsFolder
        {
            get
            {
                return Settings.RMAsFolder;
            }
            set
            {
                if (Settings.RMAsFolder != value)
                {
                    Settings.RMAsFolder = value;
                    OnPropertyChanged(nameof(MoverViewModel.RMAsFolder));
                }
            }
        }
        public string ShippingLogsFolder
        {
            get
            {
                return Settings.ShippingLogsFolder;
            }
            set
            {
                if (Settings.ShippingLogsFolder != value)
                {
                    Settings.ShippingLogsFolder = value;
                    OnPropertyChanged(nameof(MoverViewModel.ShippingLogsFolder));
                }
            }
        }
        public string ServiceFolder
        {
            get
            {
                return Settings.ServiceFolder;
            }
            set
            {
                if (Settings.ServiceFolder != value)
                {
                    Settings.ServiceFolder = value;
                    OnPropertyChanged(nameof(MoverViewModel.ServiceFolder));
                }
            }
        }
        public double Tolerance
        {
            get
            {
                return Settings.Tolerance;
            }
            set
            {
                if (Settings.Tolerance != value)
                {
                    Settings.Tolerance = value;
                    OnPropertyChanged(nameof(Tolerance));
                }
            }
        }
        public double DocumentMinimum
        {
            get
            {
                if (SelectedScanType == ScanType.RMA)
                {
                    return Settings.RMAsMinimum;
                }
                else
                {
                    return Settings.DeliveriesMinimum;
                }
            }
            set
            {
                if (DocumentMinimum != value)
                {
                    if (SelectedScanType == ScanType.Delivery)
                    {
                        Settings.DeliveriesMinimum = value;
                    }
                    else if (SelectedScanType == ScanType.RMA)
                    {
                        Settings.RMAsMinimum = value;
                    }
                    OnPropertyChanged(nameof(MoverViewModel.DocumentMinimum));
                }

            }
        }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PrefixText))]
        [NotifyPropertyChangedFor(nameof(PrefixExample))]
        [NotifyPropertyChangedFor(nameof(DocumentHasDate))]
        [NotifyPropertyChangedFor(nameof(DocumentHasMinimum))]
        [NotifyPropertyChangedFor(nameof(PagesPerDocument))]
        private ScanType _selectedScanType = ScanType.Delivery;
        [ObservableProperty]
        private bool _isBusy = false;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DocumentMinimum))]
        private bool _documentHasMinimum = true;
        [ObservableProperty]
        private bool _documentHasDate = true;
        [ObservableProperty]
        private LocationType _locType = LocationType.Scans;
        public string PrefixText
        {
            get
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
            set
            {
                if (PrefixText != value)
                {
                    if (SelectedScanType == ScanType.Delivery)
                    {
                        Settings.DeliveriesPrefix = value;
                    }
                    else if (SelectedScanType == ScanType.Shipping)
                    {
                        Settings.ShippingLogsPrefix = value;
                    }
                    else if (SelectedScanType == ScanType.RMA)
                    {
                        Settings.RMAsPrefix = value;
                    }
                    else if (SelectedScanType == ScanType.PO)
                    {
                        Settings.POsPrefix = value;
                    }
                    else
                    {
                        Settings.ServicePrefix = value;
                    }
                    OnPropertyChanged(nameof(MoverViewModel.PrefixText));
                    OnPropertyChanged(nameof(MoverViewModel.PrefixExample));
                }

            }
        }
        private string PrefixExample
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PrefixText))
                {
                    return PrefixText + " batch";
                }
                else
                {
                    return string.Empty;
                }

            }
        }
        [ObservableProperty]
        private DateTime _specifiedDate = DateTime.Now.AddDays(-1);
        public string CurrentPrefix
        {
            get
            {
                if (SelectedScanType == ScanType.Delivery)
                {
                    return Settings.DeliveriesPrefix;
                }
                else if (SelectedScanType == ScanType.RMA)
                {
                    return Settings.RMAsPrefix;
                }
                else if (SelectedScanType == ScanType.Shipping)
                {
                    return Settings.ShippingLogsPrefix;
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
        }
        #endregion

        #region Variables
        private readonly string settingsFile = "settings.json";
        private readonly Window _parentWindow;
        #endregion

        #region Commands
        public ICommand BatchSplitCommand { get; }
        public ICommand MoveDeliveriesCommand { get; }
        public ICommand ChangeLocationCommand { get; }
        public ICommand SplitTypeCheckedCommand { get; }
        public ICommand LocationCheckedCommand { get; }
        #endregion

        public MoverViewModel(Window parentWindow)
        {
            _parentWindow = parentWindow;

            BatchSplitCommand = new BatchSplitCommand(_parentWindow, this);
            MoveDeliveriesCommand = new MoveDocumentsCommand(this, parentWindow);
            ChangeLocationCommand = new ChangeLocationCommand(parentWindow, this);
            SplitTypeCheckedCommand = new SplitTypeCheckedCommand(this);
            LocationCheckedCommand = new LocationCheckedCommand(this);

            PropertyChanged += OnPropertyChange;

            _parentWindow.Opened += OnWindowOpened;
            _parentWindow.Closing += OnWindowClosing;

        }

        /// <summary>
        /// Loads the program settings.
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            Settings = await FileAccessService.LoadSettingsAsync(settingsFile);
            OnPropertyChanged(nameof(MoverViewModel.Tolerance));
            OnPropertyChanged(nameof(MoverViewModel.DeliveriesFolder));
            OnPropertyChanged(nameof(MoverViewModel.MainFolder));
            OnPropertyChanged(nameof(MoverViewModel.RMAsFolder));
            OnPropertyChanged(nameof(MoverViewModel.ServiceFolder));
            OnPropertyChanged(nameof(MoverViewModel.ShippingLogsFolder));
            OnPropertyChanged(nameof(MoverViewModel.PrefixText));
            OnPropertyChanged(nameof(MoverViewModel.PrefixExample));
            UpdateInfo();
        }

        /// <summary>
        /// Saves the current program settings.
        /// </summary>
        /// <returns>If the operation succeeded.</returns>
        public async Task<bool> SaveSettingsAsync()
        {
            return await FileAccessService.SaveSettingsAsync(Settings, settingsFile);
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
            await SaveSettingsAsync();
        }

        /// <summary>
        /// Watches for property changes on the MoverViewModel.
        /// </summary>
        /// <param name="sender">Sender of the change.</param>
        /// <param name="e">Arguments of the change.</param>
        private void OnPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MoverViewModel.SelectedScanType))
            {
                UpdateInfo();
            }
        }

        #region UpdateMethods
        private void UpdateInfo()
        {
            UpdateHasDate();
            UpdateHasMinimum();
            UpdatePagesPerDocument();
        }

        /// <summary>
        /// Updates the document minimum.
        /// </summary>
        private void UpdateMinimum()
        {
            if (SelectedScanType == ScanType.Delivery)
            {
                DocumentMinimum = Settings.DeliveriesMinimum;
            }
            else if (SelectedScanType == ScanType.RMA)
            {
                DocumentMinimum = Settings.RMAsMinimum;
            }
        }

        /// <summary>
        /// updates the document has minimum for a specified scan type.
        /// </summary>
        /// <param name="currentType">The scan type</param>
        private void UpdateHasMinimum()
        {
            if (SelectedScanType == ScanType.Delivery || SelectedScanType == ScanType.RMA)
            {
                DocumentHasMinimum = true;
                UpdateMinimum();
            }
            else
            {
                DocumentHasMinimum = false;
            }
        }

        /// <summary>
        /// Updates the pages per document.
        /// </summary>
        private void UpdatePagesPerDocument()
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
        /// Updates the document has date.
        /// </summary>
        private void UpdateHasDate()
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
        #endregion
    }
}

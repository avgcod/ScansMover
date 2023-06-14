using Scans_Mover.Models;
using System;
using ReactiveUI;
using Scans_Mover.Services;
using System.Windows.Input;
using Scans_Mover.Commands;
using Avalonia.Controls;
using System.ComponentModel;
using System.Data.Common;

namespace Scans_Mover.ViewModels
{
    public class FileRenameViewModel : ViewModelBase
    {
        #region Variables
        private readonly Window _currentWindow;
        private readonly string _fileName;
        #endregion

        #region Properties
        private string _fileInfo = string.Empty;
        public string FileInfo
        {
            get
            {
                return _fileInfo;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _fileInfo, value);
            }
        }

        private string _prefixText = string.Empty;

        private string _typeText = "Delivery";
        public string TypeText
        {
            get
            {
                return _typeText;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _typeText, value);
            }
        }

        private string _exampleText = string.Empty;
        public string ExampleText
        {
            get
            {
                return _exampleText;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _exampleText, value);
            }
        }

        private string _newFileName = string.Empty;
        public string NewFileName
        {
            get
            {
                return _newFileName;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _newFileName, value);                
            }
        }

        private string _callNum = string.Empty;
        public string CallNum
        {
            get
            {
                return _callNum;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _callNum, value);
                UpdateExampleText();
            }
        }

        private bool _isService = false;
        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _isService, value);
            }
        }

        private ScanType _currentType;
        public ScanType CurrentType
        {
            get
            {
                return _currentType;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _currentType, value);
            }
        }
        public ScanStatus Response { get; set; }

        private decimal nameLength;
        public decimal NameLength
        {
            get
            {
                return nameLength;
            }
            set
            {
                nameLength = value;
                this.RaiseAndSetIfChanged(ref nameLength, value);
            }
        }

        private string _watermark = string.Empty;
        public string Watermark
        {
            get
            {
                return _watermark;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _watermark, value);
            }
        }
        #endregion

        #region Commands
        public ICommand RenameCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        public FileRenameViewModel(Window currentWindow, string fileName, ScanType tempType, double numLength, string prefixText)
        {
            _fileName = fileName;
            _currentWindow = currentWindow;

            RenameCommand = new RenameCommand(_currentWindow, this);
            SkipCommand = new SkipCommand(_currentWindow, this);
            CancelCommand = new CancelCommand(_currentWindow, this);

            PropertyChanged += OnPropertyChange;

            _currentWindow.Opened += WindowOpened;
            _currentWindow.Closing += OnWindowClosing;

            CurrentType = tempType;
            _prefixText = prefixText;

            if (CurrentType == ScanType.Service)
            {
                IsService = true;
            }

            SetNameLength(numLength);
        }

        private void SetNameLength(double numLength)
        {
            if (CurrentType == ScanType.Shipping)
            {
                NameLength = 7;
                Watermark = "ddMMMyy";
            }
            else
            {
                if (CurrentType == ScanType.Service)
                {
                    NameLength = (numLength + 1).ToString().Length;
                }
                NameLength = numLength.ToString().Length;
                for (int i = 0; i < NameLength; i++)
                {
                    Watermark += "#";
                }

            }
        }

        public void WindowOpened(object? sender, EventArgs e)
        {
            FileAccessService.LoadDefaultApplication(_fileName);
            _currentWindow.FindControl<TextBox>("tbxFileName")?.Focus();
        }

        public void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (CurrentType == ScanType.Shipping && Response == ScanStatus.OK)
            {
                string preFormatName = NewFileName;
                NewFileName = preFormatName[0].ToString() + preFormatName[1].ToString() + "-" +
                preFormatName[2].ToString() + preFormatName[3].ToString() + preFormatName[4].ToString() + "-" +
                preFormatName[5].ToString() + preFormatName[6].ToString();
            }
            else if (CurrentType == ScanType.Service && Response == ScanStatus.OK)
            {
                NewFileName = NewFileName + "_Call " + CallNum;
            }

            _currentWindow.Opened -= WindowOpened;
            _currentWindow.Closing -= OnWindowClosing;
        }

        /// <summary>
        /// Updates the text example.
        /// </summary>
        public void UpdateExampleText()
        {
            if (!string.IsNullOrWhiteSpace(NewFileName))
            {
                if (CallNum != string.Empty && int.TryParse(CallNum,out int i) && i > 0)
                {
                    ExampleText = _prefixText + NewFileName + "_Call " + CallNum;
                }
                else
                {
                    ExampleText = _prefixText + NewFileName;
                }

            }
            else
            {
                ExampleText = string.Empty;
            }
        }

        /// <summary>
        /// Watches for property changes on the MoverViewModel.
        /// </summary>
        /// <param name="sender">Sender of the change.</param>
        /// <param name="e">Arguments of the change.</param>
        private void OnPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileRenameViewModel.NewFileName) || e.PropertyName == nameof(FileRenameViewModel.CallNum))
            {
                UpdateExampleText();
            }
        }

        protected override void Dispose()
        {
            _currentWindow.Opened -= WindowOpened;
            _currentWindow.Closing -= OnWindowClosing;
            base.Dispose();
        }
    }
}

using Scans_Mover.Models;
using System;
using Scans_Mover.Services;
using Avalonia.Controls;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Collections;

namespace Scans_Mover.ViewModels
{
    public partial class FileRenameViewModel : ViewModelBase
    {
        #region Variables
        private readonly Window _currentWindow;
        private readonly string _fileName;
        private readonly double _numLength;
        #endregion

        #region Properties

        [ObservableProperty]
        private string _fileInfo = string.Empty;

        private readonly string _prefixText = string.Empty;

        [ObservableProperty]
        private string _typeText = "Delivery";

        [ObservableProperty]
        private string _exampleText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        private string _newFileName = string.Empty;

        //TODO: UpdateExampleText
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        private string _callNum = string.Empty;

        [ObservableProperty]
        private bool _isService = false;

        [ObservableProperty]
        private ScanType _currentType;
        public ScanStatus Response { get; set; }

        [ObservableProperty]
        private decimal _nameLength;

        [ObservableProperty]
        private string _watermark = string.Empty;
        #endregion

        public FileRenameViewModel(Window currentWindow, string fileName, ScanType tempType, double numLength, string prefixText, IMessenger theMessenger) : base(theMessenger)
        {
            _fileName = fileName;
            _currentWindow = currentWindow;
            _numLength = numLength;

            CurrentType = tempType;
            TypeText = CurrentType.ToString();
            _prefixText = prefixText;

            IsActive = true;
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

        #region Commands

        public bool CanRename()
        {
            if (CurrentType == ScanType.Service)
            {
                return decimal.TryParse(NewFileName, out _)
                    && NewFileName.Length >= NameLength
                    && int.TryParse(CallNum, out int i)
                    && i > 0;
            }
            else if (CurrentType != ScanType.Shipping)
            {
                return decimal.TryParse(NewFileName, out _)
                    && NewFileName.Length >= NameLength;
            }
            else
            {
                return DateTime.TryParse(NewFileName, out _)
                    && NewFileName.Length >= NameLength;
            }
        }

        [RelayCommand(CanExecute = nameof(CanRename))]
        public void Rename()
        {
            if (CurrentType == ScanType.Shipping)
            {
                string preFormatName = NewFileName;
                NewFileName = preFormatName[0].ToString() + preFormatName[1].ToString() + "-" +
                preFormatName[2].ToString() + preFormatName[3].ToString() + preFormatName[4].ToString() + "-" +
                preFormatName[5].ToString() + preFormatName[6].ToString();
            }
            else if (CurrentType == ScanType.Service)
            {
                NewFileName = NewFileName + "_Call " + CallNum;
            }
            Messenger.Send(new RenameMessage(ScanStatus.OK, NewFileName));
            _currentWindow.Close();
        }

        [RelayCommand]
        public void Skip()
        {
            Messenger.Send(new RenameMessage(ScanStatus.Skip, string.Empty));
            _currentWindow.Close();
        }

        [RelayCommand]
        public void Cancel()
        {
            Messenger.Send(new RenameMessage(ScanStatus.Cancel, string.Empty));
            _currentWindow.Close();
        }
        #endregion

        protected override async void OnActivated()
        {
            await FileAccessService.LoadDefaultApplicationAsync(_fileName, Messenger);
            _currentWindow.FindControl<TextBox>("tbxFileName")?.Focus();

            if (CurrentType == ScanType.Service)
            {
                IsService = true;
                TypeText = "Delivery";
            }

            SetNameLength(_numLength);

            base.OnActivated();
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

        partial void OnNewFileNameChanged(string value)
        {
            UpdateExampleText();
        }

        partial void OnCallNumChanged(string value)
        {
            UpdateExampleText();
        }
    }
}

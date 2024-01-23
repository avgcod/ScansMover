using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;
using Scans_Mover.ViewModels;
using System;
using System.ComponentModel;

namespace Scans_Mover.Commands
{
    public class RenameCommand : CommandBase
    {
        private readonly FileRenameViewModel _frViewModel;
        private readonly Window _currentWindow;
        private readonly IMessenger _theMessenger;
        public RenameCommand(Window currentWindow, FileRenameViewModel frViewModel, IMessenger theMessenger)
        {
            _currentWindow = currentWindow;
            _frViewModel = frViewModel;
            _theMessenger = theMessenger;

            _frViewModel.PropertyChanged += OnViewModelPropertChanged;
        }

        public override bool CanExecute(object? parameter)
        {
            if (_frViewModel.CurrentType == ScanType.Service)
            {
                return decimal.TryParse(_frViewModel.NewFileName, out _)
                    && _frViewModel.NewFileName.Length >= _frViewModel.NameLength
                    && int.TryParse(_frViewModel.CallNum, out int i)
                    && i > 0
                    && base.CanExecute(parameter);
            }
            else if (_frViewModel.CurrentType != ScanType.Shipping)
            {
                return decimal.TryParse(_frViewModel.NewFileName, out _)
                    && _frViewModel.NewFileName.Length >= _frViewModel.NameLength
                    && base.CanExecute(parameter);
            }
            else
            {
                return DateTime.TryParse(_frViewModel.NewFileName, out _)
                    && _frViewModel.NewFileName.Length >= _frViewModel.NameLength
                    && base.CanExecute(parameter);
            }
        }
        public override void Execute(object? parameter)
        {
            if (_frViewModel.CurrentType == ScanType.Shipping)
            {
                string preFormatName = _frViewModel.NewFileName;
                _frViewModel.NewFileName = preFormatName[0].ToString() + preFormatName[1].ToString() + "-" +
                preFormatName[2].ToString() + preFormatName[3].ToString() + preFormatName[4].ToString() + "-" +
                preFormatName[5].ToString() + preFormatName[6].ToString();
            }
            else if (_frViewModel.CurrentType == ScanType.Service)
            {
                _frViewModel.NewFileName = _frViewModel.NewFileName + "_Call " + _frViewModel.CallNum;
            }
            _theMessenger.Send(new RenameMessage(ScanStatus.OK, _frViewModel.NewFileName));
            _currentWindow.Close();
        }

        private void OnViewModelPropertChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileRenameViewModel.NewFileName) || e.PropertyName == nameof(FileRenameViewModel.CallNum))
            {
                OnCanExecutedChanged();
            }
        }
    }
}

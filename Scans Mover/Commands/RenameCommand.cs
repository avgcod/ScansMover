using Avalonia.Controls;
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
        public RenameCommand(Window currentWindow, FileRenameViewModel frViewModel)
        {
            _currentWindow = currentWindow;
            _frViewModel = frViewModel;
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
            _frViewModel.Response = ScanStatus.OK;
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

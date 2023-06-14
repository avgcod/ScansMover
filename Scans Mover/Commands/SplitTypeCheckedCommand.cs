using Scans_Mover.Models;
using Scans_Mover.ViewModels;
using System;

namespace Scans_Mover.Commands
{
    public class SplitTypeCheckedCommand : CommandBase
    {
        private readonly MoverViewModel _moverViewModel;

        public SplitTypeCheckedCommand(MoverViewModel moverViewModel)
        {
            _moverViewModel = moverViewModel;
        }

        public override bool CanExecute(object? parameter)
        {
            return base.CanExecute(parameter);
        }
        public override void Execute(object? parameter)
        {
            string typeText = parameter?.ToString() ?? string.Empty;
            if(!string.IsNullOrWhiteSpace(typeText))
            {
                _moverViewModel.SelectedScanType = (ScanType)Enum.Parse(typeof(ScanType), typeText);
            }            
        }
    }
}

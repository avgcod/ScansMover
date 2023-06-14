using Scans_Mover.Models;
using Scans_Mover.ViewModels;
using System;

namespace Scans_Mover.Commands
{
    public class LocationCheckedCommand : CommandBase
    {
        private readonly MoverViewModel _moverViewModel;

        public LocationCheckedCommand(MoverViewModel moverViewModel)
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
            if (!string.IsNullOrWhiteSpace(typeText))
            {
                _moverViewModel.LocType = (LocationType)Enum.Parse(typeof(LocationType),typeText);
            }
        }
    }
}

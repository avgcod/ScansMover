using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scans_Mover.ViewModels
{
    public partial class DetailsTabViewModel(IMessenger theMessenger) : ViewModelBase(theMessenger)
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DocumentMinimum))]
        private bool _documentHasMinimum = false;

        [ObservableProperty]
        public double _documentMinimum = 1;

        [ObservableProperty]
        public int _pagesPerDocument = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PrefixExample))]
        public string _prefix = string.Empty;

        public string PrefixExample => Prefix + " batch";

        [ObservableProperty]
        public double _tolerance = 75;
    }
}

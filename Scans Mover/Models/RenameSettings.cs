using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scans_Mover.Models
{
    public class RenameSettings
    {
        public ScanType SelectedScanType { get; set; }
        public double DocumentMinimum { get; set; } = 3070200;
        public string Prefix { get; set; } = string.Empty;
    }
}

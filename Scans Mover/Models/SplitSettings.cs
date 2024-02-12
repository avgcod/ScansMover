using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scans_Mover.Models
{
    public class SplitSettings
    {
        public int PagesPerDocument { get; set; } = 1;
        public bool DocumentHasMinimum { get; set; } = true;
        public double DocumentMinimum { get; set; } = 3070200;
        public double Tolerance { get; set; } = 150;
        public string MainFolder { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public ScanType SelectedScanType { get; set; }
    }
}

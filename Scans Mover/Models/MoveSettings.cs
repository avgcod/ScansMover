using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scans_Mover.Models
{
    public class MoveSettings
    {
        public string MainFolder { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string RootDestination { get; set; } = string.Empty;
        public ScanType SelectedScanType { get; set; }
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}

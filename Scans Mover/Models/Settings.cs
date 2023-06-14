namespace Scans_Mover.Models
{
    public class Settings
    {
        public string MainFolder { get; set; } = string.Empty;
        public string DeliveriesFolder { get; set; } = string.Empty;
        public string RMAsFolder { get; set; } = string.Empty;
        public string ShippingLogsFolder { get; set; } = string.Empty;
        public string ServiceFolder { get; set; } = string.Empty;
        public string DeliveriesPrefix { get; set; } = string.Empty;
        public string RMAsPrefix { get; set; } = string.Empty;
        public string ShippingLogsPrefix { get; set; } = string.Empty;
        public string POsPrefix { get; set; } = string.Empty;
        public string ServicePrefix { get; set; } = string.Empty;
        public int DeliveriesPagesPerDocument { get; set; } = 1;
        public int RMAsPagesPerDocument { get; set; } = 1;
        public int ShippingLogsPagesPerDocument { get; set; } = 2;
        public int POsPagesPerDocument { get; set; } = 1;
        public int ServicePagesPerDocument { get; set; } = 1;
        public double Tolerance { get; set; } = 75;
        public double DeliveriesMinimum { get; set; } = 3047000;
        public double RMAsMinimum { get; set; } = 20100;
    }
}

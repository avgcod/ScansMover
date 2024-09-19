namespace Scans_Mover.Models
{
    /// <summary>
    /// Enum of the possible scan types.
    /// </summary>
    public enum ScanType
    {
        Delivery, RMA, Shipping, PO, Service
    }

    public enum LocationType
    {
        Scans, Deliveries, Shipping, RMAs, Service
    }

    public enum ScanStatus
    {
        OK, Skip, Cancel
    }
}

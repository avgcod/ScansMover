using System.Collections.Generic;

namespace Scans_Mover.Models
{
    public record class RenameMessage(ScanStatus ScanStatus, string NewFileName);

    public record class MoveLogMessage(IEnumerable<string> LogInfo, string LogFile);

    public record class PagesPerDocumentErrorMessage(string Info);

    public record class OperationErrorMessage(string ErrorType, string ErrorMessage);

    public record class OperationErrorInfoMessage(string ErrorType, string ErrorMessage);

    public record class NotificationMessage(string MessageText);

    public record class LocationsMessage(string MainFolder, string DeliveriesFolder, string RMAsFolder, string ServiceFolder, string ShippingLogsFolder);

    public record class PrefixMessage(string Prefix);
    public record class SkippedFileMessage();
}

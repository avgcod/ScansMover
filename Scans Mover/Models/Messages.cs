using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Scans_Mover.Services;
using System.Collections.Generic;

namespace Scans_Mover.Models
{
    public record class RenameMessage(ScanStatus ScanStatus, string NewFileName);
    public record class MoveLogMessage(IEnumerable<string> LogInfo, string LogFile);
    public record class PagesPerDocumentErrorMessage(string Info);
    public record class OperationErrorMessage(string ErrorType, string ErrorMessage);
    public record class OperationErrorInfoMessage(string ErrorType, string ErrorMessage);
    public record class NotificationMessage(string MessageText);
    public record class PrefixMessage(string Prefix);
    public record class PagesPerDocumentMessage(int PagesPerDocument);
    public record class ToleranceMessage(double Tolerance);
    public record class SkippedFileMessage();
    public record class ScanTypeChangedMessage();
    public record class DocumentMinimumMessage(double DocumentMinimum);
    public record class MainFolderMessage(string MainFolder);
    public record class DeliveriesFolderMessage(string DeliveriesFolder);
    public record class RMAsFolderMessage(string RMAsFolder);
    public record class ShippingLogsFolderMessage(string ShippingLogsFolder);
    public record class ServiceFolderMessage(string ServiceFolder);
    public record class BusyStatusMessage(bool Busy);

    #region Request Messages
    public class PrefixRequestMessage : AsyncRequestMessage<string>;
    public class DocumentHasDateRequestMessage : AsyncRequestMessage<bool>;
    public class DocumentHasMinimumRequestMessage : AsyncRequestMessage<bool>;
    public class DocumentMinimumRequestMessage : AsyncRequestMessage<double>;
    public class ToleranceRequestMessage : AsyncRequestMessage<double>;
    public class MainFolderRequestMessage : AsyncRequestMessage<string>;
    public class DestinationFolderRequestMessage : AsyncRequestMessage<string>;
    public class DeliveriesFolderRequestMessage : AsyncRequestMessage<string>;
    public class RMAsFolderRequestMessage : AsyncRequestMessage<string>;
    public class ShippingLogsFolderRequestMessage : AsyncRequestMessage<string>;
    public class ServiceFolderRequestMessage : AsyncRequestMessage<string>;
    public class PagesPerDocumentRequestMessage : AsyncRequestMessage<int>;
    public class SelectedScanTypeRequestMessage : AsyncRequestMessage<ScanType>;
    public class RenameServiceRequestMessage : AsyncRequestMessage<FileRenameService>;
    public class ParentWindowRequestMessage : AsyncRequestMessage<Window>;
    public class MessengerRequestMessage : AsyncRequestMessage<IMessenger>;
    #endregion
}

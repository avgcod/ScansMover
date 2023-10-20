using System.Collections.Generic;

namespace Scans_Mover.Models
{
    public record class RenameMessage(ScanStatus ScanStatus, string NewFileName);

    public record class MoveLogMessage(IEnumerable<string> LogInfo, string LogFile);

    public record class PagesPerDocumentErrorMessage(string Info);

    public record class OperationErrorMessage(string ErrorType, string ErrorMessage);

}

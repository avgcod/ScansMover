using System.Collections.Generic;

namespace Scans_Mover.Models
{
    public record class RenameMessage(ScanStatus scanStatus, string newFileName);

    public record class LogMessage(IEnumerable<string> logInfo, string logFile);

    public record class PagesPerDocumentErrorMessage(string info);
}
